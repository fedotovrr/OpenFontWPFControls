using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace OpenFontWPFControls.Layout
{
    public class SimpleTextLayout
    {
        private string _text;
        private TextTrimming _trimming;
        private float _maxWidth;
        private float _width;
        private float _height;
        private float _fontSize;
        private float _fontHeight;
        private float _pixelsPerDip;
        private float _pixelsPerInch;
        private Brush _foreground;
        private bool _underline;
        private bool _strike;

        private readonly TypefaceInfo _typefaceInfo;
        private List<LargeTextParagraph> _paragraphs;
        private List<SimpleTextLine> _lines;
        private AreasTable _linesTable;

        public SimpleTextLayout(
            string text = null,
            TypefaceInfo typefaceInfo = null,
            float maxWidth = float.MaxValue,
            float fontSize = 14,
            float pixelsPerDip = 1,
            float pixelsPerInch = 96,
            Brush foreground = null,
            bool underline = false,
            bool strike = false)
        {
            _typefaceInfo = typefaceInfo ?? new TypefaceInfo();
            _maxWidth = maxWidth;
            _fontSize = fontSize;
            _pixelsPerDip = pixelsPerDip;
            _pixelsPerInch = pixelsPerInch;
            _text = text ?? String.Empty;
            _foreground = foreground ?? Brushes.Black;
            _underline = underline;
            _strike = strike;
            UpdateFontHeight();
            UpdateLines();
        }


        public TypefaceInfo TypefaceInfo => _typefaceInfo;

        public string Text
        {
            get => _text;
            set => PropertySetter(ref _text, value);
        }

        public TextTrimming TextTrimming
        {
            get => _trimming;
            set => PropertySetter(ref _trimming, value);
        }

        public float MaxWidth
        {
            get => _maxWidth;
            set => PropertySetter(ref _maxWidth, value);
        }

        public float FontSize
        {
            get => _fontSize;
            set => PropertySetter(ref _fontSize, value, UpdateFontHeight);
        }

        public float FontHeight => _fontHeight;

        public float PixelsPerDip
        {
            get => _pixelsPerDip;
            set => PropertySetter(ref _pixelsPerDip, value);
        }

        public float PixelsPerInch
        {
            get => _pixelsPerInch;
            set => PropertySetter(ref _pixelsPerInch, value, UpdateFontHeight);
        }

        public Brush Foreground
        {
            get => _foreground;
            set => PropertySetter(ref _foreground, value);
        }

        public bool Underline
        {
            get => _underline;
            set => PropertySetter(ref _underline, value);
        }

        public bool Strike
        {
            get => _strike;
            set => PropertySetter(ref _strike, value);
        }

        private void UpdateFontHeight()
        {
            _fontHeight = _fontSize / (72f / _pixelsPerInch);
        }

        public float Width => _width;

        public float Height => _height;

        private void PropertySetter<T>(ref T field, T value, Action callback = null)
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                UpdateLines();
                callback?.Invoke();
            }
        }


        // Lines

        public SimpleTextLine this[int index] => _lines[index];

        public int LinesCount => _lines.Count;

        private void UpdateLines()
        {
            _lines = new List<SimpleTextLine>();
            _width = 0;
            if (_maxWidth > 0)
            {
                float y = 0;
                foreach (ParagraphInfo paragraph in TextLayoutLogic.GetParagraphs(_text, 0))
                {
                    StringCharacterBuffer text = new StringCharacterBuffer(_text, paragraph.CharOffset, paragraph.CharCount);
                    GlyphPoint[] glyphLayout = TypefaceInfo.GetGlyphLayout(text);
                    IEnumerable<LineInfo> lineInfos = TextLayoutLogic.GetLines(
                        glyphs:   glyphLayout,
                        text:     text,
                        trimming: _trimming,
                        maxWidth: _maxWidth,
                        fontSize: _fontSize);
                    foreach (LineInfo info in lineInfos)
                    {
                        SimpleTextLine line = 
                            new SimpleTextLine(this,
                                glyphLayout, 
                                paragraph.CharOffset + info.CharOffset, 
                                info.CharsCount, 
                                info.GlyphOffset,
                                info.GlyphsCount,
                                y);
                        _lines.Add(line);
                        _width = Math.Max(_width, line.Width);
                        y += _fontHeight;
                    }
                }
            }
            _height = _lines.Count * _fontHeight;
            _linesTable = new AreasTable(_lines);
        }


        // Enumerators

        public IEnumerable<CaretPoint> CaretPoints => Lines.SelectMany(line => line.CaretPoints);

        public IEnumerable<SimpleTextLine> LinesInArea(Rect bounds)
        {
            return _linesTable.GetItems<SimpleTextLine>(bounds);
        }

        public IEnumerable<SimpleTextLine> Lines 
        {
            get
            {
                for (int i = 0; i < _lines.Count; i++)
                {
                    yield return _lines[i];
                }
            }
        }

        public IEnumerable<SimpleTextLine> ReverseLines
        {
            get
            {
                for (int i = _lines.Count - 1; i >= 0; i--)
                {
                    yield return _lines[i];
                }
            }
        }

        public IEnumerable<SimpleTextLine> NextLines(CaretPoint current)
        {
            return LinesIterator(current, () => Lines, l => l.CaretPoints);
        }

        public IEnumerable<SimpleTextLine> PreviousLines(CaretPoint current)
        {
            return LinesIterator(current, () => ReverseLines, l => l.ReverseCaretPoints);
        }

        public IEnumerable<CaretPoint> NextCaretPoints(CaretPoint current)
        {
            return CaretPointsIterator(current, () => Lines, line => line.CaretPoints,
                (lastOwner, curOwner) =>
                    curOwner == CaretPointOwners.Glyph && lastOwner == CaretPointOwners.StartLine ||
                    curOwner == CaretPointOwners.EndLine && lastOwner == CaretPointOwners.StartLine);
        }

        public IEnumerable<CaretPoint> PreviousCaretPoints(CaretPoint current)
        {
            return CaretPointsIterator(current, () => ReverseLines,
                line => line.ReverseCaretPoints,
                (last, cur) => cur == CaretPointOwners.StartLine && last == CaretPointOwners.Glyph ||
                               cur == CaretPointOwners.StartLine && last == CaretPointOwners.EndLine);
        }

        private static IEnumerable<CaretPoint> CaretPointsIterator(
            CaretPoint current,
            Func<IEnumerable<SimpleTextLine>> linesGetter,
            Func<SimpleTextLine, IEnumerable<CaretPoint>> caretPointsGetter,
            Func<CaretPointOwners, CaretPointOwners, bool> skip)
        {
            bool found = false;
            CaretPointOwners last = CaretPointOwners.Anyone;
            foreach (SimpleTextLine line in linesGetter())
            {
                if (found || line.CaretPointContains(current.CharOffset))
                {
                    foreach (CaretPoint point in caretPointsGetter(line))
                    {
                        if (found)
                        {
                            if (!skip(last, point.Owner))
                            {
                                yield return point;
                            }
                        }
                        else if (point.Equals(current))
                        {
                            found = true;
                        }

                        last = point.Owner;
                    }
                }
            }
        }

        private static IEnumerable<SimpleTextLine> LinesIterator(
            CaretPoint current,
            Func<IEnumerable<SimpleTextLine>> linesGetter,
            Func<SimpleTextLine, IEnumerable<CaretPoint>> caretPointsGetter,
            bool withOutCurrent = true)
        {
            bool found = false;
            foreach (SimpleTextLine line in linesGetter())
            {
                if (found)
                {
                    yield return line;
                }
                else if (line.CaretPointContains(current.CharOffset))
                {
                    foreach (CaretPoint point in caretPointsGetter(line))
                    {
                        if (point.Equals(current))
                        {
                            found = true;
                            if (!withOutCurrent)
                            {
                                yield return line;
                            }

                            break;
                        }
                    }
                }
            }
        }



        // Tools

        public CaretPoint FirstCaretPoint => _lines.FirstOrDefault()?.CaretPoints.First() ?? new CaretPoint(CaretPointOwners.StartLine);

        public CaretPoint LastCaretPoint => _lines.LastOrDefault()?.CaretPoints.Last() ?? new CaretPoint(CaretPointOwners.EndLine, Text.Length);


        public bool CaretPointContains(int offset)
        {
            return offset >= 0 && offset <= Text.Length;
        }

        public bool GetPoint(CaretPoint current, int delta, out CaretPoint result)
        {
            using IEnumerator<CaretPoint> pointsEnum = (delta > 0 ? NextCaretPoints(current) : PreviousCaretPoints(current)).GetEnumerator();
            delta = Math.Abs(delta);
            bool change = false;
            while (delta > 0 && pointsEnum.MoveNext())
            {
                delta--;
                change = true;
            }

            result = change ? pointsEnum.Current : current;
            return change;
        }

        public SimpleTextLine GetLine(CaretPoint current)
        {
            using IEnumerator<SimpleTextLine> lineEnum = LinesIterator(current, () => Lines, l => l.CaretPoints, false).GetEnumerator();
            return lineEnum.MoveNext() ? lineEnum.Current : null;
        }

        public SimpleTextLine GetLine(CaretPoint current, int delta)
        {
            if (delta == 0)
            {
                return GetLine(current);
            }

            using IEnumerator<SimpleTextLine> lineEnum = (delta > 0 ? NextLines(current) : PreviousLines(current)).GetEnumerator();
            delta = Math.Abs(delta);
            while (delta > 0 && lineEnum.MoveNext())
            {
                delta--;
            }

            return lineEnum.Current;
        }

        public CaretPoint CheckCaretPoint(CaretPoint target)
        {
            if (target.CharOffset < 0)
            {
                return FirstCaretPoint;
            }

            if (target.CharOffset >= Text.Length)
            {
                return LastCaretPoint;
            }

            CaretPoint last = FirstCaretPoint;
            foreach (SimpleTextLine line in Lines)
            {
                if (line.CaretPointContains(target.CharOffset))
                {
                    foreach (CaretPoint point in line.CaretPoints)
                    {
                        if (point.Equals(target))
                        {
                            return point;
                        }
                        if (point.CharOffset > target.CharOffset)
                        {
                            return last;
                        }
                    }
                }
            }

            return last;
        }
    }
}