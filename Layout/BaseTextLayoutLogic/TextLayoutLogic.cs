using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace OpenFontWPFControls.Layout
{
    public static class TextLayoutLogic
    {
        public static IEnumerable<ParagraphInfo> GetParagraphs(this string text, int offset)
        {
            int start = offset;
            int count = 0;
            for (int i = offset; i < text.Length; i++)
            {
                switch (text[i])
                {
                    case '\n':
                        yield return new ParagraphInfo(start, count);
                        start = i + 1;
                        count = 0;
                        break;
                    case '\r':
                        count++;
                        break;
                    default:
                        count++;
                        break;
                }
            }
            yield return new ParagraphInfo(start, count);
        }

        public static GlyphPoint[] GetGlyphPoints(this TypefaceInfo info, CharacterBuffer chars)
        {
            List<GlyphLayout> lines = new List<GlyphLayout>();
            int startIndex = 0;
            int charsCount = 0;
            for (int i = 0; i < chars.Length; i++)
            {
                charsCount++;
                if (chars[i] == '\n')
                {
                    AddLine();
                    startIndex = i + 1;
                    charsCount = 0;
                }
            }
            if (charsCount > 0)
            {
                AddLine();
            }

            GlyphPoint[] result = new GlyphPoint[lines.Sum(x => x.GlyphPoints.Count)];
            int offset = 0;
            for (int i = 0; i < lines.Count; i++)
            {
                lines[i].GlyphPoints.CopyTo(result, offset);
                offset += lines[i].GlyphPoints.Count;
            }

            return result;

            void AddLine()
            {
                GlyphLayout layout = GetGlyphLayout(info, chars.GetSubBuffer(startIndex, charsCount));
                layout.GlyphPoints.ForEach(x => x.CharOffset += startIndex);
                lines.Add(layout);
            }
        }

        public static GlyphLayout GetGlyphLayout(this TypefaceInfo info, CharacterBuffer chars)
        {
            GlyphLayout data = info.DefaultBuilder.Build(chars);
            int index = data.GlyphPoints.Count - 1;
            while (index >= 0)
            {
                if (data.GlyphPoints[index].GlyphIndex == 0)
                {
                    // try replace empty glyph with extension
                    if (info.ExtensionBuilder != null)
                    {
                        int length = 1;
                        index--;
                        while (index >= 0)
                        {
                            if (data.GlyphPoints[index].GlyphIndex != 0 && chars[data.GlyphPoints[index].CharOffset] != TypefaceInfo.ZwjChar)
                            {
                                index++;
                                break;
                            }
                            length++;
                            index--;
                        }
                        index = index < 0 ? 0 : index;
                        GlyphLayout extensionGlyphs = info.ExtensionBuilder.Build(
                            chars.GetSubBuffer(
                                data.GlyphPoints[index].CharOffset,
                                (index + length >= data.GlyphPoints.Count ? chars.Length : data.GlyphPoints[index + length].CharOffset) - data.GlyphPoints[index].CharOffset));
                        foreach (GlyphPoint glyph in extensionGlyphs.GlyphPoints)
                        {
                            glyph.CharOffset += data.GlyphPoints[index].CharOffset;
                            glyph.GlyphLayoutBuilder = info.ExtensionBuilder;
                        }
                        data.GlyphPoints[index] = extensionGlyphs.GlyphPoints[0];
                        data.GlyphPoints.RemoveRange(index + 1, length - 1);
                        data.GlyphPoints.InsertRange(index + 1, extensionGlyphs.GlyphPoints.Skip(1));
                    }
                }
                else if (chars[data.GlyphPoints[index].CharOffset] == '\t')
                {
                    // replace tabs with space width
                    data.GlyphPoints[index] = new GlyphPoint
                    (
                        glyphIndex: info.DefaultBuilder.SpaceGlyph.GlyphIndex,
                        charOffset: data.GlyphPoints[index].CharOffset,
                        width: info.DefaultBuilder.SpaceGlyph.Width,
                        glyphOffsetX: info.DefaultBuilder.SpaceGlyph.GlyphOffsetX,
                        glyphOffsetY: info.DefaultBuilder.SpaceGlyph.GlyphOffsetY,
                        glyphLayoutBuilder: info.DefaultBuilder
                    );
                }
                else
                {
                    // set builder for other glyph
                    data.GlyphPoints[index].GlyphLayoutBuilder = info.DefaultBuilder;
                }
                index--;
            }

            // hide empty glyph and zwj
            index = data.GlyphPoints.Count - 1;
            int count = 0;
            while (index >= 0)
            {
                if (data.GlyphPoints[index].GlyphIndex == 0 ||
                    data.GlyphPoints[index].GlyphIndex == data.GlyphPoints[index].GlyphLayoutBuilder.ZwjIndex)
                {
                    count++;
                }
                else if (count > 0)
                {
                    data.GlyphPoints.RemoveRange(index + 1, count);
                    count = 0;
                }
                index--;
            }
            if (count > 0)
            {
                data.GlyphPoints.RemoveRange(0, count);
            }
            return data;
        }

        public static IEnumerable<LineInfo> GetLines(IList<GlyphPoint> glyphs, StringCharacterBuffer text, TextTrimming trimming, float maxWidth, float fontSize)
        {
            LineInfo line = new LineInfo();
            switch (trimming)
            {
                case TextTrimming.None:
                    {
                        line.GlyphsCount = glyphs.Count;
                        line.CharsCount = text.Length - line.CharOffset;
                    }
                    break;

                case TextTrimming.CharacterEllipsis:
                    {
                        float x = 0;
                        float width;
                        GlyphPoint glyph;
                        for (int i = 0; i < glyphs.Count; i++)
                        {
                            glyph = glyphs[i];
                            width = glyph.GetPixelWidth(fontSize);
                            if (x + width > maxWidth)
                            {
                                line.GlyphsCount = i - line.GlyphOffset;
                                line.CharsCount = glyph.CharOffset - line.CharOffset;
                                yield return line;
                                line = new LineInfo(glyphOffset: i, charOffset: glyph.CharOffset, glyphsCount: 1);
                                x = 0;
                            }

                            x += width;
                        }
                        line.GlyphsCount = glyphs.Count - line.GlyphOffset;
                        line.CharsCount = text.Length - line.CharOffset;
                    }
                    break;

                case TextTrimming.WordEllipsis:
                    {
                        int lastSpace = -1;
                        float afterSpace = 0;
                        float x = 0;
                        float width;
                        GlyphPoint glyph;
                        for (int i = 0; i < glyphs.Count; i++)
                        {
                            line.GlyphsCount++;
                            glyph = glyphs[i];
                            width = glyph.GetPixelWidth(fontSize);
                            x += width;
                            afterSpace += width;
                            if (char.IsWhiteSpace(text[glyph.CharOffset]))
                            {
                                lastSpace = i;
                                afterSpace = 0;
                            }
                            else if (x > maxWidth)
                            {
                                lastSpace++;
                                if (lastSpace - line.GlyphOffset > 0)
                                {
                                    glyph = glyphs[lastSpace];
                                    line.GlyphsCount = lastSpace - line.GlyphOffset;
                                    line.CharsCount = glyph.CharOffset - line.CharOffset;
                                    yield return line;
                                    line = new LineInfo(glyphOffset: lastSpace, charOffset: glyph.CharOffset, glyphsCount: i - lastSpace);
                                    x = afterSpace; 
                                    afterSpace = 0;
                                }
                                else
                                {
                                    line.GlyphsCount = i - line.GlyphOffset;
                                    line.CharsCount = glyph.CharOffset - line.CharOffset;
                                    yield return line;
                                    line = new LineInfo(glyphOffset: i, charOffset: glyph.CharOffset, glyphsCount: 1);
                                    x = afterSpace = width;
                                }
                                lastSpace = -1;
                            }
                        }
                        line.GlyphsCount = glyphs.Count - line.GlyphOffset;
                        line.CharsCount = text.Length - line.CharOffset;
                    }
                    break;
            }
            
            yield return line;
        }

    }
}