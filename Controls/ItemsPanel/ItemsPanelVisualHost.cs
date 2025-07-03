using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;

namespace OpenFontWPFControls.Controls
{
    internal class ItemsPanelVisualHost : FrameworkElement
    {
        private Size _renderSize;
        private Size _arrangeSize;
        private double _hideTopSize;
        private double _firstView;
        private double _viewCount;
        private int _integerViewCount;
        private int _firstContextIndex;
        private int _containersCount;
        private object _firstContext;

        private bool _contentStretch;
        private bool _inverse;
        public bool TryScrollTopItemAfterChanged = true;
        public bool NotScrollIfTopItemFirst;

        private DataTemplate _itemTemplate;
        private CollectionManager _collectionManager;
        private IScrollValueContainer _scrollValueContainer;

        private (RenderTypes RenderType, double PixelOffset) _nextRenderOptions;
        private readonly SemaphoreSlim _renderSem = new SemaphoreSlim(1, 1);

        public Action RenderCallBack;

        private PanelVisualContainer[] _containers = Array.Empty<PanelVisualContainer>();


        protected override int VisualChildrenCount => _containersCount;

        protected override Visual GetVisualChild(int index) => _containers[index].Visual;


        private double FirstViewValue
        {
            get => _scrollValueContainer?.Value ?? _firstView;
            set
            {
                if (_scrollValueContainer != null)
                {
                    _scrollValueContainer.Value = value;
                }
                else
                {
                    _firstView = value;
                }
            }
        }

        public double FirstView
        {
            get => FirstViewValue;
            set
            {
                if (Math.Abs(FirstViewValue - value) > double.Epsilon)
                {
                    FirstViewValue = value;
                    _nextRenderOptions = new ValueTuple<RenderTypes, double>(RenderTypes.Down, 0);
                    Invalidate();
                }
            }
        }

        public double ViewCount => _viewCount;

        public int IntegerViewCount => _integerViewCount;

        public bool ContentStretch
        {
            get => _contentStretch;
            set => DefaultPropertySetter(ref _contentStretch, value);
        }

        public bool Inverse
        {
            get => _inverse;
            set => DefaultPropertySetter(ref _inverse, value);
        }

        public DataTemplate ItemTemplate
        {
            get => _itemTemplate;
            set
            {
                if (_itemTemplate != value)
                {
                    _itemTemplate = value;
                    Array.ForEach(_containers, container => container.Dispose());
                    Array.Clear(_containers, 0, _containers.Length);
                    Invalidate();
                }
            }
        }

        public double SourceCount => _collectionManager?.Count ?? 0;

        public object ItemsSource
        {
            get => _collectionManager?.Source;
            set
            {
                if (_collectionManager == null || _collectionManager.Source != value)
                {
                    _collectionManager?.Dispose();
                    _collectionManager = new CollectionManager(value, UpdateWithContextFreeze);
                    _scrollValueContainer = _collectionManager.Source as IScrollValueContainer;
                    UpdateWithContextFreeze();
                }
            }
        }

        private void DefaultPropertySetter<T>(ref T target, T value)
        {
            if (!EqualityComparer<T>.Default.Equals(target, value))
            {
                target = value;
                Invalidate();
            }
        }

        public bool SourceGetIndex(object item, out int index)
        {
            index = -1;
            if (_collectionManager is CollectionManager collection && collection.Count > 0)
            {
                index = collection.IndexOf(item);
                return index >= 0;
            }
            return false;
        }

        public bool InView(object item, bool full = false)
        {
            if (SourceGetIndex(item, out int index))
            {
                if (full)
                {
                    int first = (int)Math.Ceiling(FirstViewValue);
                    return index >= first && index <= first + _integerViewCount - 1;
                }
                return index >= _firstContextIndex && index <= _firstContextIndex + _containersCount - 1;
            }
            return false;
        }

        public void IntoView(object item)
        {
            if (SourceGetIndex(item, out int index))
            {
                IntoView(index);
            }
        }

