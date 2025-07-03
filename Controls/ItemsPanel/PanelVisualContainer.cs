using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace OpenFontWPFControls.Controls
{
    internal class PanelVisualContainer : IDisposable
    {
        private readonly ContainerVisual _visual;
        private readonly FrameworkElement _control;
        private readonly BaseTextControl[] _boundedControls;
        public object Context;
        public int ContextIndex;
        public bool Placed;
        public Size ArrangeSize;

        public ContainerVisual Visual => _visual;

        public FrameworkElement Control => _control;

        public IEnumerable<BaseTextControl> BoundedControls => _boundedControls;


        public bool IsMeasureValid => Control?.IsMeasureValid ?? true;

        public PanelVisualContainer(FrameworkElement control)
        {
            _control = new Border { Child = control };
            _visual = new ContainerVisual { Children = { _control } };
            _boundedControls = GetVisuals(control).OfType<BaseTextControl>().ToArray();

            return;

            static IEnumerable<DependencyObject> GetVisuals(DependencyObject root)
            {
                foreach (DependencyObject child in LogicalTreeHelper.GetChildren(root).OfType<DependencyObject>())
                {
                    yield return child;
                    foreach (DependencyObject descendants in GetVisuals(child))
                        yield return descendants;
                }
            }
        }

        public void SetContext(object context, int contextIndex)
        {
            if (_control != null && _control.DataContext != context)
            {
                _control.DataContext = context;
            }
            Context = context;
            ContextIndex = contextIndex;
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
            Control.Arrange(new Rect(ArrangeSize));
        }

        public void Dispose()
        {
            if (_control != null)
            {
                _control.DataContext = null;
            }
            _visual.Children.Clear();
        }

        public override string ToString() => $"Placed: {Placed}, Offset: x {Visual.Offset.X} y {Visual.Offset.Y}";

    }
}