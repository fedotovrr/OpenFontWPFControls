using System.Windows.Controls;
using System.Windows.Input;

namespace OpenFontWPFControls.Controls
{
    partial class StructuralTextViewer
    {
        protected override void OnContextMenuOpening(ContextMenuEventArgs e)
        {
            ContextMenu cm = ContextMenu = new ContextMenu();

            CreateItem(cm, ApplicationCommands.Copy);

            _visualHost.SetControlLayerVisibility(true);
            _skipFocusChanged = true;
        }

        private static void CreateItem(ContextMenu cm, RoutedUICommand c, object parameter = null, string header = null)
        {
            cm.Items.Add(new MenuItem { Header = header ?? c.Text, Command = c, CommandParameter = parameter });
        }

        protected override void OnContextMenuClosing(ContextMenuEventArgs e)
        {
            _skipFocusChanged = false;
        }
    }
}