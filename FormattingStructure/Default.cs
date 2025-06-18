using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Collections.Specialized;
using System.Diagnostics;

namespace OpenFontWPFControls.FormattingStructure
{
    public class DefaultFormattingStructure : FormattingCollection<Container>, IContainersCollection, INotifyStructureChanged
    {
        public event StructureChangedEventHandler StructureChanged;

        IEnumerable<object> IContainersCollection.Items => this;

        public void OnNotifyStructureChanged(object item) => StructureChanged?.Invoke(sender: this, e: new StructureChangedEventArgs(item));
    }

    public abstract class Container : FormattingCollection<FormattingStructureItem>
    {

    }

    public class TextContainer : Container, IInlineCollection
    {
        IEnumerable<object> IInlineCollection.Items => this;
    }

    public abstract class Inline : FormattingCollection<FormattingStructureItem>, IInlineCollection
    {
        IEnumerable<object> IInlineCollection.Items => this;
    }

    public class TextFragment : FormattingStructureItem, IText
    {
        private string _value;

        public string Text
        {
            get => _value;
            set => SetField(ref _value, value);
        }

        public TextFragment() => _value = string.Empty;

        public TextFragment(string value) => _value = value;

        public override string ToString() => _value;
    }

    public class FontSize : Inline, IFontSize
    {
        private float _value;

        public float Value
        {
            get => _value;
            set => SetField(ref _value, value);
        }

        float IFontSize.FontSize => _value;


        public FontSize() => _value = (float)SystemFonts.MessageFontSize;

        public FontSize(float value) => _value = value;
    }

    public class Foreground : Inline, IForeground
    {
        private Brush _value;

        public Brush Value
        {
            get => _value;
            set => SetField(ref _value, value);
        }

        Brush IForeground.Foreground => _value;

        public Foreground() => _value = Brushes.Black;

        public Foreground(Brush value) => _value = value;
    }

    public class TextBold : Inline, IFontWeight
    {
        public FontWeight FontWeight => FontWeights.Bold;
    }

    public class TextItalic : Inline, IFontStyle
    {
        public FontStyle FontStyle => FontStyles.Italic;
    }

    public class TextUnderline : Inline, IUnderline
    {
        public bool Underline => true;
    }

    public class TextStrike : Inline, IStrike
    {
        public bool Strike => true;
    }

    public class InlineImage : FormattingStructureItem, IInlineImage
    {
        public float Width => Source == null ? 0f : (float)Source.Width;

        public float Height => Source == null ? 0f : (float)Source.Height;

        public ImageSource Source { get; set; }

        public object ToolTip => null;
    }

    public class Hyperlink : TextFragment, IHyperlink, IForeground, IUnderline
    {
        public Uri Uri { get; set; }

        public Brush OverForeground => Brushes.CornflowerBlue;

        Brush IForeground.Foreground => Brushes.RoyalBlue;

        bool IUnderline.Underline => true;

        public object ToolTip => Uri?.OriginalString;

        public void Navigate()
        {
            if (Uri != null && !string.IsNullOrWhiteSpace(Uri.AbsoluteUri))
            {
                Process.Start(Uri.AbsoluteUri);
            }
        }
    }


    public class TableContainer : Container, ITable, IBorder
    {
        IEnumerable<ITableRow> ITable.Rows => this.Cast<ITableRow>();

        public SolidColorBrush Background => Brushes.Transparent;

        public SolidColorBrush BorderBrush => Brushes.Black;

        public Thickness BorderThickness => new Thickness(1,1,0,0);

        public CornerRadius CornerRadius => new CornerRadius(0);

        public Thickness Margin => new Thickness(0);

        public Thickness Padding => new Thickness(0);
    }

    public class Row : FormattingCollection<Cell>, ITableRow
    {
        IEnumerable<ITableCell> ITableRow.Cells => this;
    }

    public class Cell : FormattingCollection<FormattingStructureItem>, ITableCell, IContainersCollection, IBorder
    {
        public float Width { get; set; }

        IEnumerable<object> IContainersCollection.Items => this;

        public SolidColorBrush Background => Brushes.Transparent;

        public SolidColorBrush BorderBrush => Brushes.Black;

        public Thickness BorderThickness => new Thickness(0,0,1,1);

