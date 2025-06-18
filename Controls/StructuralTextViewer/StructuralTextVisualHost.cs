using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using OpenFontWPFControls.Layout;
using OpenFontWPFControls.Layout.FormattingStructureLayout;

namespace OpenFontWPFControls.Controls
{
    internal partial class StructuralTextVisualHost : FrameworkElement
    {
        private Size _maxSize;
        private readonly Control _parentControl;
        private DrawingVisual _backgroundLayer;
        private DrawingVisual _errorsLayer;
        private DrawingVisual _controlLayer;
        private readonly List<ContainerDrawing> _linesLayer = new List<ContainerDrawing>();
        private readonly List<ContainerDrawing> _graphicLayer = new List<ContainerDrawing>();
        private readonly List<ContainerVisual> _uiLayer = new List<ContainerVisual>();
        private HitBox _hitObject;
        private DispatcherTimer _toolTipTimer;
        private ToolTip _hitToolTip;

        private bool _caretVisible;
        private bool _controlLayerVisible;

        private double _captureCaretXOffset;
        private StructuralCaretPoint _caretPoint;
        private StructuralCaretPoint _selectionCapture;

        private TypefaceInfo _typefaceInfo;
        private StructuralLayout _layout;

        private bool _spellCheck;
        
        private double _viewWidth;
        private double _viewHeight;

        public Action OnRenderCallBack;
        public Action OnDrawCallBack;
        public Action OnSelectionChangeCallBack;
        public Func<int, bool> CharIsError;
        public Action<object> OnHitObjectMouseLeftButtonDownCallBack;
        public Action<object> OnHitObjectMouseRightButtonDownCallBack;


        public StructuralTextVisualHost(Control parentControl)
        {
            _parentControl = parentControl;
        }

        public StructuralLayout Layout => _layout;

        public double ViewWidth => _viewWidth;

        public double ViewHeight => _viewHeight;


        public StructuralCaretPoint CaretPoint => _caretPoint;

        public int CaretCharOffset => _caretPoint.GlobalCharOffset;

        public int SelectionStart => Math.Min(_selectionCapture.GlobalCharOffset, _caretPoint.GlobalCharOffset);

        public int SelectionEnd => Math.Max(_selectionCapture.GlobalCharOffset, _caretPoint.GlobalCharOffset);

        public int SelectionLength => Math.Abs(_selectionCapture.GlobalCharOffset - _caretPoint.GlobalCharOffset);

        public bool SelectionAny => _selectionCapture.GlobalCharOffset != _caretPoint.GlobalCharOffset;


        protected override int VisualChildrenCount => 3 + _graphicLayer.Count + _linesLayer.Count + _uiLayer.Count;

        protected override Visual GetVisualChild(int index)
        {
            if (index == 0)
            {
                return _backgroundLayer;
            }

            index -= 1;
            if (index < _graphicLayer.Count)
            {
                return _graphicLayer[index].Visual;
            }

            index -= _graphicLayer.Count;
            if (index == 0)
            {
                return _errorsLayer;
            }

            index -= 1;
            if (index == 0)
            {
                return _controlLayer;
            }

            index -= 1;
            if (index < _linesLayer.Count)
            {
                return _linesLayer[index].Visual;
            }

            index -= _linesLayer.Count;
            if (index < _uiLayer.Count)
            {
                return _uiLayer[index];
            }

            return null;
        }

    }
}