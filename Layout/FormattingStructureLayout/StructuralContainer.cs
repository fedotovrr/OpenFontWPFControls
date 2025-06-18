using OpenFontWPFControls.Layout.FormattingStructureLayout;
using System.Collections.Generic;
using System.Windows.Media;

namespace OpenFontWPFControls.Layout
{
    public class StructuralContainer
    {
        public readonly StructuralLayout Layout;
        public readonly List<StructuralTextItem> TextContainers;
        public readonly List<StructuralLine> Lines = new List<StructuralLine>();
        public readonly List<StructuralBorder> Borders = new List<StructuralBorder>();
        public readonly List<ContainerVisual> Controls = new List<ContainerVisual>();
        public readonly List<HitBox> Hyperlinks = new List<HitBox>();


        public StructuralContainer(List<StructuralTextItem> textContainers, StructuralLayout layout = null)
        {
            TextContainers = textContainers ?? new List<StructuralTextItem>();
            Layout = layout;
        }
    }
}
