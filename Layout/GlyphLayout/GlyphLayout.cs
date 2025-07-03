using System.Collections.Generic;
using System.Linq;
using Typography.OpenFont;
using Typography.OpenFont.Tables;

namespace OpenFontWPFControls.Layout
{
    public class GlyphLayout : IGlyphIndexList, IGlyphPositions
    {
        public List<GlyphPoint> GlyphPoints; // todo try replace with fast structure

        public GlyphLayout(int maxLength)
        {
            GlyphPoints = new List<GlyphPoint>(maxLength);
        }

        int IGlyphIndexList.Count => GlyphPoints.Count;

        ushort IGlyphIndexList.this[int index] => GlyphPoints[index].GlyphIndex;

        void IGlyphIndexList.Replace(int index, ushort newGlyphIndex)
        {
            GlyphPoints[index] = new GlyphPoint(newGlyphIndex, GlyphPoints[index].CharOffset);
        }

        void IGlyphIndexList.Replace(int index, int removeLen, ushort newGlyphIndex)
        {
            GlyphPoints[index] = new GlyphPoint(newGlyphIndex, GlyphPoints[index].CharOffset);
            GlyphPoints.RemoveRange(index + 1, removeLen - 1);
        }

        void IGlyphIndexList.Replace(int index, ushort[] newGlyphIndices)
        {
            GlyphPoints[index] = new GlyphPoint(newGlyphIndices[0], GlyphPoints[index].CharOffset);
            GlyphPoints.InsertRange(index + 1, newGlyphIndices.Skip(1).Select(i => new GlyphPoint(i, GlyphPoints[index].CharOffset)));
        }


        int IGlyphPositions.Count => GlyphPoints.Count;

        GlyphClassKind IGlyphPositions.GetGlyphClassKind(int index)
        {
            return GlyphPoints[index].GlyphLayoutBuilder.Typeface.GetGlyph(GlyphPoints[index].GlyphIndex).GlyphClass;
        }

        void IGlyphPositions.AppendGlyphOffset(int index, short appendOffsetX, short appendOffsetY)
        {
            GlyphPoints[index].GlyphOffsetX += appendOffsetX;
            GlyphPoints[index].GlyphOffsetY += appendOffsetY;
        }

        void IGlyphPositions.AppendGlyphAdvance(int index, short appendAdvX, short appendAdvY)
        {
            GlyphPoints[index].Width += appendAdvX;
        }

        ushort IGlyphPositions.GetGlyph(int index, out short advW)
        {
            advW = GlyphPoints[index].Width;
            return GlyphPoints[index].GlyphIndex;
        }

        ushort IGlyphPositions.GetGlyph(int index, out ushort inputOffset, out short offsetX, out short offsetY, out short advW)
        {
            inputOffset = 0; //non use
            offsetX = GlyphPoints[index].GlyphOffsetX;
            offsetY = GlyphPoints[index].GlyphOffsetY;
            advW = GlyphPoints[index].Width;
            return GlyphPoints[index].GlyphIndex;
        }

        void IGlyphPositions.GetOffset(int index, out short offsetX, out short offsetY)
        {
            offsetX = GlyphPoints[index].GlyphOffsetX;
            offsetY = GlyphPoints[index].GlyphOffsetY;
        }
    }
}