        public CornerRadius CornerRadius => new CornerRadius(0);

        public Thickness Margin => new Thickness(0);

        public Thickness Padding => new Thickness(4);
    }


    public class FormattingStructureItem : INotifyPropertyChanged
    {
        private object _parent;

        public event PropertyChangedEventHandler PropertyChanged;

        public object Parent => _parent;

        internal void SetParent(object parent) => _parent = parent;

        public void OnNotifyStructureChanged()
        {
            object o = this;
            while (o != null)
            {
                if (o is DefaultFormattingStructure structure)
                {
                    structure.OnNotifyStructureChanged(this);
                    break;
                }

                o = (o as FormattingStructureItem)?.Parent;
            }
        }

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            OnNotifyStructureChanged();
        }

        public void SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                OnPropertyChanged(propertyName);
            }
        }
    }


    public class FormattingCollection<T> : FormattingStructureItem, IList, IEnumerable<T>, INotifyCollectionChanged where T : FormattingStructureItem
    {
        private List<T> _items = new List<T>();

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count => _items.Count;

        public object SyncRoot => ((IList)_items).SyncRoot;

        public bool IsSynchronized => false;

        public bool IsReadOnly => false;

        public bool IsFixedSize => false;

        public int IndexOf(object value) => ((IList)_items).IndexOf(value);

        public bool Contains(object value) => _items.Contains(value);

        public T this[int index]
        {
            get => Getter(index);
            set => Setter(index, value);
        }

        object IList.this[int index]
        {
            get => Getter(index);
            set => Setter(index, value);
        }

        private T Getter(int index)
        {
            return index >= 0 && index < _items.Count ? _items[index] : null;
        }

        private void Setter(int index, object value)
        {
            if (index >= 0 && index < _items.Count)
            {
                T item = _items[index];
                T newItem = value as T;
                if (item != newItem)
                {
                    if (newItem == null)
                    {
                        throw new ArgumentNullException();
                    }

                    if (newItem.Parent != null)
                    {
                        throw new ArgumentException(@"Value parent not null");
                    }

                    item.SetParent(null);
                    _items[index] = newItem;
                    newItem.SetParent(this);
                    OnNotifyStructureChanged();
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        public void Insert(int index, object value)
        {
            T item = value as T;
            if (item == null)
            {
                throw new ArgumentNullException();
            }
            if (item.Parent != null)
            {
                throw new ArgumentException(@"Item parent not null");
            }

            _items.Insert(index, item);
            item.SetParent(this);
            OnNotifyStructureChanged();
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public void Add(T item)
        {
            Add((object)item);
        }

        public int Add(object value)
        {
            T item = value as T;
            if (item == null)
            {
                throw new ArgumentNullException();
            }
            if (item.Parent != null)
            {
                throw new ArgumentException(@"Item parent not null");
            }

            int index = ((IList)_items).Add(item);
            if (index >= 0)
            {
                item.SetParent(this);
                OnNotifyStructureChanged();
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }

            return index;
        }

        public void AddRange(IEnumerable<T> collection)
        {
            T[] items = collection?.ToArray();

            if (items == null || items.Length == 0)
            {
                return;
            }

            foreach (T item in items)
            {
                if (item == null)
                {
                    throw new ArgumentNullException();
                }

                if (item.Parent != null)
                {
                    throw new ArgumentException(@"Item parent not null");
                }
            }

            foreach (T item in items)
            {
                _items.Add(item);
                item.SetParent(this);
            }

            OnNotifyStructureChanged();
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public void Remove(T item)
        {
            Remove((object)item);
        }

        public void Remove(object value)
        {
            T item = value as T;
            if (item == null)
            {
                throw new ArgumentNullException();
            }

            if (item.Parent == this && _items.Remove(item))
            {
                item.SetParent(null);
                OnNotifyStructureChanged();
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        public void RemoveAt(int index)
        {
            if (index >= 0 && index < _items.Count)
            {
                T item = _items[index];
                _items.RemoveAt(index);
                item.SetParent(null);
                OnNotifyStructureChanged();
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        public void Clear()
        {
            if (_items.Any())
            {
                List<T> old = _items;
                _items = new List<T>();
                old.ForEach(item => item.SetParent(null));
                OnNotifyStructureChanged();
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }
    }
}
