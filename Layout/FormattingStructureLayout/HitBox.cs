namespace OpenFontWPFControls.Layout.FormattingStructureLayout
{
    public class HitBox : IPlacement
    {
        public readonly StructuralTextItem Source;
        public float XOffset;
        public float YOffset;
        public float Width;
        public float Height;

        float IPlacement.XOffset => XOffset;

        float IPlacement.YOffset => YOffset;

        float IPlacement.Width => Width;

        float IPlacement.Height => Height;


        public HitBox(StructuralTextItem source, float x, float y, float width, float height)
        {
            Source = source;
            XOffset = x; 
            YOffset = y;
            Width = width;
            Height = height;
        }
    }
}