        public void IntoView(int index)
        {
            int first = (int)Math.Ceiling(FirstViewValue);
            if (index < first)
            {
                FirstView = index;
            }
            else if (index > first + _integerViewCount - 1)
            {
                FirstView = index;
                //_nextRenderOptions = new ValueTuple<RenderTypes, double>(RenderTypes.Up, 0);
                //FirstView = index + 1;
            }
        }

        public void ScrollByPixel(double offset)
        {
            _nextRenderOptions = new ValueTuple<RenderTypes, double>(RenderTypes.Pixel, offset);
            Invalidate();
        }

        public void Invalidate()
        {
            if (Dispatcher.CheckAccess())
            {
                InternalInvalidate();
            }
            else
            {
                Dispatcher.Invoke(InternalInvalidate);
            }
        }

        private void UpdateWithContextFreeze()
        {
            if (TryScrollTopItemAfterChanged)
            {
                _nextRenderOptions = new ValueTuple<RenderTypes, double>(RenderTypes.FreezeContext, 0);
            }
            Invalidate();
        }

        private void InternalInvalidate()
        {
            InvalidateMeasure();
            InvalidateVisual();
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            _renderSem.Wait();
            try
            {
                _renderSize = new Size(Math.Min(double.MaxValue, availableSize.Width), Math.Min(double.MaxValue, availableSize.Height));
                Render();
                return _renderSize;
            }
            finally
            {
                _renderSem.Release();
            }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            Array.ForEach(_containers, c => c.Arrange());
            return finalSize;
        }


