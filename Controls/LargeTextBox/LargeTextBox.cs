using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace OpenFontWPFControls.Controls
{
    /// <remarks>
    /// Preview version. May be significant changes
    /// </remarks>
    [TemplatePart(Name = "PART_ContentHost", Type = typeof(ContentPresenter))]
    [TemplatePart(Name = "PART_VerticalScroll", Type = typeof(ScrollBar))]
    [TemplatePart(Name = "PART_HorizontalScroll", Type = typeof(ScrollBar))]
    public partial class LargeTextBox : BaseTextControl
    {
        internal const string ContentHostTemplateName = "PART_ContentHost";
        internal const string VerticalScrollTemplateName = "PART_VerticalScroll";
        internal const string HorizontalScrollTemplateName = "PART_HorizontalScroll";

        private readonly LargeTextVisualHost _visualHost;
        private ContentPresenter _textBoxContentHost;
        private ScrollBar _verticalScrollBar;
        private ScrollBar _horizontalScrollBar;

        private bool _skipTextChanged;
        private bool _skipFocusChanged;

        private readonly TextUndoBuffer _undoBuffer = new TextUndoBuffer();
        private TextUndoUnit _lastUndoUnit;
        private int _lastChangeOffset;
        private int _lastChangeLength;


        static LargeTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LargeTextBox), new FrameworkPropertyMetadata(typeof(LargeTextBox)));
            CommandRegister();
        }


        public LargeTextBox()
        {
            _visualHost = new LargeTextVisualHost(this, TextProperty);
            _visualHost.OnDrawCallBack = UpdateScrollValues;
            _visualHost.OnXOffsetChangeCallBack = OnXOffsetChangeCallBack;
            _visualHost.OnSelectionChangeCallBack = () => RaiseEvent(new RoutedEventArgs(SelectionChangedEvent));
            _visualHost.CharIsError = CharIsError;
        }


        public override void OnApplyTemplate()
        {
            if (_textBoxContentHost != null)
            {
                _textBoxContentHost.Content = null;
                _textBoxContentHost.SizeChanged -= ContentHost_SizeChanged;
            }

            if ((_textBoxContentHost = GetTemplateChild(ContentHostTemplateName) as ContentPresenter) != null)
            {
                _textBoxContentHost.Content = _visualHost;
                _textBoxContentHost.SizeChanged += ContentHost_SizeChanged;
            }

            if (_verticalScrollBar != null)
            {
                _verticalScrollBar.ValueChanged -= Vertical_Scroll;
            }

            if ((_verticalScrollBar = GetTemplateChild(VerticalScrollTemplateName) as ScrollBar) != null)
            {
                _verticalScrollBar.ValueChanged += Vertical_Scroll;
            }

            if (_horizontalScrollBar != null)
            {
                _horizontalScrollBar.ValueChanged -= Horizontal_Scroll;
            }

            if ((_horizontalScrollBar = GetTemplateChild(HorizontalScrollTemplateName) as ScrollBar) != null)
            {
                _horizontalScrollBar.ValueChanged += Horizontal_Scroll;
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
                    _visualHost.UpdateControlLayer();
                    break;
            }
        }


        // Mouse & Scroll

        private void UpdateScrollValues()
        {
            if (_verticalScrollBar != null)
            {
                _verticalScrollBar.Maximum = Text.Length - 1;
                if (_verticalScrollBar.Value < _visualHost.StartCharOffset || _verticalScrollBar.Value >= _visualHost.NextLineCharOffset)
                {
                    _verticalScrollBar.Value = _visualHost.StartCharOffset;
                }
            }

            if (_horizontalScrollBar != null)
            {
                if (float.IsInfinity(_visualHost.Layout.MaxWidth))
                {
                    _horizontalScrollBar.ViewportSize = 0;
                    _horizontalScrollBar.Maximum = 0;
                }
                else
                {
                    _horizontalScrollBar.ViewportSize = _visualHost.Layout.MaxWidth;
                    _horizontalScrollBar.Maximum = (_visualHost.DrawMaxWidth < _horizontalScrollBar.Value ? _horizontalScrollBar.Value : _visualHost.DrawMaxWidth) - _visualHost.Layout.MaxWidth;
                }
            }
        }

        private void OnXOffsetChangeCallBack()
        {
            _horizontalScrollBar.Value = 0 - _visualHost.ViewXOffset;
        }

        private void ContentHost_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateScrollValues();
        }

        private void Horizontal_Scroll(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _visualHost.SetXOffset(0 - e.NewValue);
            e.Handled = true;
        }

        private void Vertical_Scroll(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _visualHost.SetStartChar((int)e.NewValue);
            e.Handled = true;
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            if (CanVerticalScroll)
            {
                _visualHost.ChangeStartLine(0 - e.Delta / 120 * 3);
                e.Handled = true;
            }
        }

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