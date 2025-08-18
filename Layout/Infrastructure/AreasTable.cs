using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace OpenFontWPFControls.Layout
{
    internal class AreasTable
    {
        private const float CellWidth = 10000;
        private const float CellHeight = 1000;
        private readonly List<IPlacement>[,] _areas;
        private readonly int _rows;
        private readonly int _cols;

        public AreasTable(IEnumerable<IPlacement> items)
        {
            if (items == null || !items.Any())
            {
                _areas = new List<IPlacement>[0, 0];
                return;
            }

            float maxX = float.MinValue;
            float maxY = float.MinValue;

            foreach (IPlacement item in items)
            {
                float itemMaxX = item.XOffset + item.Width;
                float itemMaxY = item.YOffset + item.Height;
                maxX = itemMaxX > maxX ? itemMaxX : maxX;
                maxY = itemMaxY > maxY ? itemMaxY : maxY;
            }

            _cols = Math.Max(1, (int)Math.Ceiling(maxX / CellWidth));
            _rows = Math.Max(1, (int)Math.Ceiling(maxY / CellHeight));

            _areas = new List<IPlacement>[_rows, _cols];
            foreach (IPlacement item in items)
            {
                int startColIndex = Clamp(item.XOffset / CellWidth, 0, _cols - 1);
                int startRowIndex = Clamp(item.YOffset / CellHeight, 0, _rows - 1);
                int endColIndex = Clamp((item.XOffset + item.Width) / CellWidth, 0, _cols - 1);
                int endRowIndex = Clamp((item.YOffset + item.Height) / CellHeight, 0, _rows - 1);

                for (int r = startRowIndex; r <= endRowIndex; r++)
                {
                    for (int c = startColIndex; c <= endColIndex; c++)
                    {
                        if (_areas[r, c] == null)
                        {
                            _areas[r, c] = new List<IPlacement>();
                        }
                        _areas[r, c].Add(item);
                    }
                }
            }
        }

        public IEnumerable<T> GetItems<T>(Rect bounds) where T : IPlacement
        {
            if (_areas != null && _areas.GetLength(0) > 0 && _areas.GetLength(1) > 0)
            {
                float width  = double.IsInfinity(bounds.Width) ? float.MaxValue : (float)bounds.Width;
                float height = double.IsInfinity(bounds.Height) ? float.MaxValue : (float)bounds.Height;
                float boundsMinX = (float)bounds.X;
                float boundsMinY = (float)bounds.Y;
                float boundsMaxX = boundsMinX + width;
                float boundsMaxY = boundsMinY + height;
                boundsMaxX = float.IsInfinity(boundsMaxX) ? float.MaxValue : boundsMaxX;
                boundsMaxY = float.IsInfinity(boundsMaxY) ? float.MaxValue : boundsMaxY;

                int startColIndex = Clamp(boundsMinX / CellWidth, 0, _cols - 1);
                int endColIndex   = Clamp(boundsMaxX / CellWidth, 0, _cols - 1);
                int startRowIndex = Clamp(boundsMinY / CellHeight, 0, _rows - 1);
                int endRowIndex   = Clamp(boundsMaxY / CellHeight, 0, _rows - 1);

                for (int row = startRowIndex; row <= endRowIndex; row++)
                {
                    for (int col = startColIndex; col <= endColIndex; col++)
                    {
                        if (_areas[row, col] != null)
                        {
                            foreach (var item in _areas[row, col])
                            {
                                float itemX = item.XOffset;
                                float itemY = item.YOffset;
                                float itemMaxX = itemX + item.Width;
                                float itemMaxY = itemY + item.Height;
                                if (itemX < boundsMaxX && itemMaxX >= boundsMinX && itemY < boundsMaxY && itemMaxY >= boundsMinY)
                                {
                                    yield return (T)item;
                                }
                            }
                        }
                    }
                }
            }
        }

        private static int Clamp(float value, int min, int max)
        {
            int integer = value > int.MaxValue ? int.MaxValue : (int)value;
            return integer < min ? min : integer > max ? max : integer;
        }
    }

}
