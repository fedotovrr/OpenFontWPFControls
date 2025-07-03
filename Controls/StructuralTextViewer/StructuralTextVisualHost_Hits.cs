using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using OpenFontWPFControls.FormattingStructure;
using OpenFontWPFControls.Layout;

namespace OpenFontWPFControls.Controls
{
    internal partial class StructuralTextVisualHost
    {
        private HitBox _hitObject;
        private DispatcherTimer _toolTipTimer;
        private ToolTip _hitToolTip;

        public object HitObject => _hitObject != null && _hitObject.Source is StructuralTextItem container ? container.HitObject : null;


        // Mouse

        protected override void OnMouseMove(MouseEventArgs e)
        {
            Point point = e.GetPosition(this) + _drawingOffset;
            HitCheck(point);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            HitCheck();
        }


        // Hits

        private void InvalidateLine(StructuralTextItem invalidChild)
        {
            foreach (ContainerDrawing container in _linesLayer)
            {
                if (container.Source is StructuralLine line && line.TextContainers.Any(x => x == invalidChild))
                {
                    container.Invalidate();
                }
            }
        }

        private void HitCheck(Point? shot = null)
        {
            HitBox last = _hitObject;
            _hitObject = null;

            if (shot != null)
            {
                Point point = shot.Value;
                foreach (HitBox box in _layout.HitBoxesInArea(_drawingBounds))
                {
                    if (point.X >= box.XOffset && point.Y >= box.YOffset &&
                        point.X <= box.XOffset + box.Width && point.Y <= box.YOffset + box.Height)
                    {
                        _hitObject = box;
                        break;
                    }
                }
            }

            if (last != _hitObject)
            {
                object toolTipContent = null;
                if (last?.Source is StructuralTextItem objLast)
                {
                    objLast.HitValue = false;
                    InvalidateLine(objLast);
                }
                if (_hitObject?.Source is StructuralTextItem objNow)
                {
                    objNow.HitValue = true;
                    toolTipContent = (objNow.HitObject as IInlineImage)?.ToolTip ?? (objNow.HitObject as IHyperlink)?.ToolTip;
                    InvalidateLine(objNow);
                }
                InvalidateDrawing();
                RaiseToolTip(toolTipContent);
            }

            //VisualTreeHelper.HitTest(this, null, result =>
            //{
            //    if (result.VisualHit.GetType() == typeof(DrawingVisual))
            //    {
            //        ((DrawingVisual)result.VisualHit).Opacity = ((DrawingVisual)result.VisualHit).Opacity == 1.0 ? 0.4 : 1.0;
            //    }
            //    return HitTestResultBehavior.Stop;
            //}, new PointHitTestParameters(point));
            
        }


        // ToolTips

        private void RaiseToolTip(object toolTipContent)
        {
            if (_hitToolTip != null)
            {
                _hitToolTip.IsOpen = false;
            }

            _toolTipTimer?.Stop();
            _toolTipTimer = null;

            if (toolTipContent != null)
            {
                _hitToolTip = new ToolTip { Content = toolTipContent };

                _toolTipTimer = new DispatcherTimer(DispatcherPriority.Normal);
                _toolTipTimer.Interval = TimeSpan.FromMilliseconds(ToolTipService.GetInitialShowDelay(this));
                _toolTipTimer.Tick += RaiseToolTipOpeningEvent;
                _toolTipTimer.Start();
            }
        }

        private void RaiseToolTipOpeningEvent(object sender, EventArgs e)
        {
            _toolTipTimer?.Stop();
            _toolTipTimer = null;
            if (_hitToolTip != null)
            {
                _hitToolTip.IsOpen = true;
            }
        }
    }
}