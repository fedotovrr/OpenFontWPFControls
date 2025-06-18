using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using OpenFontWPFControls.FormattingStructure;
using OpenFontWPFControls.Layout;
using OpenFontWPFControls.Layout.FormattingStructureLayout;

namespace OpenFontWPFControls.Controls
{
    internal partial class StructuralTextVisualHost
    {
        // Hits

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
                }
                if (_hitObject?.Source is StructuralTextItem objNow)
                {
                    objNow.HitValue = true;
                    toolTipContent = (objNow.HitObject as IInlineImage)?.ToolTip ?? (objNow.HitObject as IHyperlink)?.ToolTip;
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

        protected override void OnMouseMove(MouseEventArgs e)
        {
            HitCheck(e.GetPosition(this));
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            HitCheck();
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (_hitObject != null && _hitObject.Source is StructuralTextItem container)
            {
                OnHitObjectMouseLeftButtonDownCallBack?.Invoke(container.HitObject);
            }
        }

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            if (_hitObject != null && _hitObject.Source is StructuralTextItem container)
            {
                OnHitObjectMouseRightButtonDownCallBack?.Invoke(container.HitObject);
            }
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