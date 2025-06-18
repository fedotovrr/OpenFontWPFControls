using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using OpenFontWPFControls.Layout;

namespace OpenFontWPFControls.Controls
{
    partial class TextBox
    {
        private static CommandBinding CreateCommandBinding(ICommand command)
        {
            return new CommandBinding(command, (s, e) => ((TextBox)s).CommandExecuted(e), (s, e) => ((TextBox)s).CanCommandExecuted(e));
        }

        private static void CommandRegister()
        {
            CommandManager.RegisterClassCommandBinding(typeof(TextBox), CreateCommandBinding(Commands.ApplySuggestion));

            CommandManager.RegisterClassCommandBinding(typeof(TextBox), CreateCommandBinding(EditingCommands.SelectToDocumentStart));
            CommandManager.RegisterClassCommandBinding(typeof(TextBox), CreateCommandBinding(EditingCommands.SelectToDocumentEnd));
            CommandManager.RegisterClassCommandBinding(typeof(TextBox), CreateCommandBinding(EditingCommands.SelectToLineStart));
            CommandManager.RegisterClassCommandBinding(typeof(TextBox), CreateCommandBinding(EditingCommands.SelectToLineEnd));
            CommandManager.RegisterClassCommandBinding(typeof(TextBox), CreateCommandBinding(EditingCommands.SelectLeftByWord));
            CommandManager.RegisterClassCommandBinding(typeof(TextBox), CreateCommandBinding(EditingCommands.SelectRightByWord));
            CommandManager.RegisterClassCommandBinding(typeof(TextBox), CreateCommandBinding(EditingCommands.SelectLeftByCharacter));
            CommandManager.RegisterClassCommandBinding(typeof(TextBox), CreateCommandBinding(EditingCommands.SelectRightByCharacter));
            CommandManager.RegisterClassCommandBinding(typeof(TextBox), CreateCommandBinding(EditingCommands.SelectUpByLine));
            CommandManager.RegisterClassCommandBinding(typeof(TextBox), CreateCommandBinding(EditingCommands.SelectDownByLine));
            CommandManager.RegisterClassCommandBinding(typeof(TextBox), CreateCommandBinding(EditingCommands.MoveToDocumentStart));
            CommandManager.RegisterClassCommandBinding(typeof(TextBox), CreateCommandBinding(EditingCommands.MoveToDocumentEnd));
            CommandManager.RegisterClassCommandBinding(typeof(TextBox), CreateCommandBinding(EditingCommands.MoveToLineStart));
            CommandManager.RegisterClassCommandBinding(typeof(TextBox), CreateCommandBinding(EditingCommands.MoveToLineEnd));
            CommandManager.RegisterClassCommandBinding(typeof(TextBox), CreateCommandBinding(EditingCommands.MoveLeftByWord));
            CommandManager.RegisterClassCommandBinding(typeof(TextBox), CreateCommandBinding(EditingCommands.MoveRightByWord));
            CommandManager.RegisterClassCommandBinding(typeof(TextBox), CreateCommandBinding(EditingCommands.MoveLeftByCharacter));
            CommandManager.RegisterClassCommandBinding(typeof(TextBox), CreateCommandBinding(EditingCommands.MoveRightByCharacter));
            CommandManager.RegisterClassCommandBinding(typeof(TextBox), CreateCommandBinding(EditingCommands.MoveUpByLine));
            CommandManager.RegisterClassCommandBinding(typeof(TextBox), CreateCommandBinding(EditingCommands.MoveDownByLine));
            CommandManager.RegisterClassCommandBinding(typeof(TextBox), CreateCommandBinding(EditingCommands.Delete));
            CommandManager.RegisterClassCommandBinding(typeof(TextBox), CreateCommandBinding(EditingCommands.Backspace));
            CommandManager.RegisterClassCommandBinding(typeof(TextBox), CreateCommandBinding(EditingCommands.DeletePreviousWord));
            CommandManager.RegisterClassCommandBinding(typeof(TextBox), CreateCommandBinding(EditingCommands.DeleteNextWord));

            CommandManager.RegisterClassCommandBinding(typeof(TextBox), CreateCommandBinding(ApplicationCommands.SelectAll));
            CommandManager.RegisterClassCommandBinding(typeof(TextBox), CreateCommandBinding(ApplicationCommands.Copy));
            CommandManager.RegisterClassCommandBinding(typeof(TextBox), CreateCommandBinding(ApplicationCommands.Paste));
            CommandManager.RegisterClassCommandBinding(typeof(TextBox), CreateCommandBinding(ApplicationCommands.Cut));
            CommandManager.RegisterClassCommandBinding(typeof(TextBox), CreateCommandBinding(ApplicationCommands.Undo));
            CommandManager.RegisterClassCommandBinding(typeof(TextBox), CreateCommandBinding(ApplicationCommands.Redo));

            CommandManager.RegisterClassInputBinding(typeof(TextBox), new InputBinding(EditingCommands.SelectToDocumentStart, new KeyGesture(Key.Home, ModifierKeys.Shift | ModifierKeys.Control)));
            CommandManager.RegisterClassInputBinding(typeof(TextBox), new InputBinding(EditingCommands.SelectToDocumentEnd, new KeyGesture(Key.End, ModifierKeys.Shift | ModifierKeys.Control)));
            CommandManager.RegisterClassInputBinding(typeof(TextBox), new InputBinding(EditingCommands.SelectToLineStart, new KeyGesture(Key.Home, ModifierKeys.Shift)));
            CommandManager.RegisterClassInputBinding(typeof(TextBox), new InputBinding(EditingCommands.SelectToLineEnd, new KeyGesture(Key.End, ModifierKeys.Shift)));
            CommandManager.RegisterClassInputBinding(typeof(TextBox), new InputBinding(EditingCommands.SelectLeftByWord, new KeyGesture(Key.Left, ModifierKeys.Shift | ModifierKeys.Control)));
            CommandManager.RegisterClassInputBinding(typeof(TextBox), new InputBinding(EditingCommands.SelectRightByWord, new KeyGesture(Key.Right, ModifierKeys.Shift | ModifierKeys.Control)));
            CommandManager.RegisterClassInputBinding(typeof(TextBox), new InputBinding(EditingCommands.SelectLeftByCharacter, new KeyGesture(Key.Left, ModifierKeys.Shift)));
            CommandManager.RegisterClassInputBinding(typeof(TextBox), new InputBinding(EditingCommands.SelectRightByCharacter, new KeyGesture(Key.Right, ModifierKeys.Shift)));
            CommandManager.RegisterClassInputBinding(typeof(TextBox), new InputBinding(EditingCommands.SelectUpByLine, new KeyGesture(Key.Up, ModifierKeys.Shift)));
            CommandManager.RegisterClassInputBinding(typeof(TextBox), new InputBinding(EditingCommands.SelectDownByLine, new KeyGesture(Key.Down, ModifierKeys.Shift)));
            CommandManager.RegisterClassInputBinding(typeof(TextBox), new InputBinding(EditingCommands.MoveToDocumentStart, new KeyGesture(Key.Home, ModifierKeys.Control)));
            CommandManager.RegisterClassInputBinding(typeof(TextBox), new InputBinding(EditingCommands.MoveToDocumentEnd, new KeyGesture(Key.End, ModifierKeys.Control)));
            CommandManager.RegisterClassInputBinding(typeof(TextBox), new InputBinding(EditingCommands.MoveToLineStart, new KeyGesture(Key.Home)));
            CommandManager.RegisterClassInputBinding(typeof(TextBox), new InputBinding(EditingCommands.MoveToLineEnd, new KeyGesture(Key.End)));
            CommandManager.RegisterClassInputBinding(typeof(TextBox), new InputBinding(EditingCommands.MoveLeftByWord, new KeyGesture(Key.Left, ModifierKeys.Control)));
            CommandManager.RegisterClassInputBinding(typeof(TextBox), new InputBinding(EditingCommands.MoveRightByWord, new KeyGesture(Key.Right, ModifierKeys.Control)));
            CommandManager.RegisterClassInputBinding(typeof(TextBox), new InputBinding(EditingCommands.MoveLeftByCharacter, new KeyGesture(Key.Left)));
            CommandManager.RegisterClassInputBinding(typeof(TextBox), new InputBinding(EditingCommands.MoveRightByCharacter, new KeyGesture(Key.Right)));
            CommandManager.RegisterClassInputBinding(typeof(TextBox), new InputBinding(EditingCommands.MoveUpByLine, new KeyGesture(Key.Up)));
            CommandManager.RegisterClassInputBinding(typeof(TextBox), new InputBinding(EditingCommands.MoveDownByLine, new KeyGesture(Key.Down)));
            CommandManager.RegisterClassInputBinding(typeof(TextBox), new InputBinding(EditingCommands.Delete, new KeyGesture(Key.Delete)));
            CommandManager.RegisterClassInputBinding(typeof(TextBox), new InputBinding(EditingCommands.Backspace, new KeyGesture(Key.Back)));
            CommandManager.RegisterClassInputBinding(typeof(TextBox), new InputBinding(EditingCommands.DeletePreviousWord, new KeyGesture(Key.Back, ModifierKeys.Control)));
            CommandManager.RegisterClassInputBinding(typeof(TextBox), new InputBinding(EditingCommands.DeleteNextWord, new KeyGesture(Key.Delete, ModifierKeys.Control)));
        }

