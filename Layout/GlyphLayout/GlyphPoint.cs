namespace OpenFontWPFControls.Layout
{
    public class GlyphPoint
    {
        public readonly ushort GlyphIndex;
        public int CharOffset;
        public short Width;
        public short GlyphOffsetX;
        public short GlyphOffsetY;

        public GlyphLayoutBuilder GlyphLayoutBuilder;

        public GlyphPoint(ushort glyphIndex = 0, int charOffset = 0, short width = 0, short glyphOffsetX = 0, short glyphOffsetY = 0, GlyphLayoutBuilder glyphLayoutBuilder = null)
        {
            GlyphIndex = glyphIndex;
            CharOffset = charOffset;
            Width = width;
            GlyphOffsetX = glyphOffsetX;
            GlyphOffsetY = glyphOffsetY;
            GlyphLayoutBuilder = glyphLayoutBuilder;
        }

        public float GetPixelBaselineOffset(float fontSize) =>
            (GlyphLayoutBuilder.Typeface.ClipedAscender + GlyphOffsetY) / (float)GlyphLayoutBuilder.Typeface.UnitsPerEm * fontSize;

        public float GetPixelOffsetX(float fontSize) =>
            GlyphOffsetX / (float)GlyphLayoutBuilder.Typeface.UnitsPerEm * fontSize;

        public float GetPixelOffsetY(float fontSize) =>
            GlyphOffsetY / (float)GlyphLayoutBuilder.Typeface.UnitsPerEm * fontSize;

        public float GetPixelWidth(float fontSize) =>
            Width / (float)GlyphLayoutBuilder.Typeface.UnitsPerEm * fontSize;


        public override string ToString() => $"CharOffset: {CharOffset:0000} GlyphIndex: {GlyphIndex:0000}";
    }
}