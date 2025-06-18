using System.Linq;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;

namespace OpenFontWPFControls.Controls
{
    partial class TextBox
    {
        protected override void OnContextMenuOpening(ContextMenuEventArgs e)
        {
            ContextMenu cm = ContextMenu = new ContextMenu();

            IEnumerable<SuggestionPoint> suggestions = SpellerGetSuggestions(_visualHost.CaretCharOffset);
            if (suggestions.Any())
            {
                foreach (SuggestionPoint point in suggestions)
                {
                    CreateItem(cm, Commands.ApplySuggestion, point, point.Suggestion);
                }

                cm.Items.Add(new Separator());
            }

            CreateItem(cm, ApplicationCommands.Cut);
            CreateItem(cm, ApplicationCommands.Copy);
            CreateItem(cm, ApplicationCommands.Paste);

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