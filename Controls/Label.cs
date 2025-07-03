
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;

namespace OpenFontWPFControls.Controls
{
    [TemplatePart(Name = "PART_ContentHost", Type = typeof(ContentPresenter))]
    public class Label : BaseTextControl
    {
        internal const string ContentHostTemplateName = "PART_ContentHost";

        private readonly LargeTextVisualHost _visualHost;
        private ContentPresenter _textBoxContentHost;


        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                nameof(Text),
                typeof(string),
                typeof(Label),
                new FrameworkPropertyMetadata(string.Empty));

        [DefaultValue("")]
        [Localizability(LocalizationCategory.Text)]
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }


        static Label()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Label), new FrameworkPropertyMetadata(typeof(Label)));
        }

        public Label()
        {
            _visualHost = new LargeTextVisualHost(this, TextProperty);
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
                    _visualHost.Invalidate();
                    break;
            }
        }
    }
}
