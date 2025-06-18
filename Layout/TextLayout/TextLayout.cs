using OpenFontWPFControls.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace OpenFontWPFControls.Layout
{
    public class TextLayout
    {
        private string _text;
        private TextTrimming _trimming;
        private float _maxWidth;
        private float _width = float.PositiveInfinity;
        private float _height = float.PositiveInfinity;
        private float _fontSize;
        private float _fontHeight;
        private float _pixelsPerDip;
        private float _pixelsPerInch;
        private Brush _foreground;
        private bool _underline;
        private bool _strike;
        private bool _recycling;

        private readonly TypefaceInfo _typefaceInfo;
        private List<TextParagraph> _paragraphs;

        public TextLayout(
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
        }


        public TypefaceInfo TypefaceInfo => _typefaceInfo;

        public string Text
        {
            get => _text;
            set => UpdateParagraphs(value);
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

        /// <summary>
        /// Check changes and use matched paragraphs.
        /// Use <see langword="true" />  if the text has a lot of emoji, but few paragraphs.
        /// Use <see langword="false" /> if the text has a lot of paragraphs.
        /// </summary>
        public bool Recycling
        {
            get => _recycling;
            set => _recycling = value;
        }

        private void UpdateFontHeight()
        {
            _fontHeight = _fontSize / (72f / _pixelsPerInch);
        }

        private void PropertySetter<T>(ref T field, T value, Action callback = null)
        {
            if (!field.Equals(value))
            {
                field = value;
                _paragraphs?.ForEach(p => p.Invalidate());
                callback?.Invoke();
            }
        }


        // Paragraphs

        public TextParagraph this[int index] => GetParagraphs()[index];

        public int ParagraphsCount => GetParagraphs().Count;

        private List<TextParagraph> GetParagraphs()
        {
            return _paragraphs ??= _text.GetParagraphs(0, this).ToList();
        }

        private void UpdateParagraphs(string newText)
        {
            if (_text == newText)
            {
                return;
            }

            if (_recycling && _paragraphs != null)
            {
                int invalidateStart = 0;
                int invalidateEnd = _paragraphs.Count;
                int offset = 0;
                using IEnumerator<TextParagraph> oldEnum = _paragraphs.GetEnumerator();
                using IEnumerator<TextParagraph> newEnum = newText.GetParagraphs(0, this).GetEnumerator();
                while (oldEnum.MoveNext() && newEnum.MoveNext())
                {
                    if (!oldEnum.Current.GetBuffer().Equals(newEnum.Current.GetBuffer(newText)))
                    {
                        break;
                    }

                    offset = newEnum.Current.CharOffset + newEnum.Current.CharCount + 1;
                    invalidateStart++;
                }

                if (offset > _text.Length)
                {
                    _paragraphs.RemoveRange(invalidateStart, invalidateEnd - invalidateStart);
                }
                else
                {
                    List<TextParagraph> newParagraphs = newText.GetParagraphs(offset, this).ToList();

                    int oldIndex = _paragraphs.Count - 1;
                    int newIndex = newParagraphs.Count - 1;
                    while (invalidateEnd > invalidateStart && newIndex >= 0)
                    {
                        if (!_paragraphs[oldIndex].GetBuffer().Equals(newParagraphs[newIndex].GetBuffer(newText)))
                        {
                            break;
                        }

                        _paragraphs[oldIndex].CharOffset = newParagraphs[newIndex].CharOffset;
                        invalidateEnd--;
                        oldIndex--;
                        newIndex--;
                    }

                    _paragraphs.RemoveRange(invalidateStart, invalidateEnd - invalidateStart);
                    _paragraphs.InsertRange(invalidateStart, newParagraphs.Take(++newIndex));
                }
            }
            else
            {
                _paragraphs = null;
            }

            _text = newText;
        }


        // Enumerators

        public IEnumerable<TextParagraph> Paragraphs
        {
            get
            {
                foreach (TextParagraph paragraph in GetParagraphs())
                    yield return paragraph;
            }
        }

        public IEnumerable<TextParagraph> ReverseParagraphs
        {
            get
            {
                List<TextParagraph> paragraphs = GetParagraphs();
                for (int i = paragraphs.Count - 1; i >= 0; i--)
                {
                    yield return paragraphs[i];
                }
            }
        }

        public IEnumerable<TextLine> NextLines(CaretPoint current)
        {
            return LinesIterator(current, () => Paragraphs, p => p.Lines, l => l.CaretPoints);
        }

        public IEnumerable<TextLine> PreviousLines(CaretPoint current)
        {
            return LinesIterator(current, () => ReverseParagraphs, p => p.ReverseLines, l => l.ReverseCaretPoints);
        }

        public IEnumerable<CaretPoint> NextCaretPoints(CaretPoint current)
        {
            return CaretPointsIterator(current, () => Paragraphs, par => par.Lines, line => line.CaretPoints,
                (lastOwner, curOwner) =>
                    curOwner == CaretPointOwners.Glyph && lastOwner == CaretPointOwners.StartLine ||
                    curOwner == CaretPointOwners.EndLine && lastOwner == CaretPointOwners.StartLine);
        }

        public IEnumerable<CaretPoint> PreviousCaretPoints(CaretPoint current)
        {
            return CaretPointsIterator(current, () => ReverseParagraphs, par => par.ReverseLines,
                line => line.ReverseCaretPoints,
                (last, cur) => cur == CaretPointOwners.StartLine && last == CaretPointOwners.Glyph ||
                               cur == CaretPointOwners.StartLine && last == CaretPointOwners.EndLine);
        }

        private static IEnumerable<CaretPoint> CaretPointsIterator(
            CaretPoint current,
            Func<IEnumerable<TextParagraph>> paragraphsGetter,
            Func<TextParagraph, IEnumerable<TextLine>> linesGetter,
            Func<TextLine, IEnumerable<CaretPoint>> caretPointsGetter,
            Func<CaretPointOwners, CaretPointOwners, bool> skip)
        {
            bool found = false;
            CaretPointOwners last = CaretPointOwners.Anyone;
            foreach (TextParagraph paragraph in paragraphsGetter())
            {
                if (found || paragraph.CaretPointContains(current.CharOffset))
                {
                    foreach (TextLine line in linesGetter(paragraph))
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
            }
        }

        private static IEnumerable<TextLine> LinesIterator(
            CaretPoint current,
            Func<IEnumerable<TextParagraph>> paragraphsGetter,
            Func<TextParagraph, IEnumerable<TextLine>> linesGetter,
            Func<TextLine, IEnumerable<CaretPoint>> caretPointsGetter,
            bool withOutCurrent = true)
        {
            bool found = false;
            foreach (TextParagraph paragraph in paragraphsGetter())
            {
                if (found || paragraph.CaretPointContains(current.CharOffset))
                {
                    foreach (TextLine line in linesGetter(paragraph))
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
            }
        }

        // Full pass not recommended, calculating all model

        public IEnumerable<CaretPoint> CaretPoints =>
            Paragraphs.SelectMany(paragraph => paragraph.Lines.SelectMany(line => line.CaretPoints));

        public IEnumerable<GlyphPoint> Glyphs =>
            Paragraphs.SelectMany(paragraph => paragraph.Lines.SelectMany(line => line.Glyphs));

        public IEnumerable<(GlyphPoint, float)> GlyphPoints =>
            Paragraphs.SelectMany(paragraph => paragraph.Lines.SelectMany(line => line.GlyphPoints));


        // Tools

        /// <summary>
        /// Full Width (calculating all model)
        /// </summary>
        public float Width => Paragraphs.Max(p => p.Lines.Max(l => l.Glyphs.Sum(g => g.GetPixelWidth(_fontHeight))));

        /// <summary>
        /// Full Height (calculating all model)
        /// </summary>
        public float Height => Paragraphs.Sum(p => p.Lines.Count()) * _fontHeight;


        public CaretPoint FirstCaretPoint => new CaretPoint(CaretPointOwners.StartLine);

        public CaretPoint LastCaretPoint => new CaretPoint(CaretPointOwners.EndLine, Text.Length);


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

        public TextLine GetLine(CaretPoint current)
        {
            using IEnumerator<TextLine> lineEnum = LinesIterator(current, () => Paragraphs, p => p.Lines, l => l.CaretPoints, false).GetEnumerator();
            return lineEnum.MoveNext() ? lineEnum.Current : null;
        }

        public TextLine GetLine(CaretPoint current, int delta)
        {
            if (delta == 0)
            {
                return GetLine(current);
            }

            using IEnumerator<TextLine> lineEnum = (delta > 0 ? NextLines(current) : PreviousLines(current)).GetEnumerator();
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
            foreach (TextParagraph paragraph in Paragraphs)
            {
                if (paragraph.CaretPointContains(target.CharOffset))
                {
                    foreach (TextLine line in paragraph.Lines)
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
                }
            }

            return last;
        }
    }
}