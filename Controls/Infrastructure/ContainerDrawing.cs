using System.Windows.Media;
using OpenFontWPFControls.Layout;

namespace OpenFontWPFControls.Controls
{
    internal class ContainerDrawing
    {
        public readonly IVisualGenerator Source;
        public readonly DrawingVisual Visual;
        public bool Valid;

        public ContainerDrawing(IVisualGenerator source)
        {
            Source = source;
            Visual = source.CreateDrawingVisual();
            Valid = true;
        }
    }
}
