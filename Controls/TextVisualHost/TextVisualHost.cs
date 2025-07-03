using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using OpenFontWPFControls.Layout;

namespace OpenFontWPFControls.Controls
{
    internal partial class TextVisualHost : FrameworkElement
    {
        private Size _maxSize;
        private readonly Control _parentControl;
        private readonly DependencyProperty _textProperty;
        private DrawingVisual _backgroundLayer;
        private DrawingVisual _errorsLayer;
        private DrawingVisual _controlLayer;
        private readonly ContainerUI _caret = new ContainerUI(new Caret());
        private readonly List<ContainerDrawing> _linesLayer = new List<ContainerDrawing>();

        private bool _controlLayerVisible;

        private double _captureCaretXOffset;
        private CaretPoint _caretPoint;
        private CaretPoint _selectionCapture;

        private TypefaceInfo _typefaceInfo;
        private SimpleTextLayout _layout;

        private bool _spellCheck;

        private double _drawMaxWidth;
        private double _viewWidth;
        private double _viewHeight;
        private bool _drawingValid;
        private bool _controlsValid;
        private Rect _drawingBounds;
        private Vector _drawingOffset;

        public Action OnRenderCallBack;
        public Action OnDrawCallBack;
        public Action OnSelectionChangeCallBack;
        public Func<int, bool> CharIsError;


        public TextVisualHost(Control parentControl, DependencyProperty textProperty)
        {
            Cursor = Cursors.IBeam;
            _parentControl = parentControl;
            _textProperty = textProperty;
            AddVisualChild(_caret.Visual);
            AddLogicalChild(_caret);
        }

        public SimpleTextLayout Layout => _layout;

        public double DrawMaxWidth => _drawMaxWidth;

        public double ViewWidth => _viewWidth;

        public double ViewHeight => _viewHeight;


        public CaretPoint CaretPoint => _caretPoint;

        public int CaretCharOffset => _caretPoint.CharOffset;

        public int SelectionStart => Math.Min(_selectionCapture.CharOffset, _caretPoint.CharOffset);

        public int SelectionEnd => Math.Max(_selectionCapture.CharOffset, _caretPoint.CharOffset);

        public int SelectionLength => Math.Abs(_selectionCapture.CharOffset - _caretPoint.CharOffset);

        public bool SelectionAny => _selectionCapture.CharOffset != _caretPoint.CharOffset;


        protected override int VisualChildrenCount => _linesLayer.Count + 4;

        protected override Visual GetVisualChild(int index)
        {
            return index switch
            {
                0 => _backgroundLayer,
                1 => _errorsLayer,
                2 => _controlLayer,
                3 => _caret.Visual,
                _ => _linesLayer[index - 4].Visual
            };
        }

    }
}