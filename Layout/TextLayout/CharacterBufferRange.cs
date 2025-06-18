using System;

namespace OpenFontWPFControls.Layout
{
    public class CharacterBufferRange
    {
        private CharacterBuffer _buffer;
        private int _length;
    }

    public abstract class CharacterBuffer : IEquatable<CharacterBuffer>
    {
        public abstract char this[int index] { get; }

        public abstract int Length { get; }

        public abstract CharacterBuffer GetSubBuffer(int offset, int length);

        public bool Equals(CharacterBuffer other)
        {
            if (other != null && Length == other.Length)
            {
                for (int i = 0; i < Length; i++)
                {
                    if (this[i] != other[i])
                        return false;
                }
                return true;
            }
            return false;
        }
    }

    public class StringCharacterBuffer : CharacterBuffer
    {
        private readonly string _buffer = string.Empty;
        private readonly int _offset;
        private readonly int _length;

        public override char this[int index] => _buffer[_offset + index];

        public override int Length => _length;

        public StringCharacterBuffer(string buffer = null)
        {
            _buffer = buffer ?? string.Empty;
            _length = _buffer.Length;
        }

        public StringCharacterBuffer(string buffer, int offset, int length)
        {
            _buffer = buffer ?? string.Empty;
            _offset = offset;
            _length = length;
        }

        public override CharacterBuffer GetSubBuffer(int offset, int length)
        {
            return new StringCharacterBuffer(_buffer, _offset + offset, length);
        }
    }

    public class CharArrayCharacterBuffer : CharacterBuffer
    {
        private readonly char[] _buffer;
        private readonly int _offset;
        private readonly int _length;

        public override char this[int index] => _buffer[_offset + index];

        public override int Length => _length;

        public CharArrayCharacterBuffer(char[] buffer = null)
        {
            _buffer = buffer ?? Array.Empty<char>();
            _length = _buffer.Length;
        }

        public CharArrayCharacterBuffer(char[] buffer, int offset, int length)
        {
            _buffer = buffer ?? Array.Empty<char>();
            _offset = offset;
            _length = length;
        }

        public override CharacterBuffer GetSubBuffer(int offset, int length)
        {
            return new CharArrayCharacterBuffer(_buffer, _offset + offset, length);
        }
    }
}
