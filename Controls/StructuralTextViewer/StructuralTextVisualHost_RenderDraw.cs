using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using OpenFontWPFControls.FormattingStructure;
using OpenFontWPFControls.Layout;

namespace OpenFontWPFControls.Controls
{
    internal partial class StructuralTextVisualHost
    {
        private double ControlLayerOpacity => _controlLayerVisible ? SelectionAny ? 0.4 : 1 : 0;

        //protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi) => Invalidate();

        //protected override void OnRenderSizeChanged(SizeChangedInfo e) => Invalidate();

        public void Invalidate()
        {
            Render();
            InvalidateMeasure();
            InvalidateVisual();
        }

        public void InvalidateDrawing()
        {
            _drawingValid = false;
            InvalidateMeasure();
            InvalidateVisual();
        }


        protected override Size MeasureOverride(Size constraint)
        {
            _maxSize = new Size(Math.Min(float.MaxValue, (float)constraint.Width), Math.Min(float.MaxValue, (float)constraint.Height));
            if (_layout == null || Math.Abs(_layout.MaxWidth - (float)_maxSize.Width) > float.Epsilon)
            {
                Render();
            }

            //Size renderSize = new Size(_viewWidth, _viewHeight);
            //_uiLayer.ForEach(element => ((UIElement)element.Children[0]).Measure(renderSize));

            Size desiredSize = new Size(Math.Min(constraint.Width, _viewWidth), Math.Min(constraint.Height, _viewHeight));

            return desiredSize;
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            //arrangeSize = new Size(_viewWidth, _viewHeight);
            //Rect arrangeRect = new Rect(0, 0, arrangeSize.Width, arrangeSize.Height);
            //_uiLayer.ForEach(element => ((UIElement)element.Children[0]).Arrange(arrangeRect));

            ArrangeScrollData(arrangeSize);
            Draw();
            return arrangeSize;
        }


        // Rendering

        private void Render()
        {
            DpiScale scale = VisualTreeHelper.GetDpi(this);
            double size = _parentControl?.FontSize ?? SystemFonts.MessageFontSize;
            IContainersCollection structure = _parentControl?.GetValue(StructuralTextViewer.FormattingStructureProperty) as IContainersCollection ?? new DefaultFormattingStructure();
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
                _layout = new StructuralLayout(
                    structure,
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
                _layout.Structure = structure;
                _layout.TextTrimming = trimming;
                _layout.MaxWidth = (float)_maxSize.Width;
                _layout.FontSize = (float)size;
                _layout.PixelsPerDip = (float)scale.PixelsPerDip;
                _layout.PixelsPerInch = (float)scale.PixelsPerInchX;
                _layout.Foreground = brush;
                _layout.Underline = underline;
                _layout.Strike = strike;
            }

            _viewWidth = _layout.Width;
            _viewHeight = _layout.Height;
            _drawingValid = false;
            
            OnRenderCallBack?.Invoke();
        }


        // Drawing

