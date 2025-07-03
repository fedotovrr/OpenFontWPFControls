using OpenFontWPFControls.FormattingStructure;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace OpenFontWPFControls.Controls
{
    partial class LargeTextBox
    {
        public static readonly DependencyProperty CaretBrushProperty = 
            DependencyProperty.Register(
                nameof(CaretBrush),
                typeof(Brush),
                typeof(LargeTextBox),
                new FrameworkPropertyMetadata(Brushes.Black));

        public Brush CaretBrush
        {
            get => (Brush)GetValue(CaretBrushProperty);
            set => SetValue(CaretBrushProperty, value);
        }


        public static readonly DependencyProperty SelectionBrushProperty =
            DependencyProperty.Register(
                nameof(SelectionBrush),
                typeof(Brush),
                typeof(LargeTextBox),
                new FrameworkPropertyMetadata(Brushes.CornflowerBlue));

        public Brush SelectionBrush
        {
            get => (Brush)GetValue(SelectionBrushProperty);
            set => SetValue(SelectionBrushProperty, value);
        }


        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                nameof(Text),
                typeof(string),
                typeof(LargeTextBox),
                new FrameworkPropertyMetadata(
                    string.Empty,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal,
                    TextChangedCallback,
                    (o, value) => value ?? string.Empty,
                    true,
                    UpdateSourceTrigger.LostFocus
                ));

        [DefaultValue("")]
        [Localizability(LocalizationCategory.Text)]
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        private static void TextChangedCallback(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if (o is LargeTextBox box)
            {
                box.EditorOnTextChanged();
                box.SpellerOnTextChanged();
            }
        }


        public static readonly DependencyProperty SpellCheckProperty =
            DependencyProperty.Register(
                nameof(SpellCheck),
                typeof(bool),
                typeof(LargeTextBox),
                new FrameworkPropertyMetadata(false));

        public bool SpellCheck
        {
            get => (bool)GetValue(SpellCheckProperty);
            set => SetValue(SpellCheckProperty, value);
        }


        public static readonly DependencyProperty SpellLanguageProperty =
            DependencyProperty.Register(
                nameof(SpellLanguage),
                typeof(string),
                typeof(LargeTextBox),
                new FrameworkPropertyMetadata(CultureInfo.CurrentCulture.Name));

        public string SpellLanguage
        {
            get => (string)GetValue(SpellLanguageProperty);
            set => SetValue(SpellLanguageProperty, value);
        }


        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register(
                nameof(IsReadOnly),
                typeof(bool),
                typeof(LargeTextBox),
                new FrameworkPropertyMetadata(false));

        public bool IsReadOnly
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }


        public static readonly DependencyProperty CanVerticalScrollProperty =
            DependencyProperty.Register(
                nameof(CanVerticalScroll),
                typeof(bool),
                typeof(LargeTextBox),
                new FrameworkPropertyMetadata(true));

        public bool CanVerticalScroll
        {
            get => (bool)GetValue(CanVerticalScrollProperty);
            set => SetValue(CanVerticalScrollProperty, value);
        }


        public static readonly DependencyProperty CanHorizontalScrollProperty =
            DependencyProperty.Register(
                nameof(CanHorizontalScroll),
                typeof(bool),
                typeof(LargeTextBox),
                new FrameworkPropertyMetadata(true));

        public bool CanHorizontalScroll
        {
            get => (bool)GetValue(CanHorizontalScrollProperty);
            set => SetValue(CanHorizontalScrollProperty, value);
        }


        public static readonly DependencyProperty DisableInputLineBreaksProperty =
            DependencyProperty.Register(
                nameof(DisableInputLineBreaks),
                typeof(bool),
                typeof(LargeTextBox),
                new FrameworkPropertyMetadata(false));

        public bool DisableInputLineBreaks
        {
            get => (bool)GetValue(DisableInputLineBreaksProperty);
            set => SetValue(DisableInputLineBreaksProperty, value);
        }
        
    }
}