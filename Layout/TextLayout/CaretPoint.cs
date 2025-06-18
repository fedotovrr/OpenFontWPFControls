using System;

namespace OpenFontWPFControls.Layout
{
    public struct CaretPoint : IEquatable<CaretPoint>
    {
        public CaretPointOwners Owner;
        public int CharOffset;
        public float X;

        public CaretPoint(CaretPointOwners owner = CaretPointOwners.Anyone, int charOffset = 0, float x = 0)
        {
            Owner = owner;
            CharOffset = charOffset;
            X = x;
        }

        public bool Equals(CaretPoint other)
        {
            return CharOffset == other.CharOffset && 
                   (Owner == other.Owner || Owner == CaretPointOwners.Anyone || other.Owner == CaretPointOwners.Anyone);
        }

        public override string ToString() => $"CharOffset: {CharOffset:0000} X: {X}";
    }

    public enum CaretPointOwners
    {
        StartLine,
        Glyph,
        EndLine,
        Anyone,
    }
}
