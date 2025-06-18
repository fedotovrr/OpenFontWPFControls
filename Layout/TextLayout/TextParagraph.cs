using System;
using System.Collections.Generic;

namespace OpenFontWPFControls.Layout
{
    public class TextParagraph
    {
        private readonly TextLayout _layout;
        private readonly int _charCount;
        private int _charOffset;

        private bool _valid;
        private GlyphLayout _glyphsLayout;
        private List<TextLine> _lines;

        
        public TextParagraph(int charOffset, int charCount, TextLayout layout)
        {
            _charOffset = charOffset;
            _charCount = charCount;
            _layout = layout ?? throw new NullReferenceException();
        }


        public TextLayout TextLayout => _layout;

        public int CharCount => _charCount;

        public int CharOffset
        {
            get => _charOffset;
            set => _charOffset = value;
        }

        public bool Valid => _valid;

        public GlyphLayout GlyphsLayout => _glyphsLayout;

        public StringCharacterBuffer GetBuffer() => new StringCharacterBuffer(TextLayout.Text, _charOffset, _charCount);

        public StringCharacterBuffer GetBuffer(string buffer) => new StringCharacterBuffer(buffer, _charOffset, _charCount);

        public bool CaretPointContains(int charOffset) => charOffset >= _charOffset && charOffset <= _charOffset + _charCount;

        public void Invalidate() => _valid = false;


        // Lines

        public TextLine this[int index] => GetLines()[index];

        public int LinesCount => GetLines().Count;

        private List<TextLine> GetLines()
        {
            if (!Valid)
            {
                _glyphsLayout = TextLayout.TypefaceInfo.GetGlyphLayout(GetBuffer());
                _lines = TextLayout.MaxWidth > 0 ? TextLayoutLogic.GetLines(this) : new List<TextLine>();
                _valid = true;
            }
            return _lines;
        }


        // Enumerators

        public IEnumerable<TextLine> Lines
        {
            get
            {
                foreach (TextLine line in GetLines())
                {
                    yield return line;
                }
            }
        }

        public IEnumerable<TextLine> ReverseLines
        {
            get
            {
                List<TextLine> lines = GetLines();
                for (int i = lines.Count - 1; i >= 0; i--)
                {
                    yield return lines[i];
                }
            }
        }


        // Debug

        public string Text => _layout.Text.Substring(_charOffset, _charCount);

        public override string ToString() => $"CharOffset: {_charOffset:0000} Length: {_charCount}";
    }
}
