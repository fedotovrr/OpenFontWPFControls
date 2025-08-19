using OpenFontWPFControls.FormattingStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace OpenFontWPFControls.Layout
{
    public class StructuralLayout
    {
        private IContainersCollection _structure;
        private TextTrimming _trimming;
        private float _maxWidth;
        private float _width;
        private float _height;
        private float _fontSize;
        private float _pixelsPerDip;
        private float _pixelsPerInch;
        private Brush _foreground;
        private bool _underline;
        private bool _strike;

        private readonly TypefaceInfo _typefaceInfo;
        private List<StructuralContainer> _containers;
        private AreasTable _linesTable;
        private AreasTable _bordersTable;
        private AreasTable _hitBoxesTable;

        public StructuralLayout(
            IContainersCollection structure = null,
            TypefaceInfo typefaceInfo = null,
            TextTrimming trimming = TextTrimming.None,
            float maxWidth = float.MaxValue,
            float fontSize = 14,
            float pixelsPerDip = 1,
            float pixelsPerInch = 96,
            Brush foreground = null,
            bool underline = false,
            bool strike = false)
        {
            _typefaceInfo = typefaceInfo ?? new TypefaceInfo();
            _trimming = trimming;
            _maxWidth = maxWidth;
            _fontSize = fontSize;
            _pixelsPerDip = pixelsPerDip;
            _pixelsPerInch = pixelsPerInch;
            _structure = structure ?? new DefaultFormattingStructure();
            _foreground = foreground ?? Brushes.Black;
            _underline = underline;
            _strike = strike;

            UpdateContainers();
        }

        public TypefaceInfo TypefaceInfo => _typefaceInfo;

        public IContainersCollection Structure
        {
            get => _structure;
            set => PropertySetter(ref _structure, value);
        }

        public TextTrimming TextTrimming
        {
            get => _trimming;
            set => PropertySetter(ref _trimming, value);
        }

        public float MaxWidth
        {
            get => _maxWidth;
            set => PropertySetter(ref _maxWidth, value);
        }

        public float FontSize
        {
            get => _fontSize;
            set => PropertySetter(ref _fontSize, value);
        }

        public float PixelsPerDip
        {
            get => _pixelsPerDip;
            set => PropertySetter(ref _pixelsPerDip, value);
        }

        public float PixelsPerInch
        {
            get => _pixelsPerInch;
            set => PropertySetter(ref _pixelsPerInch, value);
        }

        public Brush Foreground
        {
            get => _foreground;
            set => PropertySetter(ref _foreground, value);
        }

        public bool Underline
        {
            get => _underline;
            set => PropertySetter(ref _underline, value);
        }

        public bool Strike
        {
            get => _strike;
            set => PropertySetter(ref _strike, value);
        }

        public float Width => _width;

        public float Height => _height;

        private void PropertySetter<T>(ref T field, T value, Action callback = null)
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                UpdateContainers();
                callback?.Invoke();
            }
        }


        // Containers

        public StructuralContainer this[int index] => _containers[index];

        public int ContainersCount => _containers.Count;

        public void UpdateContainers()
        {
            _containers = MaxWidth > 0 ? _structure.GetContainers(this, out _width, out _height).ToList() : new List<StructuralContainer>();
            int globalOffset = 0;
            foreach (StructuralLine line in Lines)
            {
                line.GlobalCharOffset = globalOffset;
                globalOffset += line.GlyphPoints.Sum(info => info.CharsCount);
            }
            _linesTable = new AreasTable(Lines);
            _bordersTable = new AreasTable(Borders);
            _hitBoxesTable = new AreasTable(HitBoxes);
        }


        // Enumerators

        public IEnumerable<StructuralLine> LinesInArea(Rect bounds)
        {
            return _linesTable.GetItems<StructuralLine>(bounds);
        }

        public IEnumerable<StructuralLine> Lines
        {
            get 
            {
                List<StructuralContainer> containers = _containers;
                for (int c = 0; c < containers.Count; c++)
                {
                    StructuralContainer container = containers[c];
                    for (int l = 0; l < container.Lines.Count; l++)
                    {
                        yield return container.Lines[l];
                    }
                }
            }
        }

        public IEnumerable<StructuralBorder> BordersInArea(Rect bounds)
        {
            return _bordersTable.GetItems<StructuralBorder>(bounds);
        }

        public IEnumerable<StructuralBorder> Borders
        {
            get 
            {
                List<StructuralContainer> containers = _containers;
                for (int c = 0; c < containers.Count; c++)
                {
                    StructuralContainer container = containers[c];
                    for (int b = 0; b < container.Borders.Count; b++)
                    {
                        yield return container.Borders[b];
                    }
                }
            }
        }

        public IEnumerable<HitBox> HitBoxesInArea(Rect bounds)
        {
            return _hitBoxesTable.GetItems<HitBox>(bounds);
        }

        public IEnumerable<HitBox> HitBoxes
        {
            get
            {
                List<StructuralContainer> containers = _containers;
                for (int c = 0; c < containers.Count; c++)
                {
                    StructuralContainer container = containers[c];
                    for (int h = 0; h < container.Hyperlinks.Count; h++)
                    {
                        yield return container.Hyperlinks[h];
                    }
                }
            }
        }

        public IEnumerable<ContainerVisual> Controls
        {
            get
            {
                List<StructuralContainer> containers = _containers;
                for (int c = 0; c < containers.Count; c++)
                {
                    StructuralContainer container = containers[c];
                    for (int cv = 0; cv < container.Controls.Count; cv++)
                    {
                        yield return container.Controls[cv];
                    }
                }
            }
        }

        public IEnumerable<StructuralCaretPoint> CaretPoints => CaretPointsEnumerator(null);

        public IEnumerable<StructuralCaretPoint> CaretPointsEnumerator(Rect? bounds)
        {
            foreach (StructuralLine line in bounds == null ? Lines : LinesInArea(bounds.Value))
            {
                int globalOffset = line.GlobalCharOffset;
                int lastGlyphLength = 0;
                float x = line.XOffset;
                yield return new StructuralCaretPoint(null, CaretPointOwners.StartLine, globalOffset, 0, 0, x, line.YOffset, line.Height);
                foreach (GlyphInfo glyphInfo in line.GlyphPoints)
                {
                    yield return new StructuralCaretPoint(glyphInfo.Text, CaretPointOwners.Glyph, globalOffset, glyphInfo.Glyph.CharOffset, glyphInfo.CharsCount, x, line.YOffset, line.Height);
                    x += glyphInfo.Width;
                    globalOffset += glyphInfo.CharsCount;
                    lastGlyphLength = glyphInfo.CharsCount;
                }
                yield return new StructuralCaretPoint(null, CaretPointOwners.EndLine, globalOffset, lastGlyphLength, 0, x, line.YOffset, line.Height);
            }
        }


        // Tools



        public StructuralCaretPoint FirstCaretPoint => CaretPoints.FirstOrDefault();

        public StructuralCaretPoint LastCaretPoint => CaretPoints.LastOrDefault();
    }

}

