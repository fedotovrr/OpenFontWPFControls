using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using OpenFontWPFControls.Layout;

namespace OpenFontWPFControls.Controls
{
    internal partial class TextVisualHost
    {
        private bool _drawingValid;
        private Rect _drawingBounds;

        private double ControlLayerOpacity => _controlLayerVisible ? SelectionAny ? 0.4 : 1 : 0;

        //protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi) => Invalidate();

        //protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo) => Invalidate();

        public void Invalidate()
        {
            Render();
            UpdateViewSize();
            InvalidateMeasure();
            InvalidateVisual();
        }

        public void InvalidateDrawing()
        {
            _drawingValid = false;
            UpdateViewSize();
            InvalidateMeasure();
            InvalidateVisual();
        }


        protected override Size MeasureOverride(Size constraint)
        {
            ((Caret)_caret.Children[0]).Measure(constraint);

            _maxSize = new Size(Math.Min(double.MaxValue, constraint.Width), Math.Min(double.MaxValue, constraint.Height));
            if (_layout == null || Math.Abs(_layout.MaxWidth - _maxSize.Width) > double.Epsilon)
            {
                Render();
                UpdateViewSize();
            }
            
            return new Size(_viewWidth, _viewHeight);
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            arrangeSize = new Size(_viewWidth, _viewHeight);
            Rect arrangeRect = new Rect(0, 0, arrangeSize.Width, arrangeSize.Height);
            ((Caret)_caret.Children[0]).Arrange(arrangeRect);
            Draw();
            return arrangeSize;
        }


        // Rendering

