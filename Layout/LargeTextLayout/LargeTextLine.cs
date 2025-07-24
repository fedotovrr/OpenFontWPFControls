using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace OpenFontWPFControls.Layout
{
    public class LargeTextLine : IVisualGenerator
    {
        public LargeTextParagraph Paragraph;
        public int CharOffset; // by paragraph
        public int CharCount;
        public int GlyphOffset; // by paragraph
        public int GlyphCount;
        private float? _width;

        public LargeTextLine(LineInfo lineInfo, LargeTextParagraph paragraph)
        {
            Paragraph = paragraph;
            GlyphOffset = lineInfo.GlyphOffset;
            CharOffset = lineInfo.CharOffset;
            GlyphCount = lineInfo.GlyphsCount;
            CharCount = lineInfo.CharsCount;
        }

        public LargeTextLine(LargeTextParagraph paragraph, int glyphOffset = 0, int charOffset = 0, int glyphCount = 0, int charCount = 0)
        {
            Paragraph = paragraph;
            GlyphOffset = glyphOffset;
            CharOffset = charOffset;
            GlyphCount = glyphCount;
            CharCount = charCount;
        }


        public float Height => Paragraph.TextLayout.FontHeight;

        public float Width => _width ?? (_width = Glyphs.Sum(glyph => glyph.GetPixelWidth(Paragraph.TextLayout.FontSize))).Value;

        public int GlobalCharOffset => Paragraph.CharOffset + CharOffset;

        public bool CaretPointContains(int charOffset) => charOffset >= GlobalCharOffset && charOffset <= GlobalCharOffset + CharCount;

        public DrawingVisual CreateDrawingVisual()
        {
            DrawingVisual visual = new DrawingVisual();
            DrawingContext context = visual.RenderOpen();

            GuidelineSet guidelines = new GuidelineSet();
            guidelines.GuidelinesX.Add(0);
            guidelines.GuidelinesY.Add(Paragraph.TextLayout.TypefaceInfo.DefaultBuilder.Typeface.ClipedAscender / (float)Paragraph.TextLayout.TypefaceInfo.DefaultBuilder.Typeface.UnitsPerEm * Paragraph.TextLayout.FontSize);
            context.PushGuidelineSet(guidelines);

            foreach ((GlyphPoint glyph, float x) in GlyphPoints)
            {
                DrawingGlyph.DrawGlyph(
                    context,
                    glyph,
                    Paragraph.TextLayout.FontSize,
                    Paragraph.TextLayout.PixelsPerDip,
                    //Paragraph.TextLayout.GetFormatValue(Paragraph.CharOffset + glyph.CharOffset, FormatType.Foreground, Paragraph.Layout.Foreground),
                    Paragraph.TextLayout.Foreground,
                    Paragraph.TextLayout.Underline,
                    Paragraph.TextLayout.Strike,
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
                int limit = GlyphOffset + GlyphCount;
                for (int i = GlyphOffset; i < limit; i++)
                    yield return Paragraph.GlyphsLayout.GlyphPoints[i];
            }
        }

        public IEnumerable<GlyphPoint> ReverseGlyphs
        {
            get
            {
                int limit = GlyphOffset + GlyphCount;
                for (int i = limit - 1; i >= GlyphOffset; i--)
                    yield return Paragraph.GlyphsLayout.GlyphPoints[i];
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
                    x += glyph.GetPixelWidth(Paragraph.TextLayout.FontSize);
                }
            }
        }

        public IEnumerable<CaretPoint> CaretPoints
        {
            get
            {
                float x = 0;
                yield return new CaretPoint(CaretPointOwners.StartLine, GlobalCharOffset, x);
                foreach (GlyphPoint glyph in Glyphs)
                {
                    yield return new CaretPoint(CaretPointOwners.Glyph, Paragraph.CharOffset + glyph.CharOffset, x);
                    x += glyph.GetPixelWidth(Paragraph.TextLayout.FontSize);
                }

                yield return new CaretPoint(CaretPointOwners.EndLine, GlobalCharOffset + CharCount, x);
            }
        }

        public IEnumerable<CaretPoint> ReverseCaretPoints
        {
            get
            {
                float x = Width;
                yield return new CaretPoint(CaretPointOwners.EndLine, GlobalCharOffset + CharCount, x);
                foreach (GlyphPoint glyph in ReverseGlyphs)
                {
                    x -= glyph.GetPixelWidth(Paragraph.TextLayout.FontSize);
                    yield return new CaretPoint(CaretPointOwners.Glyph, Paragraph.CharOffset + glyph.CharOffset, x);
                }

                yield return new CaretPoint(CaretPointOwners.StartLine, GlobalCharOffset, 0);
            }
        }


        // Debug

        public string Text => Paragraph.TextLayout.Text.Substring(GlobalCharOffset, CharCount);

        public override string ToString() => $"CharOffset: {GlobalCharOffset:0000} Length: {CharCount}";
    }
}
