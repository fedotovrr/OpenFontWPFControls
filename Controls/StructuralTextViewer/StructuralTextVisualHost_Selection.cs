using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Input;
using OpenFontWPFControls.Layout;

namespace OpenFontWPFControls.Controls
{
    internal partial class StructuralTextVisualHost
    {
        //public bool SetCaretPosition(int offset)
        //{
        //    return SetCaretPosition(new CaretPoint(CaretPointOwners.Anyone, offset));
        //}

        //public bool SetCaretPosition(CaretPoint point)
        //{
        //    if (_layout != null)
        //    {
        //        point = _layout.CheckCaretPoint(point);
        //        if (!_caretPoint.Equals(point) || !_selectionCapture.Equals(point))
        //        {
        //            _caretPoint = _selectionCapture = point;
        //            UpdateControlLayer();
        //            CaretIntoView();
        //            _captureCaretXOffset = _caretPoint.X;
        //            OnSelectionChangeCallBack?.Invoke();
        //            return true;
        //        }
        //    }
        //    return false;
        //}

        //public bool ChangeCaretPositionByLine(int delta, bool changeCapture = true)
        //{
        //    if (_layout?.GetLine(_caretPoint, delta)?.CaretPoints.LastOrDefault(p => p.X <= _captureCaretXOffset) is CaretPoint cp &&
        //        (!_caretPoint.Equals(cp) || !_selectionCapture.Equals(cp)))
        //    {
        //        if (changeCapture)
        //        {
        //            _selectionCapture = cp;
        //        }
        //        _caretPoint = cp;
        //        UpdateControlLayer();
        //        CaretIntoView();
        //        OnSelectionChangeCallBack?.Invoke();
        //        return true;
        //    }
        //    return false;
        //}

        //public bool ChangeCaretPosition(int delta)
        //{
        //    if (_layout?.GetPoint(_caretPoint, delta, out CaretPoint cp) == true && (!_caretPoint.Equals(cp) || !_selectionCapture.Equals(cp)))
        //    {
        //        _caretPoint = _selectionCapture = cp;
        //        UpdateControlLayer();
        //        CaretIntoView();
        //        _captureCaretXOffset = _caretPoint.X;
        //        OnSelectionChangeCallBack?.Invoke();
        //        return true;
        //    }
        //    return false;
        //}

        public void SetCaretPosition(Point point)
        {
            point += _drawingOffset;
            if (GetPositionByPoint(point, out StructuralCaretPoint cp))
            {
                if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                {
                    SetSelection(_selectionCapture, cp);
                }
                else if (!_caretPoint.Equals(cp) || !_selectionCapture.Equals(cp))
                {
                    _caretPoint = _selectionCapture = cp;
                    UpdateControlLayer();
                    _captureCaretXOffset = _caretPoint.X;
                    OnSelectionChangeCallBack?.Invoke();
                }
            }
        }


        public void ChangeSelection(Point point)
        {
            point += _drawingOffset;
            if (GetPositionByPoint(point, out StructuralCaretPoint cp))
            {
                SetSelection(_selectionCapture, cp);
            }
        }

        public void SetSelection(StructuralCaretPoint capture, StructuralCaretPoint caret)
        {
            StructuralCaretPoint start = capture.GlobalCharOffset < caret.GlobalCharOffset ? capture : caret;
            StructuralCaretPoint end = capture.GlobalCharOffset > caret.GlobalCharOffset ? capture : caret;
            if ((SelectionStart != start.GlobalCharOffset || SelectionEnd != end.GlobalCharOffset) && _layout != null &&
                start.GlobalCharOffset >= 0 && end.GlobalCharOffset >= 0)
            {
                _selectionCapture = capture;
                _caretPoint = caret.GlobalCharOffset > capture.GlobalCharOffset ? end : start;
                UpdateControlLayer();
                //CaretIntoView();
                _captureCaretXOffset = _caretPoint.X;
                OnSelectionChangeCallBack?.Invoke();
            }
        }

        public void SetSelection(int start, int length)
        {
            if (_layout != null)
            {
                StructuralCaretPoint startPoint = new StructuralCaretPoint(null, CaretPointOwners.Anyone, start);
                StructuralCaretPoint endPoint = new StructuralCaretPoint(null, CaretPointOwners.Anyone, start + length);
                SetSelection(startPoint, endPoint);
            }
        }

        public void ChangeSelection(StructuralCaretPoint caret)
        {
            SetSelection(_selectionCapture, caret);
        }

        //public void ChangeSelection(int delta)
        //{
        //    if (_layout?.GetPoint(_caretPoint, delta, out CaretPoint result) == true)
        //    {
        //        SetSelection(_selectionCapture, result);
        //    }
        //}


        private bool GetPositionByPoint(Point point, out StructuralCaretPoint caretPoint)
        {
            caretPoint = new StructuralCaretPoint();
            bool found = false;
            point = new Point(Math.Ceiling(point.X + 5), Math.Ceiling(point.Y));
            foreach (StructuralCaretPoint current in _layout.CaretPointsEnumerator(_drawingBounds))
            {
                if (point.Y > current.Y && point.Y < current.Y + current.Height && point.X > current.X)
                {
                    caretPoint = current;
                    found = true;
                }
            }
            return found;
        }

        public string GetText()
        {
            string result = string.Empty;
            for (int i = 0; i < _layout.ContainersCount; i++)
            {
                for (int t = 0; t < _layout[i].TextContainers.Count; t++)
                {
                    result += _layout[i].TextContainers[t].Chars;
                }
            }
            return result;
        }

        public string GetSelectedText()
        {
            StringBuilder sb = new StringBuilder();
            if (SelectionAny)
            {
                int selectionStart = SelectionStart;
                int selectionEnd = SelectionEnd;
                foreach (StructuralCaretPoint current in _layout.CaretPoints)
                {
                    if (current.GlobalCharOffset >= selectionEnd)
                    {
                        break;
                    }
                    if (current.GlobalCharOffset >= selectionStart)
                    {
                        sb.Append(current.Text?.Chars.Substring(current.CharOffset, current.Length));
                    }
                }
            }
            return sb.ToString();
        }

        public IEnumerable<(object source, int offset, int length)> GetSelectedObjects()
        {
            if (SelectionAny)
            {
                int selectionStart = SelectionStart;
                int selectionEnd = SelectionEnd;
                object source = null;
                int offset = 0;
                int length = 0;
                foreach (StructuralCaretPoint current in _layout.CaretPoints)
                {
                    if (current.GlobalCharOffset >= selectionEnd)
                    {
                        if (source != null)
                        {
                            yield return new ValueTuple<object, int, int>(source, offset, length);
                        }
                        break;
                    }
                    if (current.GlobalCharOffset >= selectionStart)
                    {
                        if (current.Text.SourceObject != null)
                        {
                            if (source == null)
                            {
                                source = current.Text.SourceObject;
                                offset = current.CharOffset;
                                length += current.Length;
                            }
                            else if (current.Text.SourceObject != source)
                            {
                                yield return new ValueTuple<object, int, int>(source, offset, length);
                                source = current.Text.SourceObject;
                                offset = current.CharOffset; 
                                length = current.Length;
                            }
                            else
                            {
                                length += current.Length;
                            }
                        }
                    }
                }
            }
        }
    }
}