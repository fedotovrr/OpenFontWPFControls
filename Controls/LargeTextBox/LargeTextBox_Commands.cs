using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using OpenFontWPFControls.Layout;

namespace OpenFontWPFControls.Controls
{
    partial class LargeTextBox
    {
        private static CommandBinding CreateCommandBinding(ICommand command)
        {
            return new CommandBinding(command, (s, e) => ((LargeTextBox)s).CommandExecuted(e), (s, e) => ((LargeTextBox)s).CanCommandExecuted(e));
        }

        private static void CommandRegister()
        {
            CommandManager.RegisterClassCommandBinding(typeof(LargeTextBox), CreateCommandBinding(Commands.ApplySuggestion));

            CommandManager.RegisterClassCommandBinding(typeof(LargeTextBox), CreateCommandBinding(EditingCommands.SelectToDocumentStart));
            CommandManager.RegisterClassCommandBinding(typeof(LargeTextBox), CreateCommandBinding(EditingCommands.SelectToDocumentEnd));
            CommandManager.RegisterClassCommandBinding(typeof(LargeTextBox), CreateCommandBinding(EditingCommands.SelectToLineStart));
            CommandManager.RegisterClassCommandBinding(typeof(LargeTextBox), CreateCommandBinding(EditingCommands.SelectToLineEnd));
            CommandManager.RegisterClassCommandBinding(typeof(LargeTextBox), CreateCommandBinding(EditingCommands.SelectLeftByWord));
            CommandManager.RegisterClassCommandBinding(typeof(LargeTextBox), CreateCommandBinding(EditingCommands.SelectRightByWord));
            CommandManager.RegisterClassCommandBinding(typeof(LargeTextBox), CreateCommandBinding(EditingCommands.SelectLeftByCharacter));
            CommandManager.RegisterClassCommandBinding(typeof(LargeTextBox), CreateCommandBinding(EditingCommands.SelectRightByCharacter));
            CommandManager.RegisterClassCommandBinding(typeof(LargeTextBox), CreateCommandBinding(EditingCommands.SelectUpByLine));
            CommandManager.RegisterClassCommandBinding(typeof(LargeTextBox), CreateCommandBinding(EditingCommands.SelectDownByLine));
            CommandManager.RegisterClassCommandBinding(typeof(LargeTextBox), CreateCommandBinding(EditingCommands.MoveToDocumentStart));
            CommandManager.RegisterClassCommandBinding(typeof(LargeTextBox), CreateCommandBinding(EditingCommands.MoveToDocumentEnd));
            CommandManager.RegisterClassCommandBinding(typeof(LargeTextBox), CreateCommandBinding(EditingCommands.MoveToLineStart));
            CommandManager.RegisterClassCommandBinding(typeof(LargeTextBox), CreateCommandBinding(EditingCommands.MoveToLineEnd));
            CommandManager.RegisterClassCommandBinding(typeof(LargeTextBox), CreateCommandBinding(EditingCommands.MoveLeftByWord));
            CommandManager.RegisterClassCommandBinding(typeof(LargeTextBox), CreateCommandBinding(EditingCommands.MoveRightByWord));
            CommandManager.RegisterClassCommandBinding(typeof(LargeTextBox), CreateCommandBinding(EditingCommands.MoveLeftByCharacter));
            CommandManager.RegisterClassCommandBinding(typeof(LargeTextBox), CreateCommandBinding(EditingCommands.MoveRightByCharacter));
            CommandManager.RegisterClassCommandBinding(typeof(LargeTextBox), CreateCommandBinding(EditingCommands.MoveUpByLine));
            CommandManager.RegisterClassCommandBinding(typeof(LargeTextBox), CreateCommandBinding(EditingCommands.MoveDownByLine));
            CommandManager.RegisterClassCommandBinding(typeof(LargeTextBox), CreateCommandBinding(EditingCommands.Delete));
            CommandManager.RegisterClassCommandBinding(typeof(LargeTextBox), CreateCommandBinding(EditingCommands.Backspace));
            CommandManager.RegisterClassCommandBinding(typeof(LargeTextBox), CreateCommandBinding(EditingCommands.DeletePreviousWord));
            CommandManager.RegisterClassCommandBinding(typeof(LargeTextBox), CreateCommandBinding(EditingCommands.DeleteNextWord));

            CommandManager.RegisterClassCommandBinding(typeof(LargeTextBox), CreateCommandBinding(ApplicationCommands.SelectAll));
            CommandManager.RegisterClassCommandBinding(typeof(LargeTextBox), CreateCommandBinding(ApplicationCommands.Copy));
            CommandManager.RegisterClassCommandBinding(typeof(LargeTextBox), CreateCommandBinding(ApplicationCommands.Paste));
            CommandManager.RegisterClassCommandBinding(typeof(LargeTextBox), CreateCommandBinding(ApplicationCommands.Cut));
            CommandManager.RegisterClassCommandBinding(typeof(LargeTextBox), CreateCommandBinding(ApplicationCommands.Undo));
            CommandManager.RegisterClassCommandBinding(typeof(LargeTextBox), CreateCommandBinding(ApplicationCommands.Redo));

            CommandManager.RegisterClassInputBinding(typeof(LargeTextBox), new InputBinding(EditingCommands.SelectToDocumentStart, new KeyGesture(Key.Home, ModifierKeys.Shift | ModifierKeys.Control)));
            CommandManager.RegisterClassInputBinding(typeof(LargeTextBox), new InputBinding(EditingCommands.SelectToDocumentEnd, new KeyGesture(Key.End, ModifierKeys.Shift | ModifierKeys.Control)));
            CommandManager.RegisterClassInputBinding(typeof(LargeTextBox), new InputBinding(EditingCommands.SelectToLineStart, new KeyGesture(Key.Home, ModifierKeys.Shift)));
            CommandManager.RegisterClassInputBinding(typeof(LargeTextBox), new InputBinding(EditingCommands.SelectToLineEnd, new KeyGesture(Key.End, ModifierKeys.Shift)));
            CommandManager.RegisterClassInputBinding(typeof(LargeTextBox), new InputBinding(EditingCommands.SelectLeftByWord, new KeyGesture(Key.Left, ModifierKeys.Shift | ModifierKeys.Control)));
            CommandManager.RegisterClassInputBinding(typeof(LargeTextBox), new InputBinding(EditingCommands.SelectRightByWord, new KeyGesture(Key.Right, ModifierKeys.Shift | ModifierKeys.Control)));
            CommandManager.RegisterClassInputBinding(typeof(LargeTextBox), new InputBinding(EditingCommands.SelectLeftByCharacter, new KeyGesture(Key.Left, ModifierKeys.Shift)));
            CommandManager.RegisterClassInputBinding(typeof(LargeTextBox), new InputBinding(EditingCommands.SelectRightByCharacter, new KeyGesture(Key.Right, ModifierKeys.Shift)));
            CommandManager.RegisterClassInputBinding(typeof(LargeTextBox), new InputBinding(EditingCommands.SelectUpByLine, new KeyGesture(Key.Up, ModifierKeys.Shift)));
            CommandManager.RegisterClassInputBinding(typeof(LargeTextBox), new InputBinding(EditingCommands.SelectDownByLine, new KeyGesture(Key.Down, ModifierKeys.Shift)));
            CommandManager.RegisterClassInputBinding(typeof(LargeTextBox), new InputBinding(EditingCommands.MoveToDocumentStart, new KeyGesture(Key.Home, ModifierKeys.Control)));
            CommandManager.RegisterClassInputBinding(typeof(LargeTextBox), new InputBinding(EditingCommands.MoveToDocumentEnd, new KeyGesture(Key.End, ModifierKeys.Control)));
            CommandManager.RegisterClassInputBinding(typeof(LargeTextBox), new InputBinding(EditingCommands.MoveToLineStart, new KeyGesture(Key.Home)));
            CommandManager.RegisterClassInputBinding(typeof(LargeTextBox), new InputBinding(EditingCommands.MoveToLineEnd, new KeyGesture(Key.End)));
            CommandManager.RegisterClassInputBinding(typeof(LargeTextBox), new InputBinding(EditingCommands.MoveLeftByWord, new KeyGesture(Key.Left, ModifierKeys.Control)));
            CommandManager.RegisterClassInputBinding(typeof(LargeTextBox), new InputBinding(EditingCommands.MoveRightByWord, new KeyGesture(Key.Right, ModifierKeys.Control)));
            CommandManager.RegisterClassInputBinding(typeof(LargeTextBox), new InputBinding(EditingCommands.MoveLeftByCharacter, new KeyGesture(Key.Left)));
            CommandManager.RegisterClassInputBinding(typeof(LargeTextBox), new InputBinding(EditingCommands.MoveRightByCharacter, new KeyGesture(Key.Right)));
            CommandManager.RegisterClassInputBinding(typeof(LargeTextBox), new InputBinding(EditingCommands.MoveUpByLine, new KeyGesture(Key.Up)));
            CommandManager.RegisterClassInputBinding(typeof(LargeTextBox), new InputBinding(EditingCommands.MoveDownByLine, new KeyGesture(Key.Down)));
            CommandManager.RegisterClassInputBinding(typeof(LargeTextBox), new InputBinding(EditingCommands.Delete, new KeyGesture(Key.Delete)));
            CommandManager.RegisterClassInputBinding(typeof(LargeTextBox), new InputBinding(EditingCommands.Backspace, new KeyGesture(Key.Back)));
            CommandManager.RegisterClassInputBinding(typeof(LargeTextBox), new InputBinding(EditingCommands.DeletePreviousWord, new KeyGesture(Key.Back, ModifierKeys.Control)));
            CommandManager.RegisterClassInputBinding(typeof(LargeTextBox), new InputBinding(EditingCommands.DeleteNextWord, new KeyGesture(Key.Delete, ModifierKeys.Control)));
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
                        if (_visualHost.Layout.GetLine(_visualHost.CaretPoint) is LargeTextLine line)
                        {
                            _visualHost.ChangeSelection(line.CaretPoints.First());
                        }
                    }
                    break;
                case RoutedUICommand when e.Command == EditingCommands.SelectToLineEnd:
                    {
                        if (_visualHost.Layout.GetLine(_visualHost.CaretPoint) is LargeTextLine line)
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
                        if (_visualHost.Layout.GetLine(_visualHost.CaretPoint) is LargeTextLine line)
                        {
                            _visualHost.SetCaretPosition(line.CaretPoints.First());
                        }
                    }
                    break;
                case RoutedUICommand when e.Command == EditingCommands.MoveToLineEnd:
                    {
                        if (_visualHost.Layout.GetLine(_visualHost.CaretPoint) is LargeTextLine line)
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
                    ReplaceSelected(DisableInputLineBreaks ? StaticHelper.LineBreaksRemover(StaticHelper.ClipboardGetText()) : StaticHelper.ClipboardGetText());
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
            return StaticHelper.GetPreviousWordOffset(Text, _visualHost.CaretPoint.CharOffset - 1);
        }

        private int GetNextWordOffset()
        {
            return StaticHelper.GetNextWordOffset(Text, _visualHost.CaretPoint.CharOffset);
        }

        private void SelectWord()
        {
            (int offset, int length) = StaticHelper.GetWordByOffset(Text, _visualHost.CaretCharOffset);
            _visualHost.SetSelection(offset, length);
        }
    }
}
