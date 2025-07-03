using System.Windows;

namespace OpenFontWPFControls.Controls;

public class HitActionEventArgs : RoutedEventArgs
{
    public readonly object HitObject;

    public HitActionEventArgs(RoutedEvent routedEvent, object source, object hitObject) : base(routedEvent, source)
    {
        HitObject = hitObject;
    }
}