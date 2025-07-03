using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenFontWPFControls.Layout
{
    public class LargeTextParagraph
    {
        private readonly LargeTextLayout _layout;
        private readonly int _charCount;
        private int _charOffset;

        private bool _valid;
        private GlyphLayout _glyphsLayout;
        private List<LargeTextLine> _lines;

        
        public LargeTextParagraph(ParagraphInfo info, LargeTextLayout layout)
        {
            _charOffset = info.CharOffset;
            _charCount = info.CharCount;
            _layout = layout ?? throw new NullReferenceException();
        }

        public LargeTextParagraph(int charOffset, int charCount, LargeTextLayout layout)
        {
            _charOffset = charOffset;
            _charCount = charCount;
            _layout = layout ?? throw new NullReferenceException();
        }


        public LargeTextLayout TextLayout => _layout;

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

        public LargeTextLine this[int index] => GetLines()[index];

        public int LinesCount => GetLines().Count;

        private List<LargeTextLine> GetLines()
        {
            if (!Valid)
            {
                _glyphsLayout = TextLayout.TypefaceInfo.GetGlyphLayout(GetBuffer());
                _lines = TextLayout.MaxWidth > 0 ?
                    TextLayoutLogic.GetLines(
                            glyphs:   GlyphsLayout.GlyphPoints,
                            text:     GetBuffer(),
                            trimming: TextLayout.TextTrimming,
                            maxWidth: TextLayout.MaxWidth,
                            fontSize: TextLayout.FontSize).Select(info => new LargeTextLine(info, this)).ToList() :
                    new List<LargeTextLine>();
                _valid = true;
            }
            return _lines;
        }


        // Enumerators

        public IEnumerable<LargeTextLine> Lines
        {
            get
            {
                foreach (LargeTextLine line in GetLines())
                {
                    yield return line;
                }
            }
        }

        public IEnumerable<LargeTextLine> ReverseLines
        {
            get
            {
                List<LargeTextLine> lines = GetLines();
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
