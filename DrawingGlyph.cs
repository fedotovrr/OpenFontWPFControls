using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using OpenFontWPFControls.Layout;
using Typeface = Typography.OpenFont.Typeface;

namespace OpenFontWPFControls
{
    public static class DrawingGlyph
    {
        public static SolidColorBrush ErrorLineBrush = new SolidColorBrush(Color.FromRgb(255, 90, 90));

        public static DrawingGroup DrawText(string text, string fontFamily = TypefaceInfo.DefaultFontFamily, FontStyle style = default, FontWeight weight = default, string fontFamilyExtension = null, float maxWidth = float.MaxValue, float fontSize = 14, Brush defaultForeground = null, float pixelsPerDip = 1, float pixelsPerInchX = 96, bool setGuidelines = true)
        {
            return TypefaceInfo.CacheGetOrTryCreate(fontFamily, style, weight, fontFamilyExtension)?.DrawText(text, maxWidth, fontSize, defaultForeground, pixelsPerDip, pixelsPerInchX, setGuidelines);
        }

        public static DrawingGroup DrawText(this TypefaceInfo typefaceInfo, string text, float maxWidth = float.MaxValue, float fontSize = 14, Brush defaultForeground = null, float pixelsPerDip = 1, float pixelsPerInchX = 96, bool setGuidelines = true)
        {
            DrawingGroup drawing = new DrawingGroup();
            using DrawingContext context = drawing.Open();
            TextLayout data = new TextLayout(text, typefaceInfo, maxWidth, fontSize, pixelsPerDip, pixelsPerInchX);
            foreach ((GlyphPoint glyph, float x) in data.GlyphPoints)
            {
                DrawGlyph(context, glyph, data.FontSize, data.PixelsPerDip, data.Foreground, data.Underline, data.Strike, false, x, 0, setGuidelines);
            }
            return drawing;
        }

        public static void DrawGlyph(this DrawingContext dc, GlyphPoint glyph, float fontSize, float pixelsPerDip, Brush defaultForeground, bool underline, bool strike, bool errorLine, float x, float y = 0, bool setGuidelines = true)
        {
            foreach ((GlyphRun gr, Brush br) in
                     GetGlyphRun(
                         glyph.GlyphLayoutBuilder.GlyphTypeface,
                         glyph.GlyphLayoutBuilder.Typeface,
                         glyph.GlyphIndex,
                         fontSize,
                         pixelsPerDip,
                         defaultForeground,
                         new Point(x + glyph.GetPixelOffsetX(fontSize), y + glyph.GetPixelBaselineOffset(fontSize))))
            {
                if (setGuidelines)
                {
                    Rect rect = gr.ComputeAlignmentBox();
                    GuidelineSet guidelines = new GuidelineSet();
                    guidelines.GuidelinesX.Add(Math.Round(rect.Left));
                    //guidelines.GuidelinesX.Add(Math.Round(rect.Right));
                    guidelines.GuidelinesY.Add(Math.Round(rect.Top));
                    //guidelines.GuidelinesY.Add(Math.Round(rect.Bottom));
                    dc.PushGuidelineSet(guidelines);
                    dc.DrawGlyphRun(br, gr);
                    dc.Pop();
                }
                else
                {
                    dc.DrawGlyphRun(br, gr);
                    //dc.DrawGeometry(br, null, gr.BuildGeometry());
                }
            }

            if (errorLine)
            {
                float height = glyph.GlyphLayoutBuilder.GetPixelUnderlineSize(fontSize) * 2;
                float offset = y + glyph.GlyphLayoutBuilder.GetPixelClipedAscender(fontSize) + glyph.GlyphLayoutBuilder.GetPixelUnderlinePosition(fontSize) - height / 2;
                DrawRect(dc, defaultForeground, x, offset, glyph.GetPixelWidth(fontSize), height);
            }

            if (underline)
            {
                float height = glyph.GlyphLayoutBuilder.GetPixelUnderlineSize(fontSize);
                float offset = y + glyph.GlyphLayoutBuilder.GetPixelClipedAscender(fontSize) - glyph.GlyphLayoutBuilder.GetPixelUnderlinePosition(fontSize) - height / 2;
                DrawRect(dc, defaultForeground, x, offset, glyph.GetPixelWidth(fontSize), height);
            }

            if (strike)
            {
                float height = glyph.GlyphLayoutBuilder.GetPixelStrikeSize(fontSize);
                float offset = y + glyph.GlyphLayoutBuilder.GetPixelClipedAscender(fontSize) - glyph.GlyphLayoutBuilder.GetPixelStrikePosition(fontSize) - height / 2;
                DrawRect(dc, defaultForeground, x, offset, glyph.GetPixelWidth(fontSize), height);
            }

            return;
            
            static void DrawRect(DrawingContext dc, Brush brush, float x, float y, float width, float height)
            {
                // todo check rounded coords
                GuidelineSet guidelines = new GuidelineSet();
                guidelines.GuidelinesY.Add(y);
                guidelines.GuidelinesX.Add(x);
                guidelines.GuidelinesX.Add(x + width);
                dc.PushGuidelineSet(guidelines);
                dc.DrawRectangle(brush, null, new Rect(x, y, width, height));
                dc.Pop();
            }
        }

        public static IEnumerable<(GlyphRun, Brush)> GetGlyphRun(GlyphTypeface geometryTypeface, Typeface colorTypeface, ushort glyphIndex, float fontSize, float pixelsPerDip, Brush defaultForeground, Point offset)
        {
            if (colorTypeface != null)
            {
                if (colorTypeface.COLRTable != null &&
                    colorTypeface.CPALTable != null &&
                    colorTypeface.COLRTable.LayerIndices.TryGetValue(glyphIndex, out ushort layerIndex))
                {
                    int start = layerIndex, stop = layerIndex + colorTypeface.COLRTable.LayerCounts[glyphIndex];
                    int palette = 0;

                    for (int i = start; i < stop; ++i)
                    {
                        ushort subGid = colorTypeface.COLRTable.GlyphLayers[i];
                        int cid = colorTypeface.CPALTable.Palettes[palette] + colorTypeface.COLRTable.GlyphPalettes[i];
                        colorTypeface.CPALTable.GetColor(cid, out byte r, out byte g, out byte b, out byte a);
                        yield return (
                            new GlyphRun(geometryTypeface, 0, false, fontSize, pixelsPerDip, new[] { subGid }, offset, new[] { 0.0 },
                                null, null, null, null, null, null),
                            new SolidColorBrush(Color.FromArgb(a, r, g, b)));
                    }
                }
                else
                {
                    yield return (
                        new GlyphRun(geometryTypeface, 0, false, fontSize, pixelsPerDip, new[] { glyphIndex }, offset, new[] { 0.0 },
                            null, null, null, null, null, null),
                        defaultForeground ?? Brushes.Black);
                }
            }
        }
    }
}
