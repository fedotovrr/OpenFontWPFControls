using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows;
using OpenFontWPFControls.FormattingStructure;

namespace OpenFontWPFControls.Layout
{
    public class StructuralBorder : IVisualGenerator, IBorder, IPlacement
    {
        public readonly SolidColorBrush _background;
        public readonly SolidColorBrush _borderBrush;
        public readonly Thickness _borderThickness;
        public readonly CornerRadius _cornerRadius;
        public readonly Thickness _margin;
        public readonly Thickness _padding;
        public float XOffset;
        public float YOffset;
        public float Width;
        public float Height;

        float IPlacement.XOffset => XOffset;

        float IPlacement.YOffset => YOffset;

        float IPlacement.Width => Width;

        float IPlacement.Height => Height;

        public SolidColorBrush Background => _background;

        public SolidColorBrush BorderBrush => _borderBrush;

        public Thickness BorderThickness => _borderThickness;

        public CornerRadius CornerRadius => _cornerRadius;

        public Thickness Margin => _margin;

        public Thickness Padding => _padding;

        public float ContentWidth => GetContentWidth(Width);

        public float ContentHeight => GetContentHeight(Height);

        public float ContentXOffset => (float)(Margin.Left + Padding.Left + BorderThickness.Left);

        public float ContentYOffset => (float)(Margin.Top + Padding.Top + BorderThickness.Top);

        public float OwnWidth => GetOwnWidth(this);

        public float OwnHeight => GetOwnHeight(this);

        public StructuralBorder(IBorder border, float width, float height)
        {
            if (border != null)
            {
                _background = border.Background ?? Brushes.Transparent;
                _borderBrush = border.BorderBrush ?? Brushes.Transparent;
                _borderThickness = border.BorderThickness;
                _cornerRadius = border.CornerRadius;
                _margin = border.Margin;
                _padding = border.Padding;
            }
            else
            {
                _background = Brushes.Transparent;
                _borderBrush = Brushes.Transparent;
            }
            Width = width;
            Height = height;
        }

        public StructuralBorder(IBorder border, float x, float y, float contentWidth, float contentHeight)
        {
            if (border != null)
            {
                _background = border.Background ?? Brushes.Transparent;
                _borderBrush = border.BorderBrush ?? Brushes.Transparent;
                _borderThickness = border.BorderThickness;
                _cornerRadius = border.CornerRadius;
                _margin = border.Margin;
                _padding = border.Padding;
            }
            else
            {
                _background = Brushes.Transparent;
                _borderBrush = Brushes.Transparent;
            }
            Width = GetWidth(contentWidth);
            Height = GetHeight(contentHeight);
            XOffset = x; 
            YOffset = y;
        }

        public static float GetOwnWidth(IBorder border)
        {
            return border == null ? 0 : (float)(border.Margin.Left + border.Margin.Right + border.Padding.Left + border.Padding.Right + border.BorderThickness.Left + border.BorderThickness.Right);
        }
        public static float GetOwnHeight(IBorder border)
        {
            return border == null ? 0 : (float)(border.Margin.Top + border.Margin.Bottom + border.Padding.Top + border.Padding.Bottom + border.BorderThickness.Top + border.BorderThickness.Bottom);
        }

        private float GetContentWidth(float width)
        {
            return (float)(width - Margin.Left - Margin.Right - Padding.Left - Padding.Right - BorderThickness.Left - BorderThickness.Right);
        }

        private float GetContentHeight(float height)
        {
            return (float)(Height - Margin.Top - Margin.Bottom - Padding.Top - Padding.Bottom - BorderThickness.Top - BorderThickness.Bottom);
        }

        private float GetWidth(float contentWidth)
        {
            return (float)(contentWidth + Margin.Left + Margin.Right + Padding.Left + Padding.Right + BorderThickness.Left + BorderThickness.Right);
        }

        private float GetHeight(float contentHeight)
        {
            return (float)(contentHeight + Margin.Top + Margin.Bottom + Padding.Top + Padding.Bottom + BorderThickness.Top + BorderThickness.Bottom);
        }

