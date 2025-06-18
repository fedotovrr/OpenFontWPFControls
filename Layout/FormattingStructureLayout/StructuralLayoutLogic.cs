using OpenFontWPFControls.FormattingStructure;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System;
using System.Windows.Media;
using OpenFontWPFControls.Layout.FormattingStructureLayout;

namespace OpenFontWPFControls.Layout
{
    public static class StructuralLayoutLogic
    {
        public static List<StructuralContainer> GetContainers(this IContainersCollection structure, StructuralLayout layout, out float width, out float height)
        {
            width = 0f; 
            height = 0f;
            List<StructuralContainer> containers = CreateContainers(structure, layout, layout.MaxWidth, 0, 0, out width, out height);
            return containers;

            static List<StructuralContainer> CreateContainers(IContainersCollection structure, StructuralLayout textLayout, float maxWidth, float x, float y, out float width, out float height)
            {
                width = 0;
                height = 0;
                List<StructuralContainer> result = new List<StructuralContainer>();

                if (structure != null && maxWidth > 0)
                {
                    foreach (object o in structure.Items)
                    {
                        switch (o)
                        {
                            case IInlineCollection inlines:

                                StructuralContainer textStructuralContainer = 
                                    new StructuralContainer(inlines.GetContainersCollection(textLayout), textLayout);
                                result.Add(textStructuralContainer);

                                List<StructuralLine> lines = textStructuralContainer.GetLines(maxWidth);
                                textStructuralContainer.Lines.AddRange(lines);
                                foreach (StructuralLine line in lines)
                                {
                                    float lineHeight = line.Height;
                                    line.XOffset = x;
                                    line.YOffset = y;
                                    y += lineHeight;
                                    width = Math.Max(line.Width, width);
                                    height += lineHeight;

                                    GlyphInfo lastContainer = new GlyphInfo(null, null, 0, 0, 0);
                                    GlyphInfo lastPoint = new GlyphInfo(null, null, 0, 0, 0);
                                    foreach (GlyphInfo current in line.GlyphPoints)
                                    {
                                        if (current.Text != lastContainer.Text)
                                        {
                                            if (lastContainer.Text?.HitObject != null)
                                            {
                                                textStructuralContainer.Hyperlinks.Add(new HitBox(lastContainer.Text, lastContainer.X, y - lineHeight, current.X - lastContainer.X, lineHeight));
                                            }
                                            lastContainer = current;
                                        }
                                        lastPoint = current;
                                    }
                                    if (lastContainer.Text?.HitObject != null)
                                    {
                                        textStructuralContainer.Hyperlinks.Add(new HitBox(lastContainer.Text, lastContainer.X, y - lineHeight, lastPoint.X + lastPoint.Width - lastContainer.X, lineHeight));
                                    }
                                }

                                break;

                            case ITable table:

                                StructuralBorder tableBorder = null;
                                float tableContentX = x;
                                float tableContentY = y;
                                float maxTableContentWidth = maxWidth - StructuralBorder.GetOwnWidth(table as IBorder);
                                int columnsCount = table.Rows.Any() ? table.Rows.Max(row => row.Cells.Count()) : 0;
                                float[] columnsWidths = new float[columnsCount];
                                List<(List<List<StructuralContainer>> cells, float rowHeight)> rows = new ();

                                if (table is IBorder iTableBorder)
                                {
                                    tableBorder = new StructuralBorder(iTableBorder, x, y, 0, 0);
                                    result.Add(new StructuralContainer(null, textLayout) { Borders = { tableBorder } });
                                    tableContentX += tableBorder.ContentXOffset;
                                    tableContentY += tableBorder.ContentYOffset;
                                }

                                foreach (ITableRow row in table.Rows)
                                {
                                    List<List<StructuralContainer>> cells = new ();
                                    float rowHeight = 0;
                                    int column = 0;
                                    foreach (ITableCell cell in row.Cells)
                                    {
                                        float maxContentWidth = (cell.Width > 0 ? cell.Width : maxTableContentWidth / columnsCount) - StructuralBorder.GetOwnWidth(cell as IBorder);
                                        List<StructuralContainer> content =
                                            CreateContainers(cell as IContainersCollection, textLayout, maxContentWidth, 0, 0, out float cellWidth, out float cellHeight);
                                        cells.Add(content);

                                        if (cellWidth > maxContentWidth)
                                        {
                                            content.Clear();
                                            cellWidth = cellHeight = 0;
                                        }

                                        if (cell is IBorder iCellBorder)
                                        {
                                            StructuralBorder cellBorder = new StructuralBorder(iCellBorder, 0, 0, cellWidth, cellHeight);
                                            content.Insert(0, new StructuralContainer(null, textLayout) { Borders = { cellBorder } });
                                            cellWidth = cellBorder.Width;
                                            cellHeight = cellBorder.Height;
                                        }

                                        columnsWidths[column] = Math.Max(columnsWidths[column], cellWidth);
                                        rowHeight = Math.Max(rowHeight, cellHeight);
                                        column++;
                                    }
                                    rows.Add((cells, rowHeight));
                                }

                                float cellX;
                                float rowY = tableContentY;
                                foreach ((List<List<StructuralContainer>> cells, float rowHeight) in rows)
                                {
                                    cellX = tableContentX;
                                    int column = 0;
                                    foreach (List<StructuralContainer> content in cells)
                                    {
                                        float contentX = 0;
                                        float contentY = 0;
                                        StructuralBorder border = content.FirstOrDefault().Borders.FirstOrDefault();
                                        if (border != null)
                                        {
                                            border.Width = columnsWidths[column];
                                            border.Height = rowHeight;
                                            contentX = border.ContentXOffset;
                                            contentY = border.ContentYOffset;
                                        }
                                        contentX += cellX;
                                        contentY += rowY;

                                        foreach (StructuralLine line in content.SelectMany(item => item.Lines))
                                        {
                                            line.XOffset += contentX;
                                            line.YOffset += contentY;
                                        }
                                        foreach (ContainerVisual container in content.SelectMany(item => item.Controls))
                                        {
                                            container.Offset = new Vector(container.Offset.X + contentX, container.Offset.Y + contentY);
                                        }
                                        foreach (HitBox hitBox in content.SelectMany(item => item.Hyperlinks))
                                        {
                                            hitBox.XOffset += contentX;
                                            hitBox.YOffset += contentY;
                                        }
                                        foreach (StructuralBorder container in content.SelectMany(item => item.Borders))
                                        {
                                            if (border == container)
                                            {
                                                container.XOffset += cellX;
                                                container.YOffset += rowY;
                                            }
                                            else
                                            {
                                                container.XOffset += contentX;
                                                container.YOffset += contentY;
                                            }
                                        }

                                        cellX += columnsWidths[column];
                                        column++;
                                    }
                                    rowY += rowHeight;
                                }

                                if (tableBorder != null)
                                {
                                    tableBorder.Width = columnsWidths.Sum() + tableBorder.OwnWidth;
                                    tableBorder.Height = rows.Sum(row => row.rowHeight) + tableBorder.OwnHeight;
                                    width = Math.Max(tableBorder.Width, width);
                                    height += tableBorder.Height;
                                    y += tableBorder.Height;
                                }
                                else
                                {
                                    width = Math.Max(columnsWidths.Sum(), width);
                                    float tableHeight = rows.Sum(row => row.rowHeight);
                                    height += tableHeight;
                                    y += tableHeight;
                                }

                                result.AddRange(rows.SelectMany(row => row.cells.SelectMany(cell => cell)));

                                break;
                        }
                    }
                }

                return result;
            }
        }

