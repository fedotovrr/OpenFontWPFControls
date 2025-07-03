using OpenFontWPFControls.FormattingStructure;
using System.Collections.Specialized;
using System;
using System.Windows;

namespace OpenFontWPFControls.Controls
{
    partial class StructuralTextViewer : IWeakEventListener
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
                    StructureChangedEventManager.RemoveListener(oldStructuralChanged, o);
                }

                if (e.NewValue is INotifyStructureChanged newStructuralChanged)
                {
                    StructureChangedEventManager.AddListener(newStructuralChanged, o);
                }
            }
        }

        public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (e is StructureChangedEventArgs)
            {
                _visualHost.Invalidate();
            }
            return true;
        }
    }
}