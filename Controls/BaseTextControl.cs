using System.Windows;
using System.Windows.Controls;
using OpenFontWPFControls.Layout;

namespace OpenFontWPFControls.Controls
{
    public class BaseTextControl : Control
    {
        public static readonly DependencyProperty DrawingBoundsProperty =
            DependencyProperty.Register(
                nameof(DrawingBounds),
                typeof(Rect),
                typeof(BaseTextControl),
                new FrameworkPropertyMetadata(new Rect(0, 0, double.PositiveInfinity, double.PositiveInfinity)));

        /// <summary>
        /// Not readable if PART_ContentHost is ScrollViewer
        /// </summary>
        public Rect DrawingBounds
        {
            get => (Rect)GetValue(DrawingBoundsProperty);
            set => SetValue(DrawingBoundsProperty, value);
        }


        public static readonly DependencyProperty FontExtensionProperty =
            DependencyProperty.Register(
                nameof(FontExtension),
                typeof(string),
                typeof(BaseTextControl),
                new FrameworkPropertyMetadata(TypefaceInfo.DefaultExtension));

        public string FontExtension
        {
            get => (string)GetValue(FontExtensionProperty);
            set => SetValue(FontExtensionProperty, value);
        }

        
        public static readonly DependencyProperty TextTrimmingProperty =
            DependencyProperty.Register(
                nameof(TextTrimming),
                typeof(TextTrimming),
                typeof(BaseTextControl),
                new FrameworkPropertyMetadata(TextTrimming.None));

        public TextTrimming TextTrimming
        {
            get => (TextTrimming)GetValue(TextTrimmingProperty);
            set => SetValue(TextTrimmingProperty, value);
        }


        public static readonly DependencyProperty UnderlineProperty =
            DependencyProperty.Register(
                nameof(Underline),
                typeof(bool),
                typeof(BaseTextControl),
                new FrameworkPropertyMetadata(false));

        public bool Underline
        {
            get => (bool)GetValue(UnderlineProperty);
            set => SetValue(UnderlineProperty, value);
        }


        public static readonly DependencyProperty StrikeProperty =
            DependencyProperty.Register(
                nameof(Strike),
                typeof(bool),
                typeof(BaseTextControl),
                new FrameworkPropertyMetadata(false));

        public bool Strike
        {
            get => (bool)GetValue(StrikeProperty);
            set => SetValue(StrikeProperty, value);
        }
    }
}
