using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using OpenFontWPFControls.Layout;

namespace OpenFontWPFControls.Controls
{
    internal partial class LargeTextVisualHost : FrameworkElement
    {
        private Size _maxSize;
        private readonly Control _parentControl;
        private readonly DependencyProperty _textProperty;
        private DrawingVisual _backgroundLayer;
        private DrawingVisual _errorsLayer;
        private DrawingVisual _controlLayer;
        private readonly ContainerVisual _caret = new ContainerVisual() { Children = { new Caret() } };
        private readonly List<ContainerDrawing> _linesLayer = new List<ContainerDrawing>();

        private bool _caretVisible;
        private bool _controlLayerVisible;

        private double _captureCaretXOffset;
        private CaretPoint _caretPoint;
        private CaretPoint _selectionCapture;

        private TypefaceInfo _typefaceInfo;
        private LargeTextLayout _layout;

        private bool _spellCheck;

        private int _startLineIndex;
        private int _startParagraphIndex;
        private int _startCharOffset;
        private int _nextLineCharOffset;

        private double _drawMaxWidth;
        private double _viewWidth;
        private double _viewHeight;
        private double _viewXOffset;

        public Action OnRenderCallBack;
        public Action OnDrawCallBack;
        public Action OnXOffsetChangeCallBack;
        public Action OnSelectionChangeCallBack;
        public Func<int, bool> CharIsError;


        public LargeTextVisualHost(Control parentControl, DependencyProperty textProperty)
        {
            _parentControl = parentControl;
            _textProperty = textProperty;
            AddVisualChild(_caret);
            AddLogicalChild(_caret);
        }

        public LargeTextLayout Layout => _layout;

        public double DrawMaxWidth => _drawMaxWidth;

        public double ViewWidth => _viewWidth;

        public double ViewHeight => _viewHeight;

        public double ViewXOffset => _viewXOffset;


        public int StartCharOffset => _startCharOffset;

        public int NextLineCharOffset => _nextLineCharOffset;

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
                3 => _caret,
                _ => _linesLayer[index - 4].Visual
            };
        }

    }
}