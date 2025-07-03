using System;
using System.Collections.Generic;
using System.Windows;

namespace OpenFontWPFControls.Controls
{
    partial class StructuralTextViewer
    {
        public static readonly RoutedEvent HitObjectMouseLeftButtonDownEvent = EventManager.RegisterRoutedEvent(
            nameof(HitObjectMouseLeftButtonDown),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(StructuralTextViewer));

        public event RoutedEventHandler HitObjectMouseLeftButtonDown
        {
            add => AddHandler(HitObjectMouseLeftButtonDownEvent, value);
            remove => RemoveHandler(HitObjectMouseLeftButtonDownEvent, value);
        }


        public static readonly RoutedEvent HitObjectMouseRightButtonDownEvent = EventManager.RegisterRoutedEvent(
            nameof(HitObjectMouseRightButtonDown),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(StructuralTextViewer));

        public event RoutedEventHandler HitObjectMouseRightButtonDown
        {
            add => AddHandler(HitObjectMouseRightButtonDownEvent, value);
            remove => RemoveHandler(HitObjectMouseRightButtonDownEvent, value);
        }


        public static readonly RoutedEvent SelectionChangedEvent = EventManager.RegisterRoutedEvent(
            nameof(SelectionChanged),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(StructuralTextViewer));

        public event RoutedEventHandler SelectionChanged
        {
            add => AddHandler(SelectionChangedEvent, value);
            remove => RemoveHandler(SelectionChangedEvent, value);
        }

        public string Text => _visualHost.GetText();

        public IEnumerable<(object source, int offset, int length)> GetSelectedObjects => _visualHost.GetSelectedObjects();

        public string SelectedText => _visualHost.GetSelectedText();

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

        //public int CaretIndex
        //{
        //    get => _visualHost.CaretCharOffset;
        //    set => _visualHost.SetCaretPosition(new CaretPoint(CaretPointOwners.Anyone, Math.Abs(value)));
        //}
    }
}