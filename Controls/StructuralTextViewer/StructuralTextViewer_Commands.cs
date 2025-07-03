using System.Windows;
using System.Windows.Input;

namespace OpenFontWPFControls.Controls
{
    partial class StructuralTextViewer
    {
        private static CommandBinding CreateCommandBinding(ICommand command)
        {
            return new CommandBinding(command, (s, e) => ((StructuralTextViewer)s).CommandExecuted(e), (s, e) => ((StructuralTextViewer)s).CanCommandExecuted(e));
        }

        private static void CommandRegister()
        {
            CommandManager.RegisterClassCommandBinding(typeof(StructuralTextViewer), CreateCommandBinding(ApplicationCommands.SelectAll));
            CommandManager.RegisterClassCommandBinding(typeof(StructuralTextViewer), CreateCommandBinding(ApplicationCommands.Copy));
        }

        private void CanCommandExecuted(CanExecuteRoutedEventArgs e)
        {
            switch (e.Command)
            {
                case RoutedUICommand when e.Command == ApplicationCommands.SelectAll:
                    e.CanExecute = true;
                    break;
                case RoutedUICommand when e.Command == ApplicationCommands.Copy:
                    e.CanExecute = _visualHost.SelectionAny;
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
                case RoutedUICommand when e.Command == ApplicationCommands.SelectAll:
                    _visualHost.SetSelection(_visualHost.Layout.FirstCaretPoint, _visualHost.Layout.LastCaretPoint);
                    break;

                case RoutedUICommand when e.Command == ApplicationCommands.Copy:
                    Clipboard.SetData(DataFormats.UnicodeText, _visualHost.GetSelectedText());
                    break;
            }
        }
    }
}