        private void Render()
        {
            RenderTypes renderType = _nextRenderOptions.RenderType;
            double pixelOffset = 0 - _hideTopSize + _nextRenderOptions.PixelOffset;
            double firstView = FirstViewValue;
            double renderSize = _renderSize.Height;
            Size renderSizeItem = new Size(_renderSize.Width, double.PositiveInfinity);
            object lastFirstContext = _firstContext;
            _arrangeSize = new Size();
            _hideTopSize = 0;
            _viewCount = 0;
            _integerViewCount = 0;
            _firstContextIndex = (int)Math.Floor(firstView);
            _containersCount = 0;
            _firstContext = null;

            Array.ForEach(_containers, c => c.Placed = false);
            if (_collectionManager is CollectionManager collection && collection.Count > 0 && _renderSize.Width > 0 && _renderSize.Height > 0)
            {
                if (renderType == RenderTypes.FreezeContext && (!NotScrollIfTopItemFirst || firstView != 0) && 
                    collection.IndexOf(lastFirstContext) is int nowContextIndex && nowContextIndex >= 0)
                {
                    firstView = nowContextIndex + firstView % 1;
                    _firstContextIndex = nowContextIndex;
                }

                if (_firstContextIndex < 0 || _firstContextIndex >= collection.Count)
                {
                    _firstContextIndex = _firstContextIndex < 0 ? 0 : collection.Count - 1;
                    firstView = _firstContextIndex;
                }

                bool setOffsetUp = false;
                double limitSize = renderSize;
                int contextIndex = _firstContextIndex;
                PanelVisualContainer panelVisualContainer;

                if (renderType == RenderTypes.Pixel)
                {
                    // Search start index
                    if (pixelOffset > 0)
                    {
                        contextIndex--;
                        while (contextIndex >= 0)
                        {
                            panelVisualContainer = PlaceContainer(contextIndex, collection.TryGetItem(contextIndex), renderSizeItem);
                            pixelOffset -= panelVisualContainer.ArrangeSize.Height;
                            if (pixelOffset < 0)
                            {
                                firstView = contextIndex + Math.Abs(pixelOffset) / panelVisualContainer.ArrangeSize.Height;
                                _firstContextIndex = contextIndex;
                                break;
                            }
                            contextIndex--;
                        }
                        if (contextIndex < 0)
                        {
                            firstView = _firstContextIndex = contextIndex = 0;
                        }
                    }
                    else if (pixelOffset < 0)
                    {
                        while (contextIndex < collection.Count)
                        {
                            panelVisualContainer = PlaceContainer(contextIndex, collection.TryGetItem(contextIndex), renderSizeItem);
                            pixelOffset += panelVisualContainer.ArrangeSize.Height;
                            if (pixelOffset > 0)
                            {
                                firstView = contextIndex + Math.Abs(pixelOffset - panelVisualContainer.ArrangeSize.Height) / panelVisualContainer.ArrangeSize.Height;
                                _firstContextIndex = contextIndex;
                                break;
                            }
                            contextIndex++;
                        }
                        if (contextIndex >= collection.Count)
                        {
                            renderType = RenderTypes.Up;
                        }
                    }
                    else
                    {
                        firstView = contextIndex = _firstContextIndex;
                    }
                    _arrangeSize = new Size();
                    Array.ForEach(_containers, c => c.Placed = false);
                }

                if (renderType != RenderTypes.Up)
                {
                    // Fill down
                    panelVisualContainer = PlaceContainer(contextIndex, collection.TryGetItem(contextIndex), renderSizeItem);
                    _containersCount++;
                    contextIndex++;
                    _hideTopSize = panelVisualContainer.ArrangeSize.Height * (firstView - _firstContextIndex);
                    limitSize += _hideTopSize;
                    while (contextIndex < collection.Count && _arrangeSize.Height < limitSize)
                    {
                        PlaceContainer(contextIndex, collection.TryGetItem(contextIndex), renderSizeItem);
                        _containersCount++;
                        contextIndex++;
                    }
                }

                // Fill up or fill empty space
                if (_arrangeSize.Height < limitSize)
                {
                    firstView = 0;
                    limitSize = renderSize;
                    contextIndex = _firstContextIndex - 1;
                    while (contextIndex >= 0 && _arrangeSize.Height < limitSize)
                    {
                        _firstContextIndex = contextIndex;
                        PlaceContainer(contextIndex, collection.TryGetItem(contextIndex), renderSizeItem);
                        _containersCount++;
                        contextIndex--;
                    }
                    setOffsetUp = !(_arrangeSize.Height < limitSize);
                }

                // Set offset
                if (_containersCount > 0)
                {
                    int placeCount = _containersCount;
                    if (setOffsetUp)
                    {
                        // Up
                        double y = renderSize;
                        contextIndex = _firstContextIndex + _containersCount - 1;
                        PlaceCacheMeasure(contextIndex + 1, collection, y, false, renderSizeItem);
                        while (placeCount > 0)
                        {
                            panelVisualContainer = _containers.First(c => c.Placed && c.ContextIndex == contextIndex);
                            y -= panelVisualContainer.ArrangeSize.Height;
                            SetOffset(panelVisualContainer, y);
                            _viewCount++;
                            if (y < 0)
                            {
                                _firstContextIndex = contextIndex;
                                _hideTopSize = Math.Abs(y) / panelVisualContainer.ArrangeSize.Height;
                                _viewCount -= _hideTopSize;
                                firstView = _firstContextIndex + _hideTopSize;
                            }
                            else
                            {
                                _integerViewCount++;
                            }
                            contextIndex--;
                            placeCount--;
                        }
                        PlaceCacheMeasure(contextIndex - 1, collection, y, true, renderSizeItem);
                    }
                    else
                    {
                        // Down
                        double y = 0;
                        contextIndex = _firstContextIndex;
                        PlaceCacheMeasure(contextIndex - 1, collection, y, true, renderSizeItem);
                        panelVisualContainer = _containers.First(c => c.Placed && c.ContextIndex == contextIndex);
                        y -= panelVisualContainer.ArrangeSize.Height * (firstView - _firstContextIndex);
                        SetOffset(panelVisualContainer, y);
                        y += panelVisualContainer.ArrangeSize.Height;
                        _viewCount = 1 - (firstView - _firstContextIndex) - (y > renderSize ? (y - renderSize) / panelVisualContainer.ArrangeSize.Height : 0);
                        _integerViewCount = _firstContextIndex < firstView ? 0 : 1;
                        contextIndex++;
                        placeCount--;
                        while (placeCount > 0)
                        {
                            panelVisualContainer = _containers.First(c => c.Placed && c.ContextIndex == contextIndex);
                            SetOffset(panelVisualContainer, y);
                            y += panelVisualContainer.ArrangeSize.Height;
                            _viewCount++;
                            if (y > renderSize)
                            {
                                _viewCount += (renderSize - y + panelVisualContainer.ArrangeSize.Height) / panelVisualContainer.ArrangeSize.Height - 1;
                            }
                            else
                            {
                                _integerViewCount++;
                            }
                            contextIndex++;
                            placeCount--;
                        }
                        PlaceCacheMeasure(contextIndex + 1, collection, y, false, renderSizeItem);
                    }
                }

                _firstContext = collection.TryGetItem(_firstContextIndex);
            }

            FirstViewValue = firstView;

            ContainersClearOverflow();
            _nextRenderOptions = new ValueTuple<RenderTypes, double>();
            RenderCallBack?.Invoke();
        }

