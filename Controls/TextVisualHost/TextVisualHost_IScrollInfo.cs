using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace OpenFontWPFControls.Controls
{
    partial class TextVisualHost : IScrollInfo
    {
        private const double _scrollLineDelta = 16.0;
        private const double _mouseWheelDelta = 48.0;

        private Size _viewport;
        private Size _extent;
        private ScrollViewer _scrollOwner;


        public bool CanVerticallyScroll { get; set; }

        public bool CanHorizontallyScroll { get; set; }

        public double ExtentWidth => _extent.Width;

        public double ExtentHeight => _extent.Height;

        public double ViewportWidth => _viewport.Width;

        public double ViewportHeight => _viewport.Height;

        public double HorizontalOffset => _drawingOffset.X;

        public double VerticalOffset => _drawingOffset.Y;

        public ScrollViewer ScrollOwner
        {
            get
            {
                return _scrollOwner;
            }
            set
            {
                if (value != _scrollOwner)
                {
                    CanVerticallyScroll = true;
                    CanHorizontallyScroll = true;
                    _drawingOffset = new Vector();
                    _viewport = new Size();
                    _extent = new Size();
                    _scrollOwner = value;
                    InvalidateArrange();
                }
            }
        }
        

        private void ArrangeScrollData(Size arrangeSize)
        {
            bool invalidateScrollInfo = false;

            if (!_viewport.Equals(arrangeSize))
            {
                _viewport = arrangeSize;
                invalidateScrollInfo = true;
            }

            Size contentSize = new Size(_viewWidth, _viewHeight);
            if (!_extent.Equals(contentSize))
            {
                _extent = contentSize;
                invalidateScrollInfo = true;
            }

            Vector offset = new Vector(
                Math.Max(0, Math.Min(_extent.Width - _viewport.Width, _drawingOffset.X)),
                Math.Max(0, Math.Min(_extent.Height - _viewport.Height, _drawingOffset.Y)));

            if (!offset.Equals(_drawingOffset))
            {
                _drawingOffset = offset;
                InvalidateDrawing();
                invalidateScrollInfo = true;
            }

            if (invalidateScrollInfo && ScrollOwner != null)
            {
                ScrollOwner.InvalidateScrollInfo();
            }
        }

        public void SetHorizontalOffset(double offset)
        {
            SetHorizontalOffset(this, offset);
        }

        private void SetHorizontalOffset(UIElement owner, double offset)
        {
            if (!CanHorizontallyScroll)
            {
                return;
            }

            offset = Math.Max(0, Math.Min(_extent.Width - _viewport.Width, offset));
            if (!offset.Equals(_drawingOffset.X))
            {
                _drawingOffset.X = offset;
                InvalidateDrawing();
                owner.InvalidateArrange();
                ScrollOwner?.InvalidateScrollInfo();
            }
        }

        public void SetVerticalOffset(double offset)
        {
            SetVerticalOffset(this, offset);
        }

        internal void SetVerticalOffset(UIElement owner, double offset)
        {
            if (!CanVerticallyScroll)
            {
                return;
            }

            offset = Math.Max(0, Math.Min(_extent.Height - _viewport.Height, offset));
            if (!offset.Equals(_drawingOffset.Y))
            {
                _drawingOffset.Y = offset;
                InvalidateDrawing();
                owner.InvalidateArrange();
                ScrollOwner?.InvalidateScrollInfo();
            }
        }

        public Rect MakeVisible(Visual visual, Rect rectangle)
        {
            if (rectangle.IsEmpty || visual == null || (visual != ScrollOwner && !ScrollOwner.IsAncestorOf(visual)))
            {
                return Rect.Empty;
            }

            GeneralTransform childTransform = visual.TransformToAncestor(ScrollOwner);
            rectangle = childTransform.TransformBounds(rectangle);

            Rect viewport = new Rect(_drawingOffset.X, _drawingOffset.Y, _viewport.Width, _viewport.Height);
            rectangle.X += viewport.X;
            rectangle.Y += viewport.Y;

            double minX = ComputeScrollOffset(viewport.Left, viewport.Right, rectangle.Left, rectangle.Right);
            double minY = ComputeScrollOffset(viewport.Top, viewport.Bottom, rectangle.Top, rectangle.Bottom);

            SetHorizontalOffset(ScrollOwner, minX);
            SetVerticalOffset(ScrollOwner, minY);

            if (CanHorizontallyScroll)
            {
                viewport.X = minX;
            }
            else
            {
                rectangle.X = viewport.X;
            }
            if (CanVerticallyScroll)
            {
                viewport.Y = minY;
            }
            else
            {
                rectangle.Y = viewport.Y;
            }
            rectangle.Intersect(viewport);
            if (!rectangle.IsEmpty)
            {
                rectangle.X -= viewport.X;
                rectangle.Y -= viewport.Y;
            }

            return rectangle;

            static double ComputeScrollOffset(double topView, double bottomView, double topChild, double bottomChild)
            {
                bool topInView = topChild >= topView && topChild < bottomView;
                bool bottomInView = bottomChild <= bottomView && bottomChild > topView;
                return topInView && bottomInView ? topView : topChild;
            }
        }

        public void LineUp()
        {
            SetVerticalOffset(this, _drawingOffset.Y - _scrollLineDelta);
        }

        public void LineDown()
        {
            SetVerticalOffset(this, _drawingOffset.Y + _scrollLineDelta);
        }

        public void LineLeft()
        {
            SetHorizontalOffset(this, _drawingOffset.X - _scrollLineDelta);
        }

        public void LineRight()
        {
            SetHorizontalOffset(this, _drawingOffset.X + _scrollLineDelta);
        }

        public void PageUp()
        {
            SetVerticalOffset(this, _drawingOffset.Y - _viewport.Height);
        }

        public void PageDown()
        {
            SetVerticalOffset(this, _drawingOffset.Y + _viewport.Height);
        }

        public void PageLeft()
        {
            SetHorizontalOffset(this, _drawingOffset.X - _viewport.Width);
        }

        public void PageRight()
        {
            SetHorizontalOffset(this, _drawingOffset.X + _viewport.Width);
        }

        public void MouseWheelUp()
        {
            SetVerticalOffset(this, _drawingOffset.Y - _mouseWheelDelta);
        }

        public void MouseWheelDown()
        {
            SetVerticalOffset(this, _drawingOffset.Y + _mouseWheelDelta);
        }

        public void MouseWheelLeft()
        {
            SetHorizontalOffset(this, _drawingOffset.X - _mouseWheelDelta);
        }

        public void MouseWheelRight()
        {
            SetHorizontalOffset(this, _drawingOffset.X + _mouseWheelDelta);
        }

    }
}
