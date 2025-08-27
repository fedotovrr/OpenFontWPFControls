using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using OpenFontWPFControls.Layout;

namespace OpenFontWPFControls.Controls
{
    partial class LargeTextBox
    {
        private void Remove(int delta = 0)
        {
            if (delta > 0)
            {
                CaretPoint start = _visualHost.CaretPoint;
                if (!ReplaceSelected() && start.CharOffset < Text.Length && _visualHost.GetCaretPoint(start, delta, out CaretPoint end))
                {
                    TextReplacer(start.CharOffset, end.CharOffset - start.CharOffset);
                }
            }
            else if (delta < 0)
            {
                CaretPoint end = _visualHost.CaretPoint;
                if (!ReplaceSelected() && end.CharOffset > 0 && _visualHost.ChangeCaretPosition(delta))
                {
                    CaretPoint start = _visualHost.CaretPoint;
                    TextReplacer(start.CharOffset, end.CharOffset - start.CharOffset);
                }
            }
            else
            {
                ReplaceSelected();
            }
        }

        private bool ReplaceSelected(string text = null)
        {
            text ??= String.Empty;
            text = text == "\r" ? "\n" : text;
            bool change = !string.IsNullOrEmpty(text);
            int start;
            int end;
            if (_visualHost.SelectionLength > 0)
            {
                start = _visualHost.SelectionStart;
                end = _visualHost.SelectionEnd;
                change = true;
            }
            else
            {
                start = end = _visualHost.CaretCharOffset;
            }
            if (change)
            {
                TextReplacer(start, end - start, text);
            }
            return change;
        }

        private void TextReplacer(int offset, int length, string value = "")
        {
            if (length == 0 && string.IsNullOrEmpty(value))
            {
                return;
            }
            
            TextChangedEventArgs eventArgs;
            if (_lastChangeOffset + 1 == offset && _lastChangeLength == 1 && value.Length == 1 && length == 0 && value != "\n")
            {
                _lastUndoUnit.NewValue += value;
                eventArgs = new TextChangedEventArgs(TextChangedEvent, UndoAction.Merge);
            }
            else
            {
                _undoBuffer.Add(_lastUndoUnit = new TextUndoUnit(offset, Text.Substring(offset, length), value));
                eventArgs = new TextChangedEventArgs(TextChangedEvent, UndoAction.Create);
            }

            DoWithoutTextChanges(() => Text = StaticHelper.StringReplace(Text, value, offset, length));
            _visualHost.SetCaretPosition(new CaretPoint(CaretPointOwners.Anyone, offset + value.Length));
            _lastChangeLength = value.Length;
            _lastChangeOffset = offset;
            RaiseEvent(eventArgs);
        }

        private void BaseUndoRedo(UndoAction action)
        {
            TextUndoBuffer.BaseMover mover = 
                action switch
                {
                    UndoAction.Undo => _undoBuffer.DoUndo,
                    UndoAction.Redo => _undoBuffer.DoRedo,
                    _ => throw new ArgumentException()
                };
            string text = Text;
            mover(ref text, out int offset, out int length);
            DoWithoutTextChanges(() => Text = text);
            _visualHost.SetCaretPosition(new CaretPoint(CaretPointOwners.Anyone, offset + length));
            RaiseEvent(new TextChangedEventArgs(TextChangedEvent, action));
        }

        private void DoWithoutTextChanges(Action action)
        {
            _skipTextChanged = true;
            action();
            _skipTextChanged = false;
        }

        private void EditorOnTextChanged()
        {
            if (!_skipTextChanged)
            {
                _undoBuffer.Clear();
                RaiseEvent(new TextChangedEventArgs(TextChangedEvent, UndoAction.Clear));
            }
        }

        protected override void OnTextInput(TextCompositionEventArgs e)
        {
            if (!IsReadOnly)
            {
                e.Handled = ReplaceSelected(DisableInputLineBreaks ? StaticHelper.LineBreaksRemover(e.Text) : e.Text);
            }
        }


    }

}
