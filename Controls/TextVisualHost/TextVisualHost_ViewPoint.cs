using System;
using System.Collections.Generic;
using System.Windows;
using OpenFontWPFControls.Layout;

namespace OpenFontWPFControls.Controls
{
    internal partial class TextVisualHost
    {
        private IEnumerable<(TextLine line, double y)> LineInViewBounds()
        {
            if (_drawingBounds.Height > 0)
            {
                foreach ((TextLine line, double y) tuple in ViewLine())
                {
                    double ymax = tuple.y + tuple.line.Height;
                    if (tuple.y < _drawingBounds.Y + _drawingBounds.Height && ymax > _drawingBounds.Y)
                    {
                        yield return tuple;
                    }
                }
            }
        }

        private IEnumerable<(TextLine line, double y)> ViewLine()
        {
            if (_layout != null)
            {
                double height = 0;
                int lineIndex = _startLineIndex;
                for (int p = _startParagraphIndex; p < _layout.ParagraphsCount; p++)
                {
                    for (int l = lineIndex; l < _layout[p].LinesCount; l++)
                    {
                        yield return (_layout[p][l], Math.Round(height));
                        height += _layout[p][l].Height;
                        if (height >= _maxSize.Height)
                        {
                            break;
                        }
                    }
                    if (height >= _maxSize.Height)
                    {
                        break;
                    }
                    lineIndex = 0;
                }
            }
        }

        public bool SetStartChar(int charIndex, bool updateDrawing = true)
        {
            charIndex = charIndex < 0 ? 0 : charIndex > _layout.Text.Length - 1 ? _layout.Text.Length - 1 : charIndex;
            int paragraphIndex = WhereCount(_layout.Paragraphs, p => p.CharOffset <= charIndex) - 1;
            paragraphIndex = paragraphIndex < 0 ? 0 : paragraphIndex;
            int lineIndex = WhereCount(_layout[paragraphIndex].Lines, l => l.GlobalCharOffset <= charIndex) - 1;
            lineIndex = lineIndex < 0 ? 0 : lineIndex;
            if (_startLineIndex != lineIndex || _startParagraphIndex != paragraphIndex)
            {
                _startLineIndex = lineIndex;
                _startParagraphIndex = paragraphIndex;
                _startCharOffset = _layout[_startParagraphIndex][_startLineIndex].GlobalCharOffset;
                _nextLineCharOffset = _startLineIndex + 1 < _layout[_startParagraphIndex].LinesCount 
                    ? _layout[_startParagraphIndex][_startLineIndex + 1].GlobalCharOffset : _startParagraphIndex + 1 < _layout.ParagraphsCount 
                        ? _layout[_startParagraphIndex + 1].CharOffset : _layout.Text.Length - 1;
                if (updateDrawing)
                {
                    InvalidateDrawing();
                }
                return true;
            }
            return false;

            static int WhereCount<T>(IEnumerable<T> source, Predicate<T> match)
            {
                int count = 0;
                if (match != null)
                {
                    foreach (T current in source)
                    {
                        if (!match(current))
                            return count;
                        count++;
                    }
                }
                return count;
            }
        }

        public void ChangeStartLine(int delta)
        {
            int paragraphIndex = _startParagraphIndex;
            int lineIndex = _startLineIndex;
            if (delta > 0)
            {
                DoNext();
            }
            else
            {
                DoBack();
            }
            if (paragraphIndex != _startParagraphIndex || lineIndex != _startLineIndex)
            {
                _startParagraphIndex = paragraphIndex;
                _startLineIndex = lineIndex;
                _startCharOffset = _layout[_startParagraphIndex][_startLineIndex].GlobalCharOffset;
                _nextLineCharOffset = _startLineIndex + 1 < _layout[_startParagraphIndex].LinesCount
                    ? _layout[_startParagraphIndex][_startLineIndex + 1].GlobalCharOffset : _startParagraphIndex + 1 < _layout.ParagraphsCount
                        ? _layout[_startParagraphIndex + 1].CharOffset : _layout.Text.Length - 1;
                InvalidateDrawing();
            }

            return;

            void DoNext()
            {
                for (int p = paragraphIndex; p < _layout.ParagraphsCount; p++)
                {
                    for (int l = lineIndex; l < _layout[p].LinesCount; l++)
                    {
                        if (delta == 0)
                        {
                            paragraphIndex = p;
                            lineIndex = l;
                            return;
                        }
                        delta--;
                    }
                    lineIndex = 0;
                }
                paragraphIndex = _layout.ParagraphsCount - 1;
                lineIndex = _layout[paragraphIndex].LinesCount - 1;
            }

            void DoBack()
            {

                for (int p = paragraphIndex; p >= 0; p--)
                {
                    for (int l = lineIndex; l >= 0; l--)
                    {
                        if (delta == 0)
                        {
                            paragraphIndex = p;
                            lineIndex = l;
                            return;
                        }
                        delta++;
                    }
                    lineIndex = p - 1 >= 0 ? _layout[p - 1].LinesCount - 1 : 0;
                }
                paragraphIndex = 0;
                lineIndex = 0;
            }
        }

        public void CaretIntoView()
        {
            TextLine first = null;
            TextLine last = null;
            int count = 0;
            double height = 0;
            foreach ((TextLine line, double _) in ViewLine())
            {
                first ??= line;
                last = line;
                count++;
                height += line.Height;
            }
            if (first != null && last != null && count > 0 && _layout?.GetLine(_caretPoint) is TextLine curLine)
            {
                int currentCharOffset = curLine.GlobalCharOffset;
                if (currentCharOffset < first.GlobalCharOffset)
                {
                    SetStartChar(currentCharOffset);
                }
                else if (currentCharOffset >= last.GlobalCharOffset + (height > _maxSize.Height ? 0 : last.CharCount))
                {
                    if (count > 2)
                    {
                        SetStartChar(currentCharOffset, false);
                        ChangeStartLine(0 - count + 2);
                    }
                    else
                    {
                        SetStartChar(currentCharOffset);
                    }
                }

                foreach (CaretPoint point in curLine.CaretPoints)
                {
                    if (point.Equals(_caretPoint))
                    {
                        if (point.X < 0 - _viewXOffset)
                        {
                            SetXOffset(Math.Ceiling(0 - point.X));
                        }
                        else if (point.X > _maxSize.Width - _viewXOffset)
                        {
                            SetXOffset(Math.Floor(0 - (point.X - _maxSize.Width)));
                        }
                        break;
                    }
                }
            }
        }

        public void SetXOffset(double value)
        {
            if (Math.Abs(_viewXOffset - value) > double.Epsilon)
            {
                _viewXOffset = Math.Round(value);
                _backgroundLayer.Offset = new Vector(_viewXOffset, 0);
                _errorsLayer.Offset = new Vector(_viewXOffset, 0);
                _controlLayer.Offset = new Vector(_viewXOffset, 0);
                _caret.Offset = new Vector(_viewXOffset, 0);
                _linesLayer.ForEach(child => child.Visual.Offset = new Vector(_viewXOffset, child.Visual.Offset.Y));
                OnXOffsetChangeCallBack?.Invoke();
            }
        }
        
    }
}