namespace OpenFontWPFControls.Controls
{
    /// <summary>
    /// Interface for storing a scroll value in a context <see cref="ItemsPanel.ItemsSource"/> 
    /// </summary>
    public interface IScrollValueContainer
    {
        double Value { get; set; }
    }
}