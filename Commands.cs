using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Input;

namespace OpenFontWPFControls
{
    public static class Commands
    {
        private static readonly SemaphoreSlim Sync = new SemaphoreSlim(1, 1);
        private static RoutedUICommand _applySuggestion;

        public static RoutedUICommand ApplySuggestion => EnsureCommand(ref _applySuggestion);


        private static RoutedUICommand EnsureCommand(ref RoutedUICommand command, [CallerMemberName] string commandPropertyName = null)
        {
            Sync.Wait();
            try
            {
                command ??= commandPropertyName == null ? null : new RoutedUICommand(commandPropertyName, commandPropertyName, typeof(Commands));
            }
            finally
            {
                Sync.Release();
            }
            return command;
        }
    }
}