        public void SetControlLayerVisibility(bool value)
        {
            if (_controlLayerVisible != value)
            {
                _controlLayerVisible = value;
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

                //UpdateErrorLayer();
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
            drawing.Offset = -_drawingOffset;
            RemoveVisualChild(_backgroundLayer);
            AddVisualChild(_backgroundLayer = drawing);
        }

        private void UpdateContentLayer()
        {
            _linesLayer.ForEach(child => child.Used = false);
            foreach (StructuralLine line in _layout.LinesInArea(_drawingBounds))
            {
                ContainerDrawing container = _linesLayer.FirstOrDefault(c => c.Valid && c.Source == line);
                if (container != null)
                {
                    container.Used = true;
                    container.Visual.Offset = new Vector(Math.Round(line.XOffset), Math.Round(line.YOffset)) - _drawingOffset;
                }
                else
                {
                    container = new ContainerDrawing(line);
                    container.Used = true;
                    container.Visual.Offset = new Vector(Math.Round(line.XOffset), Math.Round(line.YOffset)) - _drawingOffset;
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


            _graphicLayer.ForEach(child => RemoveVisualChild(child.Visual));
            _graphicLayer.Clear();
            foreach (StructuralBorder border in _layout.BordersInArea(_drawingBounds))
            {
                ContainerDrawing container = new ContainerDrawing(border);
                container.Visual.Offset = -_drawingOffset;
                _graphicLayer.Add(container);
                AddVisualChild(container.Visual);
            }


            //foreach (ContainerVisual container in _uiLayer)
            //{
            //    RemoveVisualChild(container);
            //    RemoveLogicalChild(container);
            //}
            //_uiLayer.Clear();
            //_uiLayer.AddRange(_layout.Controls);
            //foreach (ContainerVisual container in _uiLayer)
            //{
            //    AddVisualChild(container);
            //    AddLogicalChild(container);
            //}

            InvalidateMeasure();
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
            drawing.Offset = -_drawingOffset;
            RemoveVisualChild(_errorsLayer);
            AddVisualChild(_errorsLayer = drawing);
        }

        public void UpdateControlLayer()
        {
            DrawingVisual drawing = new DrawingVisual { Opacity = ControlLayerOpacity };
            DrawingContext context = drawing.RenderOpen();

            if (SelectionAny)
            {
                DrawSelection(context, _parentControl?.GetValue(LargeTextBox.SelectionBrushProperty) as Brush ?? Brushes.CornflowerBlue);
            }
            //DrawCaret(context, _parentControl?.GetValue(TextBox.CaretBrushProperty) as Brush ?? Brushes.Black);

            context.Close();
            drawing.Offset = -_drawingOffset;
            RemoveVisualChild(_controlLayer);
            AddVisualChild(_controlLayer = drawing);
        }

        private void DrawSelection(DrawingContext context, Brush brush)
        {
            int selectionStart = SelectionStart;
            int selectionEnd = SelectionEnd;
            StructuralCaretPoint start, end;
            start = end = new StructuralCaretPoint(null);
            foreach (StructuralCaretPoint current in _layout.CaretPointsEnumerator(_drawingBounds))
            {
                if (current.GlobalCharOffset <= selectionStart || current.Owner == CaretPointOwners.StartLine)
                {
                    start = end = current;
                }
                else if (current.Owner == CaretPointOwners.EndLine)
                {
                    Rect rect = new Rect(
                        new Point(Math.Round(start.X), Math.Round(start.Y)),
                        new Point(Math.Round(current.X), Math.Round(start.Y + start.Height)));
                    context.DrawRectangle(brush, null, rect);
                }
                else if (current.GlobalCharOffset >= selectionEnd)
                {
                    end = current.GlobalCharOffset == selectionEnd ? current : end;
                    Rect rect = new Rect(
                        new Point(Math.Round(start.X), Math.Round(start.Y)),
                        new Point(Math.Round(end.X), Math.Round(start.Y + start.Height)));
                    context.DrawRectangle(brush, null, rect);
                    break;
                }
                else
                {
                    end = current;
                }
            }
        }

        private void DrawCaret(DrawingContext context, Brush brush)
        {
            foreach (StructuralCaretPoint current in _layout.CaretPointsEnumerator(_drawingBounds))
            {
                if (current.Equals(_caretPoint))
                {
                    _caretPoint = current;
                    Rect rect = new Rect(current.X - 1, current.Y, 1, current.Height);
                    GuidelineSet guidelines = new GuidelineSet();
                    guidelines.GuidelinesX.Add(rect.X);
                    context.PushGuidelineSet(guidelines);
                    context.DrawRectangle(brush, null, rect);
                    _caretVisible = true;
                    return;
                }
            }
            _caretVisible = false;
        }

        private void DrawErrors(DrawingContext context)
        {
            if (CharIsError != null)
            {
                
            }
        }

    }
}