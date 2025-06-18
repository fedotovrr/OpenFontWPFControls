using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using OpenFontWPFControls.FormattingStructure;
using OpenFontWPFControls.Layout.FormattingStructureLayout;

namespace OpenFontWPFControls.Layout
{
    public class StructuralLine : IVisualGenerator, IPlacement
    {
        public readonly StructuralContainer Container;
        public readonly int StartTextContainer;
        public readonly int GlyphOffset;
        public readonly int GlyphsCount;
        public int GlobalCharOffset; // todo integration in logic
        public float XOffset;
        public float YOffset;
        private float? _maxFontSize;
        private float? _width;

        float IPlacement.XOffset => XOffset;

        float IPlacement.YOffset => YOffset;

        float IPlacement.Width => Width;

        float IPlacement.Height => Height;

        public StructuralLine(StructuralContainer container, int startTextContainer, int glyphOffset, int glyphsCount)
        {
            GlyphOffset = glyphOffset;
            GlyphsCount = glyphsCount;
            StartTextContainer = startTextContainer;
            Container = container ?? throw new NullReferenceException();
        }

        public float MaxFontSize => GetMaxFontSize();

        public float Height => MaxFontSize / (72f / Container.Layout.PixelsPerInch);

        public float Width => _width ?? (_width = GlyphPoints.Sum(info => info.Width)).Value;

        private float GetMaxFontSize()
        {
            if (_maxFontSize == null)
            {
                StructuralTextItem maxSize = null;
                foreach (StructuralTextItem container in TextContainers)
                {
                    maxSize ??= container;
                    if (container.FontSize > maxSize.FontSize)
                    {
                        maxSize = container;
                    }
                }
                _maxFontSize = maxSize?.FontSize ?? Container.Layout.FontSize;
            }
            return _maxFontSize.Value;
        }

        public DrawingVisual CreateDrawingVisual()
        {
            DrawingVisual visual = new DrawingVisual();
            DrawingContext context = visual.RenderOpen();
            foreach (GlyphInfo glyphInfo in GlyphPoints)
            {
                float yOffset = glyphInfo.Glyph.GetPixelBaselineOffset(MaxFontSize) - glyphInfo.Glyph.GetPixelBaselineOffset(glyphInfo.Text.FontSize);
                if (glyphInfo.Text.HitObject is IInlineImage img)
                {
                    GuidelineSet guidelines = new GuidelineSet();
                    guidelines.GuidelinesY.Add(yOffset);
                    guidelines.GuidelinesX.Add(glyphInfo.X);
                    guidelines.GuidelinesX.Add(glyphInfo.X + img.Width);
                    context.PushGuidelineSet(guidelines);
                    context.DrawImage(img.Source, new Rect(new Point(glyphInfo.X, yOffset), new Size(img.Width, img.Height)));
                    context.Pop();
                }
                else
                {
                    DrawingGlyph.DrawGlyph(
                        context,
                        glyphInfo.Glyph,
                        glyphInfo.Text.FontSize,
                        Container.Layout.PixelsPerDip,
                        glyphInfo.Text.NowColor,
                        glyphInfo.Text.Underline,
                        glyphInfo.Text.Strike,
                        false,
                        glyphInfo.X,
                        yOffset);
                }
            }
            context.Close();
            return visual;
        }


        // Enumerators

        public IEnumerable<StructuralTextItem> TextContainers
        {
            get
            {
                int startGlyph = GlyphOffset;
                int count = GlyphsCount;
                for (int containerIndex = StartTextContainer; containerIndex < Container.TextContainers.Count; containerIndex++)
                {
                    StructuralTextItem text = Container.TextContainers[containerIndex];
                    yield return text;
                    count -= text.GlyphsCount - startGlyph;
                    startGlyph = 0;
                    if (count <= 0)
                    {
                        yield break;
                    }
                }
            }
        }

        public IEnumerable<GlyphInfo> GlyphPoints
        {
            get
            {
                if (GlyphsCount > 0)
                {
                    int startGlyph = GlyphOffset;
                    int count = GlyphsCount;
                    float x = 0;
                    for (int containerIndex = StartTextContainer; containerIndex < Container.TextContainers.Count; containerIndex++)
                    {
                        StructuralTextItem text = Container.TextContainers[containerIndex];
                        for (int glyphIndex = startGlyph; count > 0 && glyphIndex < text.GlyphsCount; glyphIndex++)
                        {
                            GlyphPoint glyph = text[glyphIndex];
                            float width = glyph.GetPixelWidth(text.FontSize);
                            yield return new GlyphInfo(text, glyph, x, width, text.GetGlyphLength(glyphIndex));
                            x += width;
                            count--;
                        }
                        startGlyph = 0;
                    }
                }
            }
        }


        // Debug

        public string Text
        {
            get
            {
                string text = string.Empty;
                foreach (GlyphInfo glyphInfo in GlyphPoints)
                {
                    text += glyphInfo.Text.Chars.Substring(glyphInfo.Glyph.CharOffset, glyphInfo.CharsCount);
                }
                return text;
            }
        }

        public override string ToString() => $"GlyphOffset: {GlyphOffset:0000} GlyphCount: {GlyphsCount:0000} X: {XOffset:000.0} Y: {YOffset:000.0} Text: {Text}";
    }
}