using System;

namespace OpenFontWPFControls.Layout.FormattingStructureLayout
{
    public class GlyphInfo
    {
        public readonly StructuralTextItem Text;
        public readonly GlyphPoint Glyph;
        public readonly float X;
        public readonly float Width;
        public readonly int CharsCount;

        public GlyphInfo(StructuralTextItem text, GlyphPoint glyph, float x, float width, int charsCount)
        {
            Text = text;
            Glyph = glyph;
            X = x;
            Width = width;
            CharsCount = charsCount;
        }
    }
}
