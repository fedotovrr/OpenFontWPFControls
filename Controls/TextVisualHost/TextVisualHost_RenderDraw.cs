using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using OpenFontWPFControls.Layout;

namespace OpenFontWPFControls.Controls
{
    partial class TextVisualHost
    {
        private double ControlLayerOpacity => _controlLayerVisible ? SelectionAny ? 0.4 : 1 : 0;

        //protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi) => Invalidate();

        //protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo) => Invalidate();


        public void Invalidate()
        {
            Render();
            InvalidateMeasure();
            InvalidateVisual();
        }

        public void InvalidateDrawing()
        {
            _drawingValid = false;
            _controlsValid = false;
            InvalidateMeasure();
            InvalidateVisual();
        }

        public void InvalidateControlLayer()
        {
            _controlsValid = false;
            InvalidateMeasure();
            InvalidateVisual();
        }

        protected override Size MeasureOverride(Size constraint)
        {
            _caret.UpdateArrangeSize(constraint);

            _maxSize = new Size(Math.Min(float.MaxValue, (float)constraint.Width), Math.Min(float.MaxValue, (float)constraint.Height));
            if (_layout == null || Math.Abs(_layout.MaxWidth - (float)_maxSize.Width) > float.Epsilon)
            {
                Render();
            }

            Size desiredSize = new Size(Math.Min(constraint.Width, _viewWidth), Math.Min(constraint.Height, _viewHeight));

            return desiredSize;
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            CaretArrange();
            ArrangeScrollData(arrangeSize);
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
            _spellCheck = _parentControl?.GetValue(LargeTextBox.SpellCheckProperty) is bool sc && sc;

            if (_typefaceInfo == null ||
                style != _typefaceInfo.Style ||
                weight != _typefaceInfo.Weight ||
                !string.Equals(font, _typefaceInfo.TypefaceName, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(extension, _typefaceInfo.ExtensionName, StringComparison.OrdinalIgnoreCase))
            {
                _typefaceInfo = null;
                _typefaceInfo = TypefaceInfo.CacheGetOrTryCreate(font, style, weight, extension);
                _layout = null;
                _layout = new SimpleTextLayout(
                    text,
                    _typefaceInfo,
                    trimming,
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

            _caret.Control.Height = _layout.FontHeight;
            ((Caret)_caret.Control).CaretBrush = brush;

            _caretPoint.Owner = CaretPointOwners.Anyone;
            _caretPoint = _layout.CheckCaretPoint(_caretPoint);
            _selectionCapture = _caretPoint;

            _viewWidth = _layout.Width;
            _viewHeight = _layout.Height;
            _drawingValid = false;
            _controlsValid = false;

            OnRenderCallBack?.Invoke();
        }


        // Drawing

        private void CaretArrange()
        {
            double y = _caretPoint.Y - _drawingOffset.Y;
            double x = _layout.TextTrimming == TextTrimming.None ?
                Math.Min(_viewWidth - _caret.Control.Width - _drawingOffset.X, _caretPoint.X - _drawingOffset.X) : 
                Math.Min(_maxSize.Width - _caret.Control.Width, _caretPoint.X - _drawingOffset.X);
            _caret.Location = new Point(x, y);
            _caret.Arrange();
        }

        public void SetControlLayerVisibility(bool value)
        {
            if (_controlLayerVisible != value)
            {
                _controlLayerVisible = value;
                ((Caret)_caret.Visual).Visibility = _controlLayerVisible ? Visibility.Visible : Visibility.Collapsed;
                if (_controlLayer is DrawingVisual drawing)
                {
                    drawing.Opacity = ControlLayerOpacity;
                }
            }
        }


        private void Draw()
        {
            if (!_drawingValid)
            {
                _drawingBounds =
                    _scrollOwner != null ? new Rect(_drawingOffset.X, _drawingOffset.Y, _viewport.Width, _viewport.Height) :
                    _parentControl?.GetValue(BaseTextControl.DrawingBoundsProperty) is Rect r ? r :
                    new Rect(0, 0, double.PositiveInfinity, double.PositiveInfinity);

                UpdateErrorLayer();
                UpdateContentLayer();
                UpdateBackgroundLayer();
                _drawingValid = true;
                OnDrawCallBack?.Invoke();
            }
            if (!_controlsValid)
            {
                UpdateControlLayer();
                _controlsValid = true;
            }
        }

        private void UpdateBackgroundLayer()
        {
            DrawingVisual drawing = new DrawingVisual();
            DrawingContext context = drawing.RenderOpen();
            context.DrawRectangle(Brushes.Transparent, null, new Rect(0, 0, Math.Round(_viewWidth), Math.Round(_viewHeight)));
            context.Close();
            drawing.Offset = -_drawingOffset;
            RemoveVisualChild(_backgroundLayer);
            AddVisualChild(_backgroundLayer = drawing);
        }

        private void UpdateContentLayer()
        {
            _linesLayer.ForEach(child => child.Used = false);

            foreach (SimpleTextLine line in _layout.LinesInArea(_drawingBounds))
            {
                ContainerDrawing container = _linesLayer.FirstOrDefault(c => c.Source == line); // line valid anyway
                if (container != null)
                {
                    container.Used = true;
                    container.Visual.Offset = new Vector(0, line.YOffset) -_drawingOffset;
                }
                else
                {
                    container = new ContainerDrawing(line);
                    container.Visual.Offset = new Vector(0, line.YOffset) - _drawingOffset;
                    _linesLayer.Add(container);
                    AddVisualChild(container.Visual);
                }
            }

            int count = 0;
            for (int i = _linesLayer.Count - 1; i >= 0; i--)
            {
                if (!_linesLayer[i].Used)
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
            drawing.Offset = - _drawingOffset;
            RemoveVisualChild(_errorsLayer);
            AddVisualChild(_errorsLayer = drawing);
        }

        private void UpdateControlLayer()
        {
            DrawingVisual drawing = new DrawingVisual { Opacity = ControlLayerOpacity };
            DrawingContext context = drawing.RenderOpen();
            
            if (_layout?.CaretPoints.Any() == true)
            {
                if (SelectionAny)
                {
                    DrawSelection(context, _parentControl?.GetValue(LargeTextBox.SelectionBrushProperty) as Brush ?? Brushes.CornflowerBlue);
                }
                ((Caret)_caret.Visual).Visibility = _controlLayerVisible ? Visibility.Visible : Visibility.Collapsed;
            }

            context.Close();
            drawing.Offset = -_drawingOffset;
            RemoveVisualChild(_controlLayer);
            AddVisualChild(_controlLayer = drawing);
        }

        private void DrawSelection(DrawingContext context, Brush brush)
        {
            int selectionStart = SelectionStart;
            int selectionEnd = SelectionEnd;
            CaretPoint start, end;
            foreach (SimpleTextLine line in _layout.LinesInArea(_drawingBounds))
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
                            new Point(Math.Round(start.X), Math.Round(line.YOffset)),
                            new Point(Math.Round(current.X), Math.Round(line.YOffset + line.Height)));
                        context.DrawRectangle(brush, null, rect);
                    }
                    else if (current.CharOffset >= selectionEnd)
                    {
                        end = current.CharOffset == selectionEnd ? current : end;
                        Rect rect = new Rect(
                            new Point(Math.Round(start.X), Math.Round(line.YOffset)),
                            new Point(Math.Round(end.X), Math.Round(line.YOffset + line.Height)));
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

        private void DrawErrors(DrawingContext context)
        {
            if (CharIsError != null)
            {
                foreach (SimpleTextLine line in _layout.LinesInArea(_drawingBounds))
                {
                    bool sl = false;
                    float x1 = 0;
                    float x2 = 0;
                    double size = _layout.TypefaceInfo.DefaultBuilder.GetPixelUnderlineSize(_layout.FontSize);
                    double offset = line.YOffset + _layout.TypefaceInfo.DefaultBuilder.GetPixelBaseline(_layout.FontSize) + _layout.TypefaceInfo.DefaultBuilder.GetPixelUnderlinePosition(_layout.FontSize) * 2 + size;

                    foreach ((GlyphPoint glyph, float x) in line.GlyphPoints)
                    {
                        if (CharIsError(line.CharOffset + glyph.CharOffset))
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



