using System.Linq;

namespace OpenFontWPFControls.Layout
{
    public partial class GlyphLayoutBuilder
    {
        private GlyphPoint _spaceGlyph;

        public System.Windows.Media.GlyphTypeface GlyphTypeface { get; set; }

        public GlyphPoint SpaceGlyph => _spaceGlyph;


        private void InitTools()
        {
            _spaceGlyph = Build(new StringCharacterBuffer(" ")).GlyphPoints.First();
            _spaceGlyph.GlyphLayoutBuilder = this;
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