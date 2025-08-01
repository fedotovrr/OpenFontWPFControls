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
            List<GlyphPoint[]> lines = new List<GlyphPoint[]>();
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

            GlyphPoint[] result = new GlyphPoint[lines.Sum(x => x.Length)];
            int offset = 0;
            for (int i = 0; i < lines.Count; i++)
            {
                lines[i].CopyTo(result, offset);
                offset += lines[i].Length;
            }

            return result;

            void AddLine()
            {
                GlyphPoint[] layout = GetGlyphLayout(info, chars.GetSubBuffer(startIndex, charsCount));
                foreach (GlyphPoint glyph in layout)
                {
                    glyph.CharOffset += startIndex;
                }
                lines.Add(layout);
            }
        }

        public static GlyphPoint[] GetGlyphLayout(this TypefaceInfo info, CharacterBuffer chars, bool hideEmptyGlyph = false)
        {
            GlyphLayout data = info.DefaultBuilder.Build(chars);
            int index = data.Count - 1;
            while (index >= 0)
            {
                if (chars[data[index].CharOffset] == '\t')
                {
                    // replace tabs with space width
                    data.Replace(index, new GlyphPoint
                    (
                        glyphIndex: info.DefaultBuilder.TabGlyph.GlyphIndex,
                        charOffset: data[index].CharOffset,
                        width: info.DefaultBuilder.TabGlyph.Width,
                        glyphOffsetX: info.DefaultBuilder.TabGlyph.GlyphOffsetX,
                        glyphOffsetY: info.DefaultBuilder.TabGlyph.GlyphOffsetY,
                        glyphLayoutBuilder: info.DefaultBuilder
                    ));
                }
                else if (data[index].GlyphIndex == 0)
                {
                    // try replace empty glyph with extension
                    if (info.ExtensionBuilder != null)
                    {
                        int length = 1;
                        index--;
                        while (index >= 0)
                        {
                            if (data[index].GlyphIndex != 0 && chars[data[index].CharOffset] != TypefaceInfo.ZwjChar)
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
                                data[index].CharOffset,
                                (index + length >= data.Count ? chars.Length : data[index + length].CharOffset) - data[index].CharOffset));
                        foreach (GlyphPoint glyph in extensionGlyphs)
                        {
                            glyph.CharOffset += data[index].CharOffset;
                            glyph.GlyphLayoutBuilder = info.ExtensionBuilder;
                        }
                        data.Replace(index, length, extensionGlyphs.ToArray());
                        //data.GlyphPoints[index] = extensionGlyphs.GlyphPoints[0];
                        //data.GlyphPoints.RemoveRange(index + 1, length - 1);
                        //data.GlyphPoints.InsertRange(index + 1, extensionGlyphs.GlyphPoints.Skip(1));
                    }
                }
                else
                {
                    // set builder for other glyph
                    data[index].GlyphLayoutBuilder = info.DefaultBuilder;
                }
                index--;
            }

            if (hideEmptyGlyph)
            {
                // hide empty glyph and zwj
                index = data.Count - 1;
                int count = 0;
                while (index >= 0)
                {
                    if ((data[index].GlyphIndex == 0 || data[index].GlyphIndex == data[index].GlyphLayoutBuilder.ZwjIndex) && 
                        chars[data[index].CharOffset] != '\t')
                    {
                        count++;
                    }
                    else if (count > 0)
                    {
                        data.RemoveRange(index + 1, count);
                        count = 0;
                    }

                    index--;
                }

                if (count > 0)
                {
                    data.RemoveRange(0, count);
                }
            }
            else
            {
                // replace empty glyph and zwj
                index = data.Count - 1;
                while (index >= 0)
                {
                    if ((data[index].GlyphIndex == 0 || data[index].GlyphIndex == data[index].GlyphLayoutBuilder.ZwjIndex) &&
                        chars[data[index].CharOffset] != '\t')
                    {
                        data.Replace(index, new GlyphPoint
                        (
                            glyphIndex: info.DefaultBuilder.EmptyGlyph.GlyphIndex,
                            charOffset: data[index].CharOffset,
                            width: info.DefaultBuilder.EmptyGlyph.Width,
                            glyphOffsetX: info.DefaultBuilder.EmptyGlyph.GlyphOffsetX,
                            glyphOffsetY: info.DefaultBuilder.EmptyGlyph.GlyphOffsetY,
                            glyphLayoutBuilder: info.DefaultBuilder
                        ));
                    }

                    index--;
                }
            }

            return data.ToArray();
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