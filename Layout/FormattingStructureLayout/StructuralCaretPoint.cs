using System;
using OpenFontWPFControls.Layout.FormattingStructureLayout;

namespace OpenFontWPFControls.Layout;

public struct StructuralCaretPoint : IEquatable<StructuralCaretPoint>
{
    public StructuralTextItem Text;
    public CaretPointOwners Owner;
    public int GlobalCharOffset;
    public int CharOffset;
    public int Length;
    public float X;
    public float Y;
    public float Height;

    public StructuralCaretPoint(StructuralTextItem text, CaretPointOwners owner = CaretPointOwners.Anyone, int globalCharOffset = 0, int charOffset = 0, int length = 0, float x = 0, float y = 0, float height = 0)
    {
        Text = text;
        Owner = owner;
        GlobalCharOffset = globalCharOffset;
        CharOffset = charOffset;
        Length = length;
        X = x;
        Y = y;
        Height = height;
    }

    public bool Equals(StructuralCaretPoint other)
    {
        return GlobalCharOffset == other.GlobalCharOffset &&
               (Owner == other.Owner || Owner == CaretPointOwners.Anyone || other.Owner == CaretPointOwners.Anyone);
    }

    public override string ToString() => $"GlobalCharOffset: {GlobalCharOffset:0000} X: {X} Y: {Y}";
}