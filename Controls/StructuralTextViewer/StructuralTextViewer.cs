using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using OpenFontWPFControls.FormattingStructure;

namespace OpenFontWPFControls.Controls
{
    [TemplatePart(Name = "PART_ContentHost", Type = typeof(ContentPresenter))]
    public partial class StructuralTextViewer : BaseTextControl
    {
        internal const string ContentHostTemplateName = "PART_ContentHost";

        private readonly StructuralTextVisualHost _visualHost;
        private ContentPresenter _textBoxContentHost;

        private bool _skipFocusChanged;


        static StructuralTextViewer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(StructuralTextViewer), new FrameworkPropertyMetadata(typeof(StructuralTextViewer)));
            CommandRegister();
        }

        public StructuralTextViewer()
        {
            _visualHost = new StructuralTextVisualHost(this);
            _visualHost.OnHitObjectMouseLeftButtonDownCallBack = o => LeftButtonDownEventInvoke(HitObjectMouseLeftButtonDownEvent, o);
            _visualHost.OnHitObjectMouseRightButtonDownCallBack = o => EventInvoke(HitObjectMouseRightButtonDownEvent, o);
        }

        public override void OnApplyTemplate()
        {
            if (_textBoxContentHost != null)
            {
                _textBoxContentHost.Content = null;
            }
            if ((_textBoxContentHost = GetTemplateChild(ContentHostTemplateName) as ContentPresenter) != null)
            {
                _textBoxContentHost.Content = _visualHost;
            }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            switch (e.Property)
            {
                case not null when e.Property == IsKeyboardFocusedProperty:
                    if (!_skipFocusChanged)
                    {
                        _visualHost.SetControlLayerVisibility((bool)e.NewValue);
                    }

                    break;

                case not null when e.Property == DrawingBoundsProperty:
                    _visualHost.InvalidateDrawing();
                    break;

                case not null when e.Property == FormattingStructureProperty:
                case not null when e.Property == TextTrimmingProperty:
                case not null when e.Property == ForegroundProperty:
                case not null when e.Property == FontSizeProperty:
                case not null when e.Property == FontStyleProperty:
                case not null when e.Property == FontWeightProperty:
                case not null when e.Property == FontFamilyProperty:
                case not null when e.Property == FontExtensionProperty:
                    _visualHost.Invalidate();
                    break;
            }
        }


        // Evets

        private void LeftButtonDownEventInvoke(RoutedEvent e, object hitObject)
        {
            if (!EventInvoke(e, hitObject) && hitObject is IHyperlink link)
            {
                link.Navigate();
            }
        }

        private bool EventInvoke(RoutedEvent e, object hitObject)
        {
            if (e != null)
            {
                HitActionEventArgs args = new HitActionEventArgs(e, this, hitObject);
                RaiseEvent(args);
                return args.Handled;
            }
            return false;
        }


        // Mouse

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {

        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (Equals(Mouse.Captured))
            {
                _visualHost.ChangeSelection(e.GetPosition(_visualHost));
                e.Handled = true;
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                SelectWord();
            }
            else
            {
                Focus();
                _visualHost.SetCaretPosition(e.GetPosition(_visualHost));
                Mouse.Capture(this);
            }
            e.Handled = true;
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
            e.Handled = true;
        }

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            Focus();
            if (!_visualHost.SelectionAny)
            {
                _visualHost.SetCaretPosition(e.GetPosition(_visualHost));
            }
            e.Handled = true;
        }
    }
}