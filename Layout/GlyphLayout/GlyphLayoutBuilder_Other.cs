using System.Linq;

namespace OpenFontWPFControls.Layout
{
    public partial class GlyphLayoutBuilder
    {
        private GlyphPoint _spaceGlyph;
        private GlyphPoint _tabGlyph;
        private GlyphPoint _emptyGlyph;

        public System.Windows.Media.GlyphTypeface GlyphTypeface { get; set; }

        public GlyphPoint SpaceGlyph => _spaceGlyph;

        public GlyphPoint TabGlyph => _tabGlyph;

        public GlyphPoint EmptyGlyph => _emptyGlyph;

        private void InitTools()
        {
            GlyphLayout layout = Build(new StringCharacterBuffer(" "));
            _spaceGlyph = layout[0];
            _spaceGlyph.GlyphLayoutBuilder = this;

            _tabGlyph = new GlyphPoint(_spaceGlyph.GlyphIndex, 0, (short)(_spaceGlyph.Width * 4), _spaceGlyph.GlyphOffsetX, _spaceGlyph.GlyphOffsetY);
            _tabGlyph.GlyphLayoutBuilder = this;

            _emptyGlyph = new GlyphPoint(_spaceGlyph.GlyphIndex, 0, 0, _spaceGlyph.GlyphOffsetX, _spaceGlyph.GlyphOffsetY);
            _emptyGlyph.GlyphLayoutBuilder = this;
        }

        // Tools


        public float UnderlinePosition => _typeface.PostTable.UnderlinePosition;

        public float UnderlineSize => _typeface.PostTable.UnderlineThickness;

        public float StrikePosition => _typeface.OS2Table.yStrikeoutPosition;

        public float StrikeSize => _typeface.OS2Table.yStrikeoutSize;

        public float Baseline => _baseline;

        public float ClipedAscender => _typeface.ClipedAscender;

        public float Height => _typeface.ClipedAscender + _typeface.ClipedDescender;

        public float ConvertToPixelSize(float value, float fontSize) => value / Typeface.UnitsPerEm * fontSize;

        public float GetPixelUnderlinePosition(float fontSize) => ConvertToPixelSize(UnderlinePosition, fontSize);

        public float GetPixelUnderlineSize(float fontSize) => ConvertToPixelSize(UnderlineSize, fontSize);

        public float GetPixelStrikePosition(float fontSize) => ConvertToPixelSize(StrikePosition, fontSize);

        public float GetPixelStrikeSize(float fontSize) => ConvertToPixelSize(StrikeSize, fontSize);

        public float GetPixelBaseline(float fontSize) => ConvertToPixelSize(Baseline, fontSize);

        public float GetPixelClipedAscender(float fontSize) => ConvertToPixelSize(ClipedAscender, fontSize);

        public float GetPixelHeight(float fontSize) => ConvertToPixelSize(Height, fontSize);


        public float GetSpaceWidth(float fontSIze) => _spaceGlyph.GetPixelWidth(fontSIze);
    }
}