        private void ContainersClearOverflow()
        {
            Array.Sort(_containers, (a, b) => a.Placed == b.Placed ? 0 : a.Placed ? -1 : 1);
            for (int i = _containersCount; i < _containers.Length; i++)
            {
                RemoveVisualChild(_containers[i].Visual);
                RemoveLogicalChild(_containers[i].Control);
                _containers[i].SetContext(null, -1);
            }
        }

        private PanelVisualContainer PlaceContainer(int contextIndex, object context, Size renderSize)
        {
            // Place in visual
            PanelVisualContainer panelVisualContainer = _containers.FirstOrDefault(c => !c.Placed && c.Context == context);
            if (panelVisualContainer == null)
            {
                panelVisualContainer = _containers.FirstOrDefault(c => c.ContextIndex == -1);
                if (panelVisualContainer == null)
                {
                    panelVisualContainer = new PanelVisualContainer(ItemTemplate?.LoadContent() as FrameworkElement);
                    Array.Resize(ref _containers, _containers.Length + 1);
                    _containers[_containers.Length - 1] = panelVisualContainer;
                }
                AddVisualChild(panelVisualContainer.Visual);
                AddLogicalChild(panelVisualContainer.Control);
            }
            panelVisualContainer.Placed = true;

            // Set context
            panelVisualContainer.SetContext(context, contextIndex);
            
            // Update size 
            panelVisualContainer.UpdateArrangeSize(renderSize, _contentStretch);
            _arrangeSize.Width = Math.Max(_arrangeSize.Width, panelVisualContainer.ArrangeSize.Width);
            _arrangeSize.Height += panelVisualContainer.ArrangeSize.Height;

            return panelVisualContainer;
        }

        private void PlaceCacheMeasure(int contextIndex, CollectionManager collection, double offset, bool decrementHeight, Size renderSize)
        {
            if (contextIndex >= 0 && contextIndex < collection.Count && collection.TryGetItem(contextIndex) is object context)
            {
                PanelVisualContainer panelVisualContainer = PlaceContainer(contextIndex, context, renderSize);
                offset -= decrementHeight ? panelVisualContainer.ArrangeSize.Height : 0;
                _containersCount++;
                SetOffset(panelVisualContainer, offset);
            }
        }

        private void SetOffset(PanelVisualContainer panelVisualContainer, double offset)
        {
            panelVisualContainer.Visual.Offset = new Vector(0, _inverse ? _renderSize.Height - panelVisualContainer.ArrangeSize.Height - offset : offset);
            
            // update view bounds
            foreach (BaseTextControl box in panelVisualContainer.BoundedControls)
            {
                box.DrawingBounds = GetBounds(box);
            }

            return;

            Rect GetBounds(Visual box)
            {
                if (box.TransformToVisual(this) is MatrixTransform transform)
                {
                    if (transform.Value.OffsetY <= 0)
                    {
                        return new Rect(0, transform.Value.OffsetY * -1, _renderSize.Width, _renderSize.Height);
                    }
                    if (transform.Value.OffsetY < _renderSize.Height)
                    {
                        return new Rect(0, 0, _renderSize.Width, _renderSize.Height - transform.Value.OffsetY);
                    }
                }
                return new Rect();
            }
        }

        private enum RenderTypes : byte
        {
            Down,
            Up,
            Pixel,
            FreezeContext,
        }
    }
}