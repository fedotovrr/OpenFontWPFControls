using System.Linq;
using Typography.OpenFont;
using Typography.OpenFont.Tables;

namespace OpenFontWPFControls.Layout
{
    public class GlyphLayout : HoleyCollection<GlyphPoint>, IGlyphIndexList, IGlyphPositions
    {
        public GlyphLayout(int maxLength) : base(maxLength)
        {

        }


        // IGlyphIndexList

        int IGlyphIndexList.Count => this.Count;

        ushort IGlyphIndexList.this[int index] => this[index].GlyphIndex;

        void IGlyphIndexList.Replace(int index, ushort newGlyphIndex)
        {
            //GlyphPoints[index] = new GlyphPoint(newGlyphIndex, GlyphPoints[index].CharOffset);
            this.Replace(index, new GlyphPoint(newGlyphIndex, this[index].CharOffset));
        }

        void IGlyphIndexList.Replace(int index, int removeLen, ushort newGlyphIndex)
        {
            //GlyphPoints[index] = new GlyphPoint(newGlyphIndex, GlyphPoints[index].CharOffset);
            //GlyphPoints.RemoveRange(index + 1, removeLen - 1);
            this.Replace(index, removeLen, new GlyphPoint(newGlyphIndex, this[index].CharOffset));
        }

        void IGlyphIndexList.Replace(int index, ushort[] newGlyphIndices)
        {
            //GlyphPoints[index] = new GlyphPoint(newGlyphIndices[0], GlyphPoints[index].CharOffset);
            //GlyphPoints.InsertRange(index + 1, newGlyphIndices.Skip(1).Select(i => new GlyphPoint(i, GlyphPoints[index].CharOffset)));
            this.Replace(index, 1, newGlyphIndices.Select(i => new GlyphPoint(i, this[index].CharOffset)).ToArray());
        }


        // IGlyphPositions

        int IGlyphPositions.Count => this.Count;

        GlyphClassKind IGlyphPositions.GetGlyphClassKind(int index)
        {
            return this[index].GlyphLayoutBuilder.Typeface.GetGlyph(this[index].GlyphIndex).GlyphClass;
        }

        void IGlyphPositions.AppendGlyphOffset(int index, short appendOffsetX, short appendOffsetY)
        {
            this[index].GlyphOffsetX += appendOffsetX;
            this[index].GlyphOffsetY += appendOffsetY;
        }

        void IGlyphPositions.AppendGlyphAdvance(int index, short appendAdvX, short appendAdvY)
        {
            this[index].Width += appendAdvX;
        }

        ushort IGlyphPositions.GetGlyph(int index, out short advW)
        {
            advW = this[index].Width;
            return this[index].GlyphIndex;
        }

        ushort IGlyphPositions.GetGlyph(int index, out ushort inputOffset, out short offsetX, out short offsetY, out short advW)
        {
            inputOffset = 0; //non use
            offsetX = this[index].GlyphOffsetX;
            offsetY = this[index].GlyphOffsetY;
            advW = this[index].Width;
            return this[index].GlyphIndex;
        }

        void IGlyphPositions.GetOffset(int index, out short offsetX, out short offsetY)
        {
            offsetX = this[index].GlyphOffsetX;
            offsetY = this[index].GlyphOffsetY;
        }
    }
}