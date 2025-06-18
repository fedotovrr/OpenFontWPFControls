using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace OpenFontWPFControls.Layout
{
    public static class TextLayoutLogic
    {
        public static IEnumerable<TextParagraph> GetParagraphs(this string text, int offset, TextLayout layout)
        {
            int start = offset;
            int count = 0;
            for (int i = offset; i < text.Length; i++)
            {
                switch (text[i])
                {
                    case '\n':
                        yield return new TextParagraph(start, count, layout);
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
            yield return new TextParagraph(start, count, layout);
        }

        public static GlyphLayout GetGlyphLayout(this TypefaceInfo info, CharacterBuffer chars)
        {
            GlyphLayout data = info.DefaultBuilder.Build(chars);
            int index = data.GlyphPoints.Count - 1;
            while (index >= 0)
            {
                if (data.GlyphPoints[index].GlyphIndex == 0)
                {
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
                    data.GlyphPoints[index].GlyphLayoutBuilder = info.DefaultBuilder;
                }
                index--;
            }

            index = data.GlyphPoints.Count - 1;
            while (index >= 0)
            {
                if (data.GlyphPoints[index].GlyphIndex == 0 ||
                    data.GlyphPoints[index].GlyphIndex == data.GlyphPoints[index].GlyphLayoutBuilder.ZwjIndex)
                {
                    data.GlyphPoints.RemoveAt(index);
                }
                index--;
            }

            return data;
        }

        public static List<TextLine> GetLines(this TextParagraph paragraph)
        {
            List<TextLine> lines = new List<TextLine>();
            TextLine line = new TextLine(paragraph, 0);
            lines.Add(line);

            int glyphsCount = paragraph.GlyphsLayout.GlyphPoints.Count;
            float maxWidth = paragraph.TextLayout.MaxWidth;
            float fontSize = paragraph.TextLayout.FontSize;

            switch (paragraph.TextLayout.TextTrimming)
            {
                case TextTrimming.None:
                    {
                        line.GlyphCount = glyphsCount;
                        line.CharCount = paragraph.CharCount - line.CharOffset;
                    }
                    break;

                case TextTrimming.CharacterEllipsis:
                    {
                        float x = 0;
                        float width;
                        GlyphPoint glyph;
                        for (int i = 0; i < glyphsCount; i++)
                        {
                            glyph = paragraph.GlyphsLayout.GlyphPoints[i];
                            width = glyph.GetPixelWidth(fontSize);
                            if (x + width > maxWidth)
                            {
                                line.GlyphCount = i - line.GlyphOffset;
                                line.CharCount = glyph.CharOffset - line.CharOffset;
                                lines.Add(line = new TextLine(paragraph: paragraph, glyphOffset: i, charOffset: glyph.CharOffset, glyphCount: 1));
                                x = 0;
                            }

                            x += width;
                        }
                        line.GlyphCount = glyphsCount - line.GlyphOffset;
                        line.CharCount = paragraph.CharCount - line.CharOffset;
                    }
                    break;

                case TextTrimming.WordEllipsis:
                    {
                        int lastSpace = -1;
                        float afterSpace = 0;
                        float x = 0;
                        float width;
                        GlyphPoint glyph;
                        StringCharacterBuffer text = paragraph.GetBuffer();
                        for (int i = 0; i < glyphsCount; i++)
                        {
                            line.GlyphCount++;
                            glyph = paragraph.GlyphsLayout.GlyphPoints[i];
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
                                    glyph = paragraph.GlyphsLayout.GlyphPoints[lastSpace];
                                    line.GlyphCount = lastSpace - line.GlyphOffset;
                                    line.CharCount = glyph.CharOffset - line.CharOffset;
                                    lines.Add(line = new TextLine(paragraph: paragraph, glyphOffset: lastSpace, charOffset: glyph.CharOffset, glyphCount: i - lastSpace));
                                    x = afterSpace; 
                                    afterSpace = 0;
                                }
                                else
                                {
                                    line.GlyphCount = i - line.GlyphOffset;
                                    line.CharCount = glyph.CharOffset - line.CharOffset;
                                    lines.Add(line = new TextLine(paragraph: paragraph, glyphOffset: i, charOffset: glyph.CharOffset, glyphCount: 1));
                                    x = afterSpace = width;
                                }
                                lastSpace = -1;
                            }
                        }
                        line.GlyphCount = glyphsCount - line.GlyphOffset;
                        line.CharCount = paragraph.CharCount - line.CharOffset;
                    }
                    break;
            }
            
            return lines;
        }
    }
}