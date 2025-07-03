using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Typography.OpenFont;

namespace OpenFontWPFControls.Layout
{
    public class SimpleTextLine : IVisualGenerator, IPlacement
    {
        public readonly SimpleTextLayout Layout;
        public readonly int CharsCount;
        public readonly int CharOffset;
        public readonly int GlyphOffset;
        public readonly int GlyphsCount;
        public readonly float YOffset;

        private readonly IList<GlyphPoint> _glyphPoints;
        private float? _width;

        float IPlacement.XOffset => 0;

        float IPlacement.YOffset => YOffset;

        float IPlacement.Width => Width;

        float IPlacement.Height => Height;


        public SimpleTextLine(SimpleTextLayout layout, IList<GlyphPoint> glyphs, int charOffset, int charsCount, int glyphOffset, int glyphsCount, float y)
        {
            _glyphPoints = glyphs;
            CharOffset = charOffset;
            CharsCount = charsCount;
            GlyphOffset = glyphOffset;
            GlyphsCount = glyphsCount;
            YOffset = y;
            Layout = layout ?? throw new NullReferenceException();
        }

        public float Height => Layout.FontHeight;

        public float Width => GetWidth();

        private float GetWidth()
        {
            return _width ?? (_width = Glyphs.Sum(glyph => glyph.GetPixelWidth(Layout.FontSize))).Value;
        }

        public bool CaretPointContains(int charOffset)
        {
            return charOffset >= CharOffset && charOffset <= CharOffset + CharsCount;
        }

        public DrawingVisual CreateDrawingVisual()
        {
            DrawingVisual visual = new DrawingVisual();
            DrawingContext context = visual.RenderOpen();

            GuidelineSet guidelines = new GuidelineSet();
            guidelines.GuidelinesX.Add(0);
            guidelines.GuidelinesY.Add(0);
            context.PushGuidelineSet(guidelines);

            foreach ((GlyphPoint glyph, float x) in GlyphPoints)
            {
                DrawingGlyph.DrawGlyph(
                    context,
                    glyph,
                    Layout.FontSize,
                    Layout.PixelsPerDip,
                    Layout.Foreground,
                    Layout.Underline,
                    Layout.Strike,
                    false,
                    x,
                    0,
                    false);
            }

            context.Pop();

            context.Close();
            return visual;
        }


        // Enumerators

        public IEnumerable<GlyphPoint> Glyphs
        {
            get
            {
                int index = GlyphOffset;
                int count = GlyphsCount;
                while (count > 0)
                {
                    yield return _glyphPoints[index];
                    count--;
                    index++;
                }
            }
        }

        public IEnumerable<GlyphPoint> ReverseGlyphs
        {
            get
            {
                int index = GlyphOffset + GlyphsCount - 1;
                int count = GlyphsCount;
                while (count > 0)
                {
                    yield return _glyphPoints[index];
                    count--;
                    index--;
                }
            }
        }

        public IEnumerable<(GlyphPoint glyph, float x)> GlyphPoints
        {
            get
            {
                float x = 0;
                foreach (GlyphPoint glyph in Glyphs)
                {
                    yield return (glyph, x);
                    x += glyph.GetPixelWidth(Layout.FontSize);
                }
            }
        }

        public IEnumerable<CaretPoint> CaretPoints
        {
            get
            {
                float x = 0;
                yield return new CaretPoint(CaretPointOwners.StartLine, CharOffset, x, YOffset);
                foreach (GlyphPoint glyph in Glyphs)
                {
                    yield return new CaretPoint(CaretPointOwners.Glyph, CharOffset + glyph.CharOffset - _glyphPoints[GlyphOffset].CharOffset, x, YOffset);
                    x += glyph.GetPixelWidth(Layout.FontSize);
                }
                yield return new CaretPoint(CaretPointOwners.EndLine, CharOffset + CharsCount, x, YOffset);
            }
        }

        public IEnumerable<CaretPoint> ReverseCaretPoints
        {
            get
            {
                float x = Width;
                yield return new CaretPoint(CaretPointOwners.EndLine, CharOffset + CharsCount, x, YOffset);
                foreach (GlyphPoint glyph in ReverseGlyphs)
                {
                    x -= glyph.GetPixelWidth(Layout.FontSize);
                    yield return new CaretPoint(CaretPointOwners.Glyph, CharOffset + glyph.CharOffset - _glyphPoints[GlyphOffset].CharOffset, x, YOffset);
                }
                yield return new CaretPoint(CaretPointOwners.StartLine, CharOffset, 0, YOffset);
            }
        }


        // Debug

        public string Text => Layout.Text.Substring(CharOffset, CharsCount);
        
        public override string ToString() => $"GlyphCount: {GlyphsCount:0000} X: {0:000.0} Y: {YOffset:000.0} Text: {Text}";
    }
}