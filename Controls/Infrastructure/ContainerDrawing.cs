using System.Windows.Media;
using OpenFontWPFControls.Layout;

namespace OpenFontWPFControls.Controls
{
    internal class ContainerDrawing
    {
        public readonly IVisualGenerator Source;
        public readonly DrawingVisual Visual;
        public bool Used;
        private bool _valid;
        
        public bool Valid => _valid;

        public ContainerDrawing(IVisualGenerator source, bool used = true)
        {
            Source = source;
            Visual = source.CreateDrawingVisual();
            Used = used;
            _valid = true;
        }

        public void Invalidate()
        {
            _valid = false;
        }
    }
}
