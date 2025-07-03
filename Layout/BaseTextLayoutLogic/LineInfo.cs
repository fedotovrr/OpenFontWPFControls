namespace OpenFontWPFControls.Layout;

public class LineInfo
{
    public int CharOffset;
    public int CharsCount;
    public int GlyphOffset;
    public int GlyphsCount;

    public LineInfo(int glyphOffset = 0, int charOffset = 0, int glyphsCount = 0, int charsCount = 0)
    {
        GlyphOffset = glyphOffset;
        CharOffset = charOffset;
        GlyphsCount = glyphsCount;
        CharsCount = charsCount;
    }
}