        private void CanCommandExecuted(CanExecuteRoutedEventArgs e)
        {
            switch (e.Command)
            {
                case RoutedUICommand when !IsReadOnly && e.Command == Commands.ApplySuggestion && e.Parameter is SuggestionPoint:
                    e.CanExecute = true;
                    break;
                case RoutedUICommand when e.Command == EditingCommands.SelectToDocumentStart:
                case RoutedUICommand when e.Command == EditingCommands.SelectToDocumentEnd:
                case RoutedUICommand when e.Command == EditingCommands.SelectToLineStart:
                case RoutedUICommand when e.Command == EditingCommands.SelectToLineEnd:
                case RoutedUICommand when e.Command == EditingCommands.SelectLeftByWord:
                case RoutedUICommand when e.Command == EditingCommands.SelectRightByWord:
                case RoutedUICommand when e.Command == EditingCommands.SelectLeftByCharacter:
                case RoutedUICommand when e.Command == EditingCommands.SelectRightByCharacter:
                case RoutedUICommand when e.Command == EditingCommands.SelectUpByLine:
                case RoutedUICommand when e.Command == EditingCommands.SelectDownByLine:
                case RoutedUICommand when e.Command == EditingCommands.MoveToDocumentStart:
                case RoutedUICommand when e.Command == EditingCommands.MoveToDocumentEnd:
                case RoutedUICommand when e.Command == EditingCommands.MoveToLineStart:
                case RoutedUICommand when e.Command == EditingCommands.MoveToLineEnd:
                case RoutedUICommand when e.Command == EditingCommands.MoveLeftByWord:
                case RoutedUICommand when e.Command == EditingCommands.MoveRightByWord:
                case RoutedUICommand when e.Command == EditingCommands.MoveLeftByCharacter:
                case RoutedUICommand when e.Command == EditingCommands.MoveRightByCharacter:
                case RoutedUICommand when e.Command == EditingCommands.MoveUpByLine:
                case RoutedUICommand when e.Command == EditingCommands.MoveDownByLine:
                case RoutedUICommand when e.Command == ApplicationCommands.SelectAll:
                    e.CanExecute = true;
                    break;
                case RoutedUICommand when !IsReadOnly && e.Command == EditingCommands.Delete:
                case RoutedUICommand when !IsReadOnly && e.Command == EditingCommands.Backspace:
                case RoutedUICommand when !IsReadOnly && e.Command == EditingCommands.DeletePreviousWord:
                case RoutedUICommand when !IsReadOnly && e.Command == EditingCommands.DeleteNextWord:
                    e.CanExecute = true;
                    break;
                case RoutedUICommand when e.Command == ApplicationCommands.Copy:
                    e.CanExecute = _visualHost.SelectionAny;
                    break;
                case RoutedUICommand when !IsReadOnly && e.Command == ApplicationCommands.Paste:
                    e.CanExecute = Clipboard.ContainsData(DataFormats.UnicodeText);
                    break;
                case RoutedUICommand when !IsReadOnly && e.Command == ApplicationCommands.Cut:
                    e.CanExecute = _visualHost.SelectionAny;
                    break;
                case RoutedUICommand when !IsReadOnly && e.Command == ApplicationCommands.Undo:
                    e.CanExecute = _undoBuffer.CanUndo;
                    break;
                case RoutedUICommand when !IsReadOnly && e.Command == ApplicationCommands.Redo:
                    e.CanExecute = _undoBuffer.CanRedo;
                    break;
                default:
                    e.CanExecute = false;
                    break;
            }
        }