        public DrawingVisual CreateDrawingVisual()
        {
            float t_r = (float)BorderThickness.Right;
            float t_l = (float)BorderThickness.Left;
            float t_t = (float)BorderThickness.Top;
            float t_b = (float)BorderThickness.Bottom;

            float r_tl = (float)CornerRadius.TopLeft;
            float r_tr = (float)CornerRadius.TopRight;
            float r_bl = (float)CornerRadius.BottomLeft;
            float r_br = (float)CornerRadius.BottomRight;

            float width = (float)(Width - Margin.Left - Margin.Right);
            float height = (float)(Height - Margin.Top - Margin.Bottom);

            bool drawInside = true;

            if (t_t + t_b > height || t_r + t_l > width)
            {
                t_r = t_l = t_t = t_b = 0;
                drawInside = false;
            }

            float sumR = r_tl + r_tr + t_l + t_r;
            if (sumR > width)
            {
                r_tl = r_tl / sumR * (width - t_l - t_r);
                r_tr = r_tr / sumR * (width - t_l - t_r);
            }
            
            sumR = r_bl + r_br + t_l + t_r;
            if (sumR > width)
            {
                r_bl = r_bl / sumR * (width - t_l - t_r);
                r_br = r_br / sumR * (width - t_l - t_r);
            }

            sumR = r_tl + r_bl + t_t + t_b;
            if (sumR > height)
            {
                r_tl = r_tl / sumR * (height - t_t - t_b);
                r_bl = r_bl / sumR * (height - t_t - t_b);
            }

            sumR = r_tr + r_br + t_t + t_b;
            if (sumR > height)
            {
                r_tr = r_tr / sumR * (height - t_t - t_b);
                r_br = r_br / sumR * (height - t_t - t_b);
            }

            float tk_r = Math.Max(0, t_r - Math.Min(r_tr, r_br));
            float tk_l = Math.Max(0, t_l - Math.Min(r_tl, r_bl));
            float tk_t = Math.Max(0, t_t - Math.Min(r_tl, r_tr));
            float tk_b = Math.Max(0, t_b - Math.Min(r_br, r_bl));

            float x = (float)Margin.Left + XOffset;
            float y = (float)Margin.Top + YOffset;

            DrawingVisual visual = new DrawingVisual();
            DrawingContext context = visual.RenderOpen();

            GuidelineSet guidelines = new GuidelineSet();
            guidelines.GuidelinesX.Add(x);
            guidelines.GuidelinesY.Add(y);
            guidelines.GuidelinesX.Add(x + width);
            guidelines.GuidelinesY.Add(y + height);
            context.PushGuidelineSet(guidelines);

            context.PushTransform(new MatrixTransform(1, 0, 0, 1, x, y));

            PathGeometry group = new PathGeometry(new PathFigure[]
            {
                new PathFigure(new Point(0, r_tl), drawInside ? GenerateOutside().Concat(GenerateInside()) : GenerateOutside(), false),
            });
            context.DrawGeometry(BorderBrush, null, group);

            if (drawInside)
            {
                group = new PathGeometry(new PathFigure[]
                {
                    new PathFigure(new Point(0, r_tl), GenerateInside(), false),
                });
                context.DrawGeometry(Background, null, group);
            }

            context.Close();

            return visual;

            IEnumerable<PathSegment> GenerateOutside()
            {
                yield return new QuadraticBezierSegment(new Point(0, 0), new Point(r_tl, 0), false);
                yield return new LineSegment(new Point(width - r_tr, 0), false);
                yield return new QuadraticBezierSegment(new Point(width, 0), new Point(width, r_tr), false);
                yield return new LineSegment(new Point(width, height - r_br), false);
                yield return new QuadraticBezierSegment(new Point(width, height), new Point(width - r_br, height), false);
                yield return new LineSegment(new Point(r_bl, height), false);
                yield return new QuadraticBezierSegment(new Point(0, height), new Point(0, height - r_bl), false);
                yield return new LineSegment(new Point(0, r_tl), false);
            }

            IEnumerable<PathSegment> GenerateInside()
            {
                yield return new LineSegment(new Point(t_l, r_tl + tk_t), false);
                yield return new QuadraticBezierSegment(new Point(t_l, t_t), new Point(r_tl + tk_l, t_t), false);
                yield return new LineSegment(new Point(width - r_tr - tk_r, t_t), false);
                yield return new QuadraticBezierSegment(new Point(width - t_r, t_t), new Point(width - t_r, r_tr + tk_t), false);
                yield return new LineSegment(new Point(width - t_r, height - r_br - tk_b), false);
                yield return new QuadraticBezierSegment(new Point(width - t_r, height - t_b), new Point(width - r_br - tk_r, height - t_b), false);
                yield return new LineSegment(new Point(r_bl + tk_l, height - t_b), false);
                yield return new QuadraticBezierSegment(new Point(t_l, height - t_b), new Point(t_l, height - r_bl - tk_b), false);
                yield return new LineSegment(new Point(t_l, r_tl + tk_t), false);
            }
        }
    }
}
