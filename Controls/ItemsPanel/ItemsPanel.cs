using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace OpenFontWPFControls.Controls
{
    /// <summary>
    /// Example of a panel with set <see cref="BaseTextControl.DrawingBounds"/><br/>
    /// Always virtualizing<br/>
    /// Mouse scroll - pixel<br/>
    /// Scroll bar - logical<br/>
    /// </summary>
    [TemplatePart(Name = "PART_ContentHost", Type = typeof(ContentPresenter))]
    [TemplatePart(Name = "PART_VerticalScrollBar", Type = typeof(ScrollBar))]
    public class ItemsPanel : Control
    {
        internal const string ContentHostTemplateName = "PART_ContentHost";
        internal const string VerticalScrollBarTemplateName = "PART_VerticalScrollBar";

        private readonly ItemsPanelVisualHost _visualHost;
        private ContentPresenter _contentHost;
        private ScrollBar _verticalScrollBar;
        //private readonly ICommand _scrollIntoView;

        //public ICommand ScrollIntoViewCommand => _scrollIntoView;

        static ItemsPanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ItemsPanel), new FrameworkPropertyMetadata(typeof(ItemsPanel)));
        }

        public ItemsPanel()
        {
            _visualHost = new ItemsPanelVisualHost();
            _visualHost.ContentStretch = HorizontalContentAlignment == HorizontalAlignment.Stretch;
            _visualHost.RenderCallBack = RenderCallBack;
            //_scrollIntoView = new Command<object>(ScrollIntoView);
        }


        public override void OnApplyTemplate()
        {
            if (_contentHost != null)
            {
                _contentHost.Content = null;
            }
            if ((_contentHost = GetTemplateChild(ContentHostTemplateName) as ContentPresenter) != null)
            {
                _contentHost.Content = _visualHost;
            }

            if (_verticalScrollBar != null)
            {
                _verticalScrollBar.ValueChanged -= VerticalScrollBar_ValueChanged;
            }
            if ((_verticalScrollBar = GetTemplateChild(VerticalScrollBarTemplateName) as ScrollBar) != null)
            {
                _verticalScrollBar.ValueChanged += VerticalScrollBar_ValueChanged;
            }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            switch (e.Property)
            {
                case DependencyProperty _ when e.Property == HorizontalContentAlignmentProperty:
                    _visualHost.ContentStretch = HorizontalContentAlignment == HorizontalAlignment.Stretch;
                    break;
            }
            base.OnPropertyChanged(e);
        }


        // Mouse and scroll

        private void VerticalScrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _visualHost.FirstView = e.NewValue;
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            double offset = 50 * (e.Delta > 0 ^ Inverse ? 1 : -1);
            if (_verticalScrollBar != null && (offset > 0 ? _verticalScrollBar.Value > 0 : _verticalScrollBar.Value < _verticalScrollBar.Maximum))
            {
                _visualHost.ScrollByPixel(offset);
                e.Handled = true;
            }
        }

        private void RenderCallBack()
        {
            if (_verticalScrollBar != null)
            {
                _verticalScrollBar.Value = _visualHost.FirstView;
                _verticalScrollBar.Maximum = _visualHost.SourceCount - _visualHost.ViewCount;
                _verticalScrollBar.ViewportSize = _visualHost.ViewCount;
            }
        }

        public bool InView(object item, bool full)
        {
            return _visualHost.InView(item, full);
        }

        public void ScrollIntoView(object item)
        {
            _visualHost.IntoView(item);
        }

        public void ScrollIntoView(int index)
        {
            _visualHost.IntoView(index);
        }

        public void SetScrollValue(double value)
        {
            _visualHost.FirstView = value;
        }

        public double GetScrollValue()
        {
            return _visualHost.FirstView;
        }


        // DependencyProperties

        public bool Inverse
        {
            get => _visualHost.Inverse;
            set => SetValue(InverseProperty, value);
        }

        public static readonly DependencyProperty InverseProperty = DependencyProperty.Register(
            nameof(Inverse),
            typeof(bool),
            typeof(ItemsPanel),
            new FrameworkPropertyMetadata(false, InversePropertyChange));

        private static void InversePropertyChange(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if (o is ItemsPanel panel)
            {
                panel._visualHost.Inverse = (bool)e.NewValue;
            }
        }


        /// <summary>
        /// When updating the source, if the element that was at the base of the scroll is present,
        /// then the scroll logical value is reset to its new index in the source
        /// </summary>
        public bool TryScrollTopItemAfterChanged
        {
            get => _visualHost.TryScrollTopItemAfterChanged;
            set => SetValue(TryScrollTopItemAfterChangedProperty, value);
        }

        public static readonly DependencyProperty TryScrollTopItemAfterChangedProperty = DependencyProperty.Register(
            nameof(TryScrollTopItemAfterChanged),
            typeof(bool),
            typeof(ItemsPanel),
            new FrameworkPropertyMetadata(true, TryScrollTopItemAfterChangedPropertyChange));

        private static void TryScrollTopItemAfterChangedPropertyChange(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if (o is ItemsPanel panel)
            {
                panel._visualHost.TryScrollTopItemAfterChanged = (bool)e.NewValue;
            }
        }


        /// <summary>
        /// When <see cref="TryScrollTopItemAfterChanged"/> don't try to reset if the element was at the first of the collection
        /// </summary>
        public bool NotScrollIfTopItemFirst
        {
            get => _visualHost.NotScrollIfTopItemFirst;
            set => SetValue(NotScrollIfTopItemFirstProperty, value);
        }

        public static readonly DependencyProperty NotScrollIfTopItemFirstProperty = DependencyProperty.Register(
            nameof(NotScrollIfTopItemFirst),
            typeof(bool),
            typeof(ItemsPanel),
            new FrameworkPropertyMetadata(false, NotScrollIfTopItemFirstPropertyChange));

        private static void NotScrollIfTopItemFirstPropertyChange(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if (o is ItemsPanel panel)
            {
                panel._visualHost.NotScrollIfTopItemFirst = (bool)e.NewValue;
            }
        }


        public DataTemplate ItemTemplate
        {
            get => _visualHost.ItemTemplate;
            set => SetValue(ItemTemplateProperty, value);
        }

        public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register(
            nameof(ItemTemplate),
            typeof(DataTemplate),
            typeof(ItemsPanel),
            new FrameworkPropertyMetadata(null, ItemTemplateChange));

        private static void ItemTemplateChange(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if (o is ItemsPanel panel)
            {
                panel._visualHost.ItemTemplate = e.NewValue as DataTemplate;
            }
        }


        public object ItemsSource
        {
            get => _visualHost.ItemsSource;
            set => SetValue(ItemsSourceProperty, value);
        }

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            nameof(ItemsSource),
            typeof(object),
            typeof(ItemsPanel),
            new FrameworkPropertyMetadata(null, ItemsSourceChange));

        private static void ItemsSourceChange(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if (o is ItemsPanel panel)
            {
                panel._visualHost.ItemsSource = e.NewValue;
            }
        }

    }
}