        private void CommandExecuted(ExecutedRoutedEventArgs e)
        {
            switch (e.Command)
            {
                case RoutedUICommand when !IsReadOnly && e.Command == Commands.ApplySuggestion && e.Parameter is SuggestionPoint suggestionPoint:
                    TextReplacer(suggestionPoint.Offset, suggestionPoint.Length, suggestionPoint.Suggestion);
                    break;
                case RoutedUICommand when e.Command == EditingCommands.SelectToDocumentStart:
                    _visualHost.ChangeSelection(_visualHost.Layout.FirstCaretPoint);
                    break;
                case RoutedUICommand when e.Command == EditingCommands.SelectToDocumentEnd:
                    _visualHost.ChangeSelection(_visualHost.Layout.LastCaretPoint);
                    break;
                case RoutedUICommand when e.Command == EditingCommands.SelectToLineStart:
                    {
                        if (_visualHost.Layout.GetLine(_visualHost.CaretPoint) is TextLine line)
                        {
                            _visualHost.ChangeSelection(line.CaretPoints.First());
                        }
                    }
                    break;
                case RoutedUICommand when e.Command == EditingCommands.SelectToLineEnd:
                    {
                        if (_visualHost.Layout.GetLine(_visualHost.CaretPoint) is TextLine line)
                        {
                            _visualHost.ChangeSelection(line.ReverseCaretPoints.First());
                        }
                    }
                    break;
                case RoutedUICommand when e.Command == EditingCommands.SelectLeftByWord:
                    _visualHost.ChangeSelection(new CaretPoint(CaretPointOwners.Anyone, GetPreviousWordOffset()));
                    break;
                case RoutedUICommand when e.Command == EditingCommands.SelectRightByWord:
                    _visualHost.ChangeSelection(new CaretPoint(CaretPointOwners.Anyone, GetNextWordOffset()));
                    break;
                case RoutedUICommand when e.Command == EditingCommands.SelectLeftByCharacter:
                    _visualHost.ChangeSelection(-1);
                    break;
                case RoutedUICommand when e.Command == EditingCommands.SelectRightByCharacter:
                    _visualHost.ChangeSelection(1);
                    break;
                case RoutedUICommand when e.Command == EditingCommands.SelectUpByLine:
                    _visualHost.ChangeCaretPositionByLine(-1, false);
                    break;
                case RoutedUICommand when e.Command == EditingCommands.SelectDownByLine:
                    _visualHost.ChangeCaretPositionByLine(1, false);
                    break;

                case RoutedUICommand when e.Command == EditingCommands.MoveToDocumentStart:
                    _visualHost.SetCaretPosition(_visualHost.Layout.FirstCaretPoint);
                    break;
                case RoutedUICommand when e.Command == EditingCommands.MoveToDocumentEnd:
                    _visualHost.SetCaretPosition(_visualHost.Layout.LastCaretPoint);
                    break;
                case RoutedUICommand when e.Command == EditingCommands.MoveToLineStart:
                    {
                        if (_visualHost.Layout.GetLine(_visualHost.CaretPoint) is TextLine line)
                        {
                            _visualHost.SetCaretPosition(line.CaretPoints.First());
                        }
                    }
                    break;
                case RoutedUICommand when e.Command == EditingCommands.MoveToLineEnd:
                    {
                        if (_visualHost.Layout.GetLine(_visualHost.CaretPoint) is TextLine line)
                        {
                            _visualHost.SetCaretPosition(line.ReverseCaretPoints.First());
                        }
                    }
                    break;
                case RoutedUICommand when e.Command == EditingCommands.MoveLeftByWord:
                    _visualHost.SetCaretPosition(GetPreviousWordOffset());
                    break;
                case RoutedUICommand when e.Command == EditingCommands.MoveRightByWord:
                    _visualHost.SetCaretPosition(GetNextWordOffset());
                    break;
                case RoutedUICommand when e.Command == EditingCommands.MoveLeftByCharacter:
                    _visualHost.ChangeCaretPosition(-1);
                    break;
                case RoutedUICommand when e.Command == EditingCommands.MoveRightByCharacter:
                    _visualHost.ChangeCaretPosition(1);
                    break;
                case RoutedUICommand when e.Command == EditingCommands.MoveUpByLine:
                    _visualHost.ChangeCaretPositionByLine(-1);
                    break;
                case RoutedUICommand when e.Command == EditingCommands.MoveDownByLine:
                    _visualHost.ChangeCaretPositionByLine(1);
                    break;
                case RoutedUICommand when e.Command == ApplicationCommands.SelectAll:
                    _visualHost.SetSelection(_visualHost.Layout.FirstCaretPoint, _visualHost.Layout.LastCaretPoint);
                    break;

                case RoutedUICommand when !IsReadOnly && e.Command == EditingCommands.Delete:
                    Remove(1);
                    break;
                case RoutedUICommand when !IsReadOnly && e.Command == EditingCommands.Backspace:
                    Remove(-1);
                    break;
                case RoutedUICommand when !IsReadOnly && e.Command == EditingCommands.DeletePreviousWord:
                    {
                        int start = GetPreviousWordOffset();
                        TextReplacer(start, _visualHost.CaretCharOffset - start);
                    }
                    break;
                case RoutedUICommand when !IsReadOnly && e.Command == EditingCommands.DeleteNextWord:
                    {
                        int start = _visualHost.CaretCharOffset;
                        TextReplacer(start, GetNextWordOffset() - start);
                    }
                    break;

                case RoutedUICommand when e.Command == ApplicationCommands.Copy:
                    Clipboard.SetData(DataFormats.UnicodeText, Text.Substring(_visualHost.SelectionStart, _visualHost.SelectionLength));
                    break;
                case RoutedUICommand when !IsReadOnly && e.Command == ApplicationCommands.Paste:
                    ReplaceSelected(ClipboardGetText());
                    break;
                case RoutedUICommand when !IsReadOnly && e.Command == ApplicationCommands.Cut:
                    Clipboard.SetData(DataFormats.UnicodeText, Text.Substring(_visualHost.SelectionStart, _visualHost.SelectionLength));
                    ReplaceSelected();
                    break;
                case RoutedUICommand when !IsReadOnly && e.Command == ApplicationCommands.Undo:
                    BaseUndoRedo(UndoAction.Undo);
                    break;
                case RoutedUICommand when !IsReadOnly && e.Command == ApplicationCommands.Redo:
                    BaseUndoRedo(UndoAction.Redo);
                    break;
            }
        }

