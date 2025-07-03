using System;
using System.Windows;
using System.Windows.Controls;
using OpenFontWPFControls.Layout;

namespace OpenFontWPFControls.Controls
{
    partial class LargeTextBox
    {
        public static readonly RoutedEvent TextChangedEvent = EventManager.RegisterRoutedEvent(
            nameof(TextChanged),
            RoutingStrategy.Bubble,
            typeof(TextChangedEventHandler),
            typeof(LargeTextBox));

        public event TextChangedEventHandler TextChanged
        {
            add => AddHandler(TextChangedEvent, value);
            remove => RemoveHandler(TextChangedEvent, value);
        }

        public static readonly RoutedEvent SelectionChangedEvent = EventManager.RegisterRoutedEvent(
            nameof(SelectionChanged),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(LargeTextBox));

        public event RoutedEventHandler SelectionChanged
        {
            add => AddHandler(SelectionChangedEvent, value);
            remove => RemoveHandler(SelectionChangedEvent, value);
        }

        public string SelectedText
        {
            get => _visualHost.SelectionAny ? Text.Substring(_visualHost.SelectionStart, _visualHost.SelectionLength) : null;
            set => ReplaceSelected(value);
        }

        public int SelectionLength
        {
            get => _visualHost.SelectionLength;
            set => _visualHost.SetSelection(_visualHost.SelectionStart, Math.Abs(value));
        }

        public int SelectionStart
        {
            get => _visualHost.SelectionStart;
            set => _visualHost.SetSelection(Math.Abs(value), _visualHost.SelectionLength);
        }

        public int CaretIndex
        {
            get => _visualHost.CaretCharOffset;
            set => _visualHost.SetCaretPosition(new CaretPoint(CaretPointOwners.Anyone, Math.Abs(value)));
        }
    }
}