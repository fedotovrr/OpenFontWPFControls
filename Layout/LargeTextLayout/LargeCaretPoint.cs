using System;

namespace OpenFontWPFControls.Layout
{
    public struct CaretPoint : IEquatable<CaretPoint>
    {
        public CaretPointOwners Owner;
        public int CharOffset;
        public float X;
        public float Y;

        public CaretPoint(CaretPointOwners owner = CaretPointOwners.Anyone, int charOffset = 0, float x = 0, float y = 0)
        {
            Owner = owner;
            CharOffset = charOffset;
            X = x;
            Y = y;
        }

        public bool Equals(CaretPoint other)
        {
            return CharOffset == other.CharOffset && 
                   (Owner == other.Owner || Owner == CaretPointOwners.Anyone || other.Owner == CaretPointOwners.Anyone);
        }

        public override string ToString() => $"CharOffset: {CharOffset:0000} X: {X:0000.0} Y: {Y:0000.0}";
    }

    public enum CaretPointOwners
    {
        StartLine,
        Glyph,
        EndLine,
        Anyone,
    }
}