        public static List<StructuralTextItem> GetContainersCollection(this IInlineCollection inlines, StructuralLayout layout)
        {
            List<StructuralTextItem> containers = new List<StructuralTextItem>();
            AddInline(inlines, StructuralTextItem.CreateDefault(layout), containers);
            return containers;

            static void AddInline(object o, StructuralTextItem parent, List<StructuralTextItem> result)
            {
                bool isDrawObj = false;
                StructuralTextItem text = parent.GetFormatCopy();
                text.SourceObject = o;

                if (o is string str)
                {
                    isDrawObj = true;
                }

                if (o is IText iText)
                {
                    isDrawObj = true;
                }

                if (o is IHyperlink hyperlink)
                {
                    isDrawObj = true;
                }

                if (o is IFontSize size)
                {
                    text.FontSize = size.FontSize;
                }

                if (o is IForeground foreground)
                {
                    text.Foreground = foreground.Foreground;
                }

                if (o is IFontWeight bold)
                {
                    text.FontWeight = bold.FontWeight;
                }

                if (o is IFontStyle italic)
                {
                    text.FontStyle = italic.FontStyle;
                }

                if (o is IUnderline underline)
                {
                    text.Underline = underline.Underline;
                }

                if (o is IStrike strike)
                {
                    text.Strike = strike.Strike;
                }

                if (o is IInlineImage img)
                {
                    isDrawObj = true;
                }

                if (isDrawObj)
                {
                    result.Add(text);
                }

                if (o is IInlineCollection collection)
                {
                    foreach (object child in collection.Items)
                    {
                        AddInline(child, text, result);
                    }
                }
            }
        }
        
