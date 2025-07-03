using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OpenFontWPFControls.Controls
{
    [TemplatePart(Name = "PART_ContentHost", Type = typeof(ContentControl))]
    public partial class TextBox : BaseTextControl
    {
        internal const string ContentHostTemplateName = "PART_ContentHost";

        private readonly TextVisualHost _visualHost;
        private ContentControl _textBoxContentHost;

        private bool _skipTextChanged;
        private bool _skipFocusChanged;

        private readonly TextUndoBuffer _undoBuffer = new TextUndoBuffer();
        private TextUndoUnit _lastUndoUnit;
        private int _lastChangeOffset;
        private int _lastChangeLength;


        static TextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TextBox), new FrameworkPropertyMetadata(typeof(TextBox)));
            CommandRegister();
        }


        public TextBox()
        {
            _visualHost = new TextVisualHost(this, TextProperty);
            _visualHost.OnSelectionChangeCallBack = () => RaiseEvent(new RoutedEventArgs(SelectionChangedEvent));
            _visualHost.CharIsError = CharIsError;
        }


        public override void OnApplyTemplate()
        {
            if (_textBoxContentHost != null)
            {
                _textBoxContentHost.Content = null;
            }
            if ((_textBoxContentHost = GetTemplateChild(ContentHostTemplateName) as ContentControl) != null)
            {
                _textBoxContentHost.Content = _visualHost;
                _textBoxContentHost.Focusable = false;
                _textBoxContentHost.ClipToBounds = true;
                if (_textBoxContentHost is ScrollViewer scroll)
                {
                    scroll.CanContentScroll = true;
                }
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

                case not null when e.Property == TextProperty:
                case not null when e.Property == TextTrimmingProperty:
                case not null when e.Property == ForegroundProperty:
                case not null when e.Property == FontSizeProperty:
                case not null when e.Property == FontStyleProperty:
                case not null when e.Property == FontWeightProperty:
                case not null when e.Property == FontFamilyProperty:
                case not null when e.Property == FontExtensionProperty:
                case not null when e.Property == UnderlineProperty:
                case not null when e.Property == StrikeProperty:
                case not null when e.Property == SpellCheckProperty:
                case not null when e.Property == SpellLanguageProperty:
                    _visualHost.Invalidate();
                    break;

                case not null when e.Property == CaretBrushProperty:
                    _visualHost.InvalidateControlLayer();
                    break;
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            Focus();
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
                (int offset, int length) = StaticHelper.GetWordByOffset(Text, _visualHost.CaretCharOffset);
                _visualHost.SetSelection(offset, length);
            }
            else
            {
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
            if (!_visualHost.SelectionAny)
            {
                _visualHost.SetCaretPosition(e.GetPosition(_visualHost));
            }
            e.Handled = true;
        }
    }
}