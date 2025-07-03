using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace OpenFontWPFControls.FormattingStructure
{
    public delegate void StructureChangedEventHandler(object sender, StructureChangedEventArgs e);

    public class StructureChangedEventArgs : System.EventArgs
    {
        private readonly object _item;

        public object StructureItem => _item;

        public StructureChangedEventArgs(object item) => _item = item;
    }

    public interface INotifyStructureChanged
    {
        event StructureChangedEventHandler StructureChanged;
    }

    public interface IContainersCollection
    {
        /// <summary>
        /// where object as IInlineCollection or ITable other ignore
        /// </summary>
        IEnumerable<object> Items { get; }
    }

    public interface IInlineCollection
    {
        /// <summary>
        /// where object as string or IText or IFontSize or IForeground or IFontWeight or IFontStyle or IUnderline or IStrike other ignore
        /// </summary>
        IEnumerable<object> Items { get; }
    }

    public interface ITextAlignment
    {
        TextAlignment TextAlignment { get; }
    }

    public interface IText
    {
        public string Text { get; }
    }

    public interface IFontSize
    {
        public float FontSize { get; }
    }

    public interface IForeground
    {
        public Brush Foreground { get; }
    }

    public interface IFontWeight
    {
        FontWeight FontWeight { get; }
    }

    public interface IFontStyle
    {
        FontStyle FontStyle { get; }
    }

    public interface IUnderline
    {
        bool Underline { get; }
    }

    public interface IStrike
    {
        bool Strike { get; }
    }



    public interface ITable
    {
        IEnumerable<ITableRow> Rows { get; }
    }

    public interface ITableRow
    {
        IEnumerable<ITableCell> Cells { get; }
    }

    public interface ITableCell
    {
        public float Width { get; }
    }

    public interface IBorder
    {
        SolidColorBrush Background { get; }

        SolidColorBrush BorderBrush { get; }

        Thickness BorderThickness { get; }

        CornerRadius CornerRadius { get; }

        Thickness Margin { get; }

        Thickness Padding { get; }
    }

    /// <summary>
    /// Object has hit handling
    /// </summary>
    public interface IInlineImage
    {
        public float Width { get; }

        public float Height { get; }

        public ImageSource OverSource { get; }

        public ImageSource Source { get; }

        public object ToolTip { get; }
    }

    /// <summary>
    /// Object has hit handling
    /// </summary>
    public interface IHyperlink
    {
        public string Text { get; }

        public Brush OverForeground { get; }

        public object ToolTip { get; }

        public void Navigate();
    }
}
