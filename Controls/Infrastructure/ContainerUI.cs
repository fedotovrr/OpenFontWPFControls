using System;
using System.Windows;
using System.Windows.Media;

namespace OpenFontWPFControls.Controls
{
    internal class ContainerUI
    {
        private readonly FrameworkElement _control;
        public Size ArrangeSize;
        public Point Location;

        public Visual Visual => _control;

        public FrameworkElement Control => _control;

        public ContainerUI(FrameworkElement source)
        {
            _control = source;
        }

        public void UpdateArrangeSize(Size renderSize, bool stretch = false)
        {
            if (_control != null)
            {
                _control.Measure(renderSize);
                if (stretch)
                {
                    ArrangeSize = new Size(
                        Math.Max(_control.DesiredSize.Width, double.IsInfinity(renderSize.Width) ? _control.DesiredSize.Width : renderSize.Width),
                        Math.Max(_control.DesiredSize.Height, double.IsInfinity(renderSize.Height) ? _control.DesiredSize.Height : renderSize.Height));
                }
                else
                {
                    ArrangeSize = _control.DesiredSize;
                }
            }
        }

        public void Arrange()
        {
            Control.Arrange(new Rect(Location, ArrangeSize));
        }
    }
}