        public static List<StructuralLine> GetLines(this StructuralContainer container, float maxWidth)
        {
            List<StructuralLine> lines = new List<StructuralLine>();
            if (container.TextContainers.Count == 0 || container.TextContainers[0].GlyphsCount == 0)
            {
                return lines;
            }

            int glyphStart = -1;
            int containerStart = -1;
            int glyphsCount = 0;

            // unique
            float x = 0;
            float afterSpaceWidth = 0;
            (int containerStart, int glyphStart, int glyphsCount) previous = new (containerStart, glyphStart, glyphsCount);
            (int containerStart, int glyphStart, int glyphsCount) beforeSpace = new (containerStart, glyphStart, glyphsCount);
            (int containerIndex, int glyphIndex)? afterSpace = null;

            for (int containerIndex = 0; containerIndex < container.TextContainers.Count; containerIndex++)
            {
                containerStart = containerStart >= 0 ? containerStart : containerIndex;
                StructuralTextItem text = container.TextContainers[containerIndex];
                for (int glyphIndex = 0; glyphIndex < text.GlyphsCount; glyphIndex++)
                {
                    glyphsCount++;
                    glyphStart = glyphStart >= 0 ? glyphStart : glyphIndex;
                    containerStart = containerStart >= 0 ? containerStart : containerIndex;
                    
                    // unique
                    switch (container.Layout.TextTrimming)
                    {
                        case TextTrimming.None:
                            break;

                        case TextTrimming.CharacterEllipsis:
                            {
                                GlyphPoint glyph = text[glyphIndex];
                                float width = glyph.GetPixelWidth(text.FontSize);
                                x += width;
                                if (x > maxWidth && previous.glyphsCount > 0)
                                {
                                    lines.Add(new StructuralLine(container, previous.containerStart, previous.glyphStart, previous.glyphsCount));
                                    containerStart = containerIndex;
                                    glyphStart = glyphIndex;
                                    glyphsCount = 1;
                                    x = width;
                                }
                            }
                            break;

                        case TextTrimming.WordEllipsis:
                            {
                                GlyphPoint glyph = text[glyphIndex];
                                float width = glyph.GetPixelWidth(text.FontSize);
                                x += width;
                                afterSpaceWidth += width;
                                if (char.IsWhiteSpace(text.Chars[glyph.CharOffset]))
                                {
                                    afterSpaceWidth = 0;
                                    beforeSpace = (containerStart, glyphStart, glyphsCount);
                                    afterSpace = null;
                                }
                                else
                                {
                                    afterSpace ??= (containerIndex, glyphIndex);
                                    if (x > maxWidth)
                                    {
                                        if (beforeSpace.glyphsCount > 0)
                                        {
                                            lines.Add(new StructuralLine(container, beforeSpace.containerStart, beforeSpace.glyphStart, beforeSpace.glyphsCount));
                                            containerStart = afterSpace.Value.containerIndex;
                                            glyphStart = afterSpace.Value.glyphIndex;
                                            glyphsCount = glyphsCount - beforeSpace.glyphsCount;
                                            x = afterSpaceWidth;
                                        }
                                        else if (previous.glyphsCount > 0)
                                        {
                                            lines.Add(new StructuralLine(container, previous.containerStart, previous.glyphStart, previous.glyphsCount));
                                            containerStart = containerIndex;
                                            glyphStart = glyphIndex;
                                            glyphsCount = 1;
                                            x = width;
                                        }
                                        afterSpaceWidth = 0;
                                        beforeSpace = (containerStart, glyphStart, 0);
                                        afterSpace = null;
                                    }
                                }
                            }
                            break;
                    }
                    if (text.GetGlyphChars(glyphIndex).Last() == '\n')
                    {
                        lines.Add(new StructuralLine(container, containerStart, glyphStart, glyphsCount));
                        containerStart = glyphStart = -1;
                        glyphsCount = 0;
                        
                        // unique
                        x = 0;
                        afterSpaceWidth = 0;
                        beforeSpace = (containerStart, glyphStart, 0);
                        afterSpace = null;
                    }

                    // unique
                    previous = (containerStart, glyphStart, glyphsCount);
                }
            }
            if (glyphsCount > 0)
            {
                lines.Add(new StructuralLine(container, containerStart, glyphStart, glyphsCount));
            }

            return lines;
        }
    }
}