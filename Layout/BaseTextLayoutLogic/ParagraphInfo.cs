namespace OpenFontWPFControls.Layout;

public readonly struct ParagraphInfo
{
    public readonly int CharOffset;
    public readonly int CharCount;

    public ParagraphInfo(int charOffset = 0, int charCount = 0)
    {
        CharOffset = charOffset;
        CharCount = charCount;
    }
}