        private void Render()
        {
            _drawMaxWidth = 0;
            DpiScale scale = VisualTreeHelper.GetDpi(this);
            double size = _parentControl?.FontSize ?? SystemFonts.MessageFontSize;
            string text = _parentControl?.GetValue(_textProperty) as string ?? String.Empty;
            TextTrimming trimming = _parentControl?.GetValue(BaseTextControl.TextTrimmingProperty) is TextTrimming t ? t : default;
            Brush brush = _parentControl?.Foreground;
            string font = _parentControl?.FontFamily?.FamilyNames?.Values?.FirstOrDefault() ?? TypefaceInfo.DefaultFontFamily;
            string extension = _parentControl?.GetValue(BaseTextControl.FontExtensionProperty) as string;
            FontStyle style = _parentControl?.FontStyle ?? default;
            FontWeight weight = _parentControl?.FontWeight ?? default;
            bool underline = _parentControl?.GetValue(BaseTextControl.UnderlineProperty) is bool u && u;
            bool strike = _parentControl?.GetValue(BaseTextControl.StrikeProperty) is bool s && s;
            _spellCheck = _parentControl?.GetValue(TextBox.SpellCheckProperty) is bool sc && sc;

            if (_typefaceInfo == null ||
                style != _typefaceInfo.Style ||
                weight != _typefaceInfo.Weight ||
                !string.Equals(font, _typefaceInfo.TypefaceName, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(extension, _typefaceInfo.ExtensionName, StringComparison.OrdinalIgnoreCase))
            {
                _typefaceInfo = null;
                _typefaceInfo = TypefaceInfo.CacheGetOrTryCreate(font, style, weight, extension);
                _layout = null;
                _layout = new TextLayout(
                    text,
                    _typefaceInfo,
                    (float)_maxSize.Width,
                    (float)size,
                    (float)scale.PixelsPerDip,
                    (float)scale.PixelsPerInchX,
                    brush,
                    underline,
                    strike);
            }
            else
            {
                _layout.Text = text;
                _layout.TextTrimming = trimming;
                _layout.MaxWidth = (float)_maxSize.Width;
                _layout.FontSize = (float)size;
                _layout.PixelsPerDip = (float)scale.PixelsPerDip;
                _layout.PixelsPerInch = (float)scale.PixelsPerInchX;
                _layout.Foreground = brush;
                _layout.Underline = underline;
                _layout.Strike = strike;
            }

            _caretPoint.Owner = CaretPointOwners.Anyone;
            _caretPoint = _layout.CaretPointContains(_caretPoint.CharOffset) ? _caretPoint : _layout.LastCaretPoint;
            _selectionCapture = _caretPoint;
            SetStartChar(_startCharOffset, false);
            _drawingValid = false;

            OnRenderCallBack?.Invoke();
        }

        private void UpdateViewSize()
        {
            _viewWidth = 0;
            _viewHeight = 0;
            foreach ((TextLine line, double y) in ViewLine())
            {
                _viewWidth = Math.Max(_viewWidth, line.Width);
                _viewHeight = y + line.Height;
            }
            _viewWidth += ((Caret)_caret.Children[0]).Width;
            _drawMaxWidth = Math.Max(_drawMaxWidth, _viewWidth);
        }


        // Drawing

        private void UpdateCaretVisible()
        {
            ((Caret)_caret.Children[0]).Visibility = _controlLayerVisible && _caretVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public void SetControlLayerVisibility(bool value)
        {
            if (_controlLayerVisible != value)
            {
                _controlLayerVisible = value;
                if (_controlLayer is DrawingVisual drawing)
                {
                    drawing.Opacity = ControlLayerOpacity;
                }

                UpdateCaretVisible();
            }
        }


        private void Draw()
        {
            if (!_drawingValid)
            {
                _drawingBounds = _parentControl?.GetValue(BaseTextControl.DrawingBoundsProperty) is Rect r ? r :
                    new Rect(0, 0, double.PositiveInfinity, double.PositiveInfinity);
                UpdateErrorLayer();
                UpdateControlLayer();
                UpdateContentLayer();
                UpdateBackgroundLayer();
                _drawingValid = true;
                OnDrawCallBack?.Invoke();
            }
        }

        private void UpdateBackgroundLayer()
        {
            DrawingVisual drawing = new DrawingVisual();
            DrawingContext context = drawing.RenderOpen();
            context.DrawRectangle(Brushes.Transparent, null, new Rect(0, 0, Math.Round(_viewWidth), Math.Round(_viewHeight)));
            context.Close();
            drawing.Offset = new Vector(_viewXOffset, 0);
            RemoveVisualChild(_backgroundLayer);
            AddVisualChild(_backgroundLayer = drawing);
        }

        private void UpdateContentLayer()
        {
            _linesLayer.ForEach(child => child.Valid = false);

            foreach ((TextLine line, double y) in LineInViewBounds())
            {
                ContainerDrawing container = _linesLayer.FirstOrDefault(c => c.Source == line && line.Paragraph.Valid);
                if (container != null)
                {
                    container.Valid = true;
                    container.Visual.Offset = new Vector(_viewXOffset, y);
                }
                else
                {
                    container = new ContainerDrawing(line);
                    container.Visual.Offset = new Vector(_viewXOffset, y);
                    _linesLayer.Add(container);
                    AddVisualChild(container.Visual);
                }
            }

            int count = 0;
            for (int i = _linesLayer.Count - 1; i >= 0; i--)
            {
                if (!_linesLayer[i].Valid)
                {
                    count++;
                    RemoveVisualChild(_linesLayer[i].Visual);
                }
                else if (count > 0)
                {
                    _linesLayer.RemoveRange(i + 1, count);
                    count = 0;
                }
            }
            if (count > 0)
            {
                _linesLayer.RemoveRange(0, count);
            }
        }

        public void UpdateErrorLayer()
        {
            DrawingVisual drawing = new DrawingVisual();
            DrawingContext context = drawing.RenderOpen();
            if (_spellCheck)
            {
                DrawErrors(context);
            }
            context.Close();
            drawing.Offset = new Vector(_viewXOffset, 0);
            RemoveVisualChild(_errorsLayer);
            AddVisualChild(_errorsLayer = drawing);
        }

        public void UpdateControlLayer()
        {
            DrawingVisual drawing = new DrawingVisual { Opacity = ControlLayerOpacity };
            DrawingContext context = drawing.RenderOpen();
            
            if (_layout?.CaretPoints.Any() == true)
            {
                if (SelectionAny)
                {
                    DrawSelection(context, _parentControl?.GetValue(TextBox.SelectionBrushProperty) as Brush ?? Brushes.CornflowerBlue);
                }
                DrawCaret(context, _parentControl?.GetValue(TextBox.CaretBrushProperty) as Brush ?? Brushes.Black);
                UpdateCaretVisible();
            }

            context.Close();
            drawing.Offset = new Vector(_viewXOffset, 0);
            RemoveVisualChild(_controlLayer);
            AddVisualChild(_controlLayer = drawing);
        }

        private void DrawSelection(DrawingContext context, Brush brush)
        {
            int selectionStart = SelectionStart;
            int selectionEnd = SelectionEnd;
            CaretPoint start, end;
            foreach ((TextLine line, double y) in LineInViewBounds())
            {
                start = end = line.CaretPoints.First();
                foreach (CaretPoint current in line.CaretPoints)
                {
                    if (current.CharOffset <= selectionStart)
                    {
                        start = end = current;
                    }
                    else if (current.Owner == CaretPointOwners.EndLine)
                    {
                        Rect rect = new Rect(
                            new Point(Math.Round(start.X), Math.Round(y)),
                            new Point(Math.Round(current.X), Math.Round(y + line.Height)));
                        context.DrawRectangle(brush, null, rect);
                    }
                    else if (current.CharOffset >= selectionEnd)
                    {
                        end = current.CharOffset == selectionEnd ? current : end;
                        Rect rect = new Rect(
                            new Point(Math.Round(start.X), Math.Round(y)),
                            new Point(Math.Round(end.X), Math.Round(y + line.Height)));
                        context.DrawRectangle(brush, null, rect);
                        break;
                    }
                    else
                    {
                        end = current;
                    }
                }
            }
        }

        private void DrawCaret(DrawingContext context, Brush brush)
        {
            Caret caret = (Caret)_caret.Children[0];
            foreach ((TextLine line, double y) in LineInViewBounds())
            {
                if (line.CaretPointContains(_caretPoint.CharOffset))
                {
                    foreach (CaretPoint current in line.CaretPoints)
                    {
                        if (current.Equals(_caretPoint))
                        {
                            //_caretPoint = current;
                            //Rect rect = new Rect(current.X - 1, y, 1, line.Height);
                            //GuidelineSet guidelines = new GuidelineSet();
                            //guidelines.GuidelinesX.Add(rect.X);
                            //context.PushGuidelineSet(guidelines);
                            //context.DrawRectangle(brush, null, rect);

                            caret.Height = line.Height;
                            caret.CaretBrush = brush;
                            caret.Margin = new Thickness(current.X, y, 0, 0);
                            _caretVisible = true;

                            return;
                        }
                    }
                }
            }
            _caretVisible = false;
        }

        private void DrawErrors(DrawingContext context)
        {
            if (CharIsError != null)
            {
                foreach ((TextLine line, double y) in LineInViewBounds())
                {
                    bool sl = false;
                    float x1 = 0;
                    float x2 = 0;
                    double size = _layout.TypefaceInfo.DefaultBuilder.GetPixelUnderlineSize(_layout.FontSize);
                    double offset = y + _layout.TypefaceInfo.DefaultBuilder.GetPixelBaseline(_layout.FontSize) + _layout.TypefaceInfo.DefaultBuilder.GetPixelUnderlinePosition(_layout.FontSize) * 2 + size;

                    foreach ((GlyphPoint glyph, float x) in line.GlyphPoints)
                    {
                        if (CharIsError(line.Paragraph.CharOffset + glyph.CharOffset))
                        {
                            if (!sl)
                            {
                                sl = true;
                                x1 = x;
                            }
                            x2 = x + glyph.GetPixelWidth(_layout.FontSize);
                        }
                        else if (sl)
                        {
                            DrawRect();
                        }
                    }

                    if (sl)
                    {
                        DrawRect();
                    }

                    continue;

                    void DrawRect()
                    {
                        GuidelineSet guidelines = new GuidelineSet();
                        guidelines.GuidelinesY.Add(offset + 0.5);
                        context.PushGuidelineSet(guidelines);
                        context.DrawRectangle(DrawingGlyph.ErrorLineBrush, null, new Rect(x1, offset, x2 - x1, size * 2));
                        sl = false;
                    }
                }
            }
        }
        
    }
}
