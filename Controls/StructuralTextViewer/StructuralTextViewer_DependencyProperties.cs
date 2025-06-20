using OpenFontWPFControls.FormattingStructure;
using System.Windows;

namespace OpenFontWPFControls.Controls
{
    partial class StructuralTextViewer
    {
        public static readonly DependencyProperty FormattingStructureProperty =
            DependencyProperty.Register(
                nameof(FormattingStructure),
                typeof(IContainersCollection),
                typeof(StructuralTextViewer),
                new FrameworkPropertyMetadata(new DefaultFormattingStructure(), FrameworkPropertyMetadataOptions.AffectsMeasure, OnStructurePropertyChange));

        public IContainersCollection FormattingStructure
        {
            get => (IContainersCollection)GetValue(FormattingStructureProperty);
            set => SetValue(FormattingStructureProperty, value);
        }

        private static void OnStructurePropertyChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is StructuralTextViewer o)
            {
                if (e.OldValue is INotifyStructureChanged oldStructuralChanged)
                {
                    StructureChangedEventManager.RemoveHandler(oldStructuralChanged, o.StructureChanged);
                }

                if (e.NewValue is INotifyStructureChanged newStructuralChanged)
                {
                    StructureChangedEventManager.AddHandler(newStructuralChanged, o.StructureChanged);
                }
            }
        }

        private void StructureChanged(object sender, StructureChangedEventArgs e)
        {
            _visualHost.Invalidate();
        }
    }
}