        private int GetPreviousWordOffset()
        {
            int offset = _visualHost.CaretPoint.CharOffset - 1;
            if (offset < Text.Length && offset >= 0)
            {
                int param = CharType(Text[offset]);
                while (true)
                {
                    offset--;
                    if (offset < 0)
                        break;
                    if (param != CharType(Text[offset]))
                        break;
                }
            }
            return offset   + 1;
        }

        private int GetNextWordOffset()
        {
            int offset = _visualHost.CaretPoint.CharOffset;
            if (offset < Text.Length && offset >= 0)
            {
                int param = CharType(Text[offset]);
                while (true)
                {
                    offset++;
                    if (offset >= Text.Length)
                        break;
                    if (param != CharType(Text[offset]))
                        break;
                }
            }
            return offset;
        }

        private void SelectWord()
        {
            (int offset, int length) = GetWordByOffset(Text, _visualHost.CaretCharOffset);
            _visualHost.SetSelection(offset, length);
        }

        internal static (int offset, int length) GetWordByOffset(string text, int offset)
        {
            int length = text.Length;
            offset = offset < 0 ? 0 : offset < length ? offset : length - 1;
            int current = CharType(text[offset]);
            int left = offset;
            int right = offset;

            while (true)
            {
                right++;
                if (right >= length)
                    break;
                if (current != CharType(text[right]))
                {
                    break;
                }
            }

            while (true)
            {
                left--;
                if (left < 0)
                    break;
                if (current != CharType(text[left]))
                {
                    left++;
                    break;
                }
            }

            return new ValueTuple<int, int>(left, right - left);
        }

        private static int CharType(char c)
        {
            return char.IsSeparator(c) ? 1 : char.IsPunctuation(c) ? 2 : char.IsDigit(c) ? 3 : char.IsLetter(c) ? 4 : 0;
        }

        internal static string ClipboardGetText()
        {
            try
            {
                return Clipboard.GetData(DataFormats.UnicodeText) as string;
            }
            catch
            {
                return null;
            }
        }
    }
}
