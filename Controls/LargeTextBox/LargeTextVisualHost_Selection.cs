using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using OpenFontWPFControls.Layout;

namespace OpenFontWPFControls.Controls
{
    internal partial class LargeTextVisualHost
    {
        public bool SetCaretPosition(int offset)
        {
            return SetCaretPosition(new CaretPoint(CaretPointOwners.Anyone, offset));
        }

        public bool SetCaretPosition(CaretPoint point)
        {
            if (_layout != null)
            {
                point = _layout.CheckCaretPoint(point);
                if (!_caretPoint.Equals(point) || !_selectionCapture.Equals(point))
                {
                    _caretPoint = _selectionCapture = point;
                    UpdateControlLayer();
                    CaretIntoView();
                    _captureCaretXOffset = _caretPoint.X;
                    OnSelectionChangeCallBack?.Invoke();
                    return true;
                }
            }
            return false;
        }

        public bool ChangeCaretPositionByLine(int delta, bool changeCapture = true)
        {
            if (_layout?.GetLine(_caretPoint, delta)?.CaretPoints.LastOrDefault(p => p.X <= _captureCaretXOffset) is CaretPoint cp &&
                (!_caretPoint.Equals(cp) || !_selectionCapture.Equals(cp)))
            {
                if (changeCapture)
                {
                    _selectionCapture = cp;
                }
                _caretPoint = cp;
                UpdateControlLayer();
                CaretIntoView();
                OnSelectionChangeCallBack?.Invoke();
                return true;
            }
            return false;
        }

        public bool ChangeCaretPosition(int delta)
        {
            if (_layout?.GetPoint(_caretPoint, delta, out CaretPoint cp) == true && (!_caretPoint.Equals(cp) || !_selectionCapture.Equals(cp)))
            {
                _caretPoint = _selectionCapture = cp;
                UpdateControlLayer();
                CaretIntoView();
                _captureCaretXOffset = _caretPoint.X;
                OnSelectionChangeCallBack?.Invoke();
                return true;
            }
            return false;
        }

        public void SetCaretPosition(Point point)
        {
            point.X -= _viewXOffset;
            if (GetPositionByPoint(point, out CaretPoint cp))
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
            point.X -= _viewXOffset;
            if (GetPositionByPoint(point, out CaretPoint cp))
            {
                SetSelection(_selectionCapture, cp);
            }
        }

        public void SetSelection(CaretPoint capture, CaretPoint caret)
        {
            CaretPoint start = capture.CharOffset < caret.CharOffset ? capture : caret;
            CaretPoint end = capture.CharOffset > caret.CharOffset ? capture : caret;
            if ((SelectionStart != start.CharOffset || SelectionEnd != end.CharOffset) && _layout != null &&
                start.CharOffset >= 0 && start.CharOffset <= _layout.Text.Length && end.CharOffset >= 0 && end.CharOffset <= _layout.Text.Length)
            {
                _selectionCapture = capture;
                _caretPoint = caret.CharOffset > capture.CharOffset ? end : start;
                UpdateControlLayer();
                CaretIntoView();
                _captureCaretXOffset = _caretPoint.X;
                OnSelectionChangeCallBack?.Invoke();
            }
        }

        public void SetSelection(int start, int length)
        {
            if (_layout != null)
            {
                CaretPoint startPoint = _layout.CheckCaretPoint(new CaretPoint(CaretPointOwners.Anyone, start));
                CaretPoint endPoint = _layout.CheckCaretPoint(new CaretPoint(CaretPointOwners.Anyone, start + length));
                SetSelection(startPoint, endPoint);
            }
        }

        public void ChangeSelection(CaretPoint caret)
        {
            SetSelection(_selectionCapture, caret);
        }

        public void ChangeSelection(int delta)
        {
            if (_layout?.GetPoint(_caretPoint, delta, out CaretPoint result) == true)
            {
                SetSelection(_selectionCapture, result);
            }
        }


        private bool GetPositionByPoint(Point point, out CaretPoint caretPoint)
        {
            caretPoint = new CaretPoint();
            bool found = false;
            point = new Point(Math.Ceiling(point.X + 5), Math.Ceiling(point.Y));
            foreach ((LargeTextLine line, double y) in ViewLine())
            {
                foreach (CaretPoint current in line.CaretPoints)
                {
                    if (current.X < point.X && y < point.Y)
                    {
                        caretPoint = current;
                        found = true;
                    }
                    else if (current.X > point.X)
                    {
                        break;
                    }
                }
                if (y > point.Y)
                {
                    break;
                }
            }
            return found;
        }

        public bool GetCaretPoint(CaretPoint current, int delta, out CaretPoint result)
        {
            if (_layout == null)
            {
                result = new CaretPoint();
                return false;
            }
            return _layout.GetPoint(current, delta, out result);
        }
        
    }
}