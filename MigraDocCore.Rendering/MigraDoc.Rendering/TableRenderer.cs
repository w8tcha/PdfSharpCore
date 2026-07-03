//
// Authors:
//   Klaus Potzesny (mailto:Klaus.Potzesny@PdfSharpCore.com)
//
// Copyright (c) 2001-2009 empira Software GmbH, Cologne (Germany)
//
// http://www.PdfSharpCore.com
// http://www.migradoc.com
// http://sourceforge.net/projects/pdfsharp
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included
// in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using PdfSharpCore.Drawing;
using MigraDocCore.DocumentObjectModel;
using MigraDocCore.DocumentObjectModel.Visitors;
using MigraDocCore.DocumentObjectModel.Tables;
using MigraDocCore.DocumentObjectModel.Internals;

namespace MigraDocCore.Rendering;

/// <summary>
/// Renders a table to an XGraphics object.
/// </summary>
internal class TableRenderer : Renderer
{
    internal TableRenderer(XGraphics gfx, Table documentObject, FieldInfos fieldInfos)
        :
        base(gfx, documentObject, fieldInfos)
    {
        this.table = (Table)documentObject;
    }

    internal TableRenderer(XGraphics gfx, RenderInfo renderInfo, FieldInfos fieldInfos)
        :
        base(gfx, renderInfo, fieldInfos)
    {
        this.table = (Table)this.renderInfo.DocumentObject;
    }

    internal override LayoutInfo InitialLayoutInfo
    {
        get
        {
            var layoutInfo = new LayoutInfo();
            layoutInfo.KeepTogether = this.table.KeepTogether;
            layoutInfo.KeepWithNext = false;
            layoutInfo.MarginBottom = 0;
            layoutInfo.MarginLeft = 0;
            layoutInfo.MarginTop = 0;
            layoutInfo.MarginRight = 0;
            return layoutInfo;
        }
    }


    void InitRendering()
    {
        var formatInfo = (TableFormatInfo)this.renderInfo.FormatInfo;
        this.bottomBorderMap = formatInfo.bottomBorderMap;
        this.connectedRowsMap = formatInfo.connectedRowsMap;
        this.formattedCells = formatInfo.formattedCells;

        this.currRow = formatInfo.startRow;
        this.startRow = formatInfo.startRow;
        this.endRow = formatInfo.endRow;

        this.mergedCells = formatInfo.mergedCells;
        this.lastHeaderRow = formatInfo.lastHeaderRow;
        this.startX = this.renderInfo.LayoutInfo.ContentArea.X;
        this.startY = this.renderInfo.LayoutInfo.ContentArea.Y;
    }

    /// <summary>
    /// 
    /// </summary>
    void RenderHeaderRows()
    {
        if (this.lastHeaderRow < 0)
            return;

        foreach (var cell in this.mergedCells)
        {
            if (cell.Row.Index <= this.lastHeaderRow)
                RenderCell(cell);
        }
    }

    void RenderCell(Cell cell)
    {
        var innerRect = GetInnerRect(CalcStartingHeight(), cell);
        RenderShading(cell, innerRect);
        RenderContent(cell, innerRect);
        RenderBorders(cell, innerRect);
    }

    private void EqualizeRoundedCornerBorders(Cell cell) {
        // If any of a corner relevant border is set, we want to copy its values to the second corner relevant border, 
        // to ensure the innerWidth of the cell is the same, regardless of which border is used.
        // If set, we use the vertical borders as source for the values, otherwise we use the horizontal borders.
        var roundedCorner = cell.RoundedCorner;

        if (roundedCorner == RoundedCorner.None)
            return;

        BorderType primaryBorderType = BorderType.Top, secondaryBorderType = BorderType.Top;

        if (roundedCorner == RoundedCorner.TopLeft || roundedCorner == RoundedCorner.BottomLeft)
            primaryBorderType = BorderType.Left;
        if (roundedCorner == RoundedCorner.TopRight || roundedCorner == RoundedCorner.BottomRight)
            primaryBorderType = BorderType.Right;

        if (roundedCorner == RoundedCorner.TopLeft || roundedCorner == RoundedCorner.TopRight)
            secondaryBorderType = BorderType.Top;
        if (roundedCorner == RoundedCorner.BottomLeft || roundedCorner == RoundedCorner.BottomRight)
            secondaryBorderType = BorderType.Bottom;

        // If both borders don't exist, there's nothing to do and we should not create one by accessing it.
        if (!cell.Borders.HasBorder(primaryBorderType) && !cell.Borders.HasBorder(secondaryBorderType))
            return;

        // Get the borders. By using GV.ReadWrite we create the border, if not existing.
        var primaryBorder = (Border) cell.Borders.GetValue(primaryBorderType.ToString(), GV.ReadWrite);
        var secondaryBorder = (Border) cell.Borders.GetValue(secondaryBorderType.ToString(), GV.ReadWrite);

        var source = primaryBorder.Visible ? primaryBorder : secondaryBorder.Visible ? secondaryBorder : null;
        var target = primaryBorder.Visible ? secondaryBorder : secondaryBorder.Visible ? primaryBorder : null;

        if (source == null || target == null)
            return;

        target.Visible = source.Visible;
        target.Width = source.Width;
        target.Style = source.Style;
        target.Color = source.Color;
    }

    void RenderShading(Cell cell, Rectangle innerRect)
    {
        var shadeRenderer = new ShadingRenderer(this.gfx, cell.Shading);            
        shadeRenderer.Render(innerRect.X, innerRect.Y, innerRect.Width, innerRect.Height, cell.RoundedCorner);
    }

    void RenderBorders(Cell cell, Rectangle innerRect)
    {
        var leftPos = innerRect.X;
        XUnit rightPos = leftPos + innerRect.Width;
        var topPos = innerRect.Y;
        XUnit bottomPos = innerRect.Y + innerRect.Height;
        var mergedBorders = this.mergedCells.GetEffectiveBorders(cell);

        var bordersRenderer = new BordersRenderer(mergedBorders, this.gfx);
        var bottomWidth = bordersRenderer.GetWidth(BorderType.Bottom);
        var leftWidth = bordersRenderer.GetWidth(BorderType.Left);
        var topWidth = bordersRenderer.GetWidth(BorderType.Top);
        var rightWidth = bordersRenderer.GetWidth(BorderType.Right);

        if (cell.RoundedCorner == RoundedCorner.TopLeft)
            bordersRenderer.RenderRounded(cell.RoundedCorner, innerRect.X, innerRect.Y, innerRect.Width + rightWidth, innerRect.Height + bottomWidth);
        else if (cell.RoundedCorner == RoundedCorner.TopRight)
            bordersRenderer.RenderRounded(cell.RoundedCorner, innerRect.X - leftWidth, innerRect.Y, innerRect.Width + leftWidth, innerRect.Height + bottomWidth);
        else if (cell.RoundedCorner == RoundedCorner.BottomLeft)
            bordersRenderer.RenderRounded(cell.RoundedCorner, innerRect.X, innerRect.Y - topWidth, innerRect.Width + rightWidth, innerRect.Height + topWidth);
        else if (cell.RoundedCorner == RoundedCorner.BottomRight)
            bordersRenderer.RenderRounded(cell.RoundedCorner, innerRect.X - leftWidth, innerRect.Y - topWidth, innerRect.Width + leftWidth, innerRect.Height + topWidth);

        // Render horizontal and vertical borders only if touching no rounded corner.
        if (cell.RoundedCorner != RoundedCorner.TopRight && cell.RoundedCorner != RoundedCorner.BottomRight)
            bordersRenderer.RenderVertically(BorderType.Right, rightPos, topPos, bottomPos + bottomWidth - topPos);

        if (cell.RoundedCorner != RoundedCorner.TopLeft && cell.RoundedCorner != RoundedCorner.BottomLeft)
            bordersRenderer.RenderVertically(BorderType.Left, leftPos - leftWidth, topPos, bottomPos + bottomWidth - topPos);

        if (cell.RoundedCorner != RoundedCorner.BottomLeft && cell.RoundedCorner != RoundedCorner.BottomRight)
            bordersRenderer.RenderHorizontally(BorderType.Bottom, leftPos - leftWidth, bottomPos, rightPos + rightWidth + leftWidth - leftPos);

        if (cell.RoundedCorner != RoundedCorner.TopLeft && cell.RoundedCorner != RoundedCorner.TopRight)
            bordersRenderer.RenderHorizontally(BorderType.Top, leftPos - leftWidth, topPos - topWidth, rightPos + rightWidth + leftWidth - leftPos);

        RenderDiagonalBorders(mergedBorders, innerRect);
    }

    void RenderDiagonalBorders(Borders mergedBorders, Rectangle innerRect)
    {
        var bordersRenderer = new BordersRenderer(mergedBorders, this.gfx);
        bordersRenderer.RenderDiagonally(BorderType.DiagonalDown, innerRect.X, innerRect.Y, innerRect.Width, innerRect.Height);
        bordersRenderer.RenderDiagonally(BorderType.DiagonalUp, innerRect.X, innerRect.Y, innerRect.Width, innerRect.Height);
    }

    void RenderContent(Cell cell, Rectangle innerRect)
    {
        var formattedCell = (FormattedCell)this.formattedCells[cell];
        var renderInfos = formattedCell.GetRenderInfos();

        if (renderInfos == null)
            return;

        var verticalAlignment = cell.VerticalAlignment;
        var contentHeight = formattedCell.ContentHeight;
        var innerHeight = innerRect.Height;
        XUnit targetX = innerRect.X + cell.Column.LeftPadding;

        XUnit targetY;
        if (verticalAlignment == VerticalAlignment.Bottom)
        {
            targetY = innerRect.Y + innerRect.Height;
            targetY -= cell.Row.BottomPadding;
            targetY -= contentHeight;
        }
        else if (verticalAlignment == VerticalAlignment.Center)
        {
            targetY = innerRect.Y + cell.Row.TopPadding;
            targetY += innerRect.Y + innerRect.Height - cell.Row.BottomPadding;
            targetY -= contentHeight;
            targetY /= 2;
        }
        else
            targetY = innerRect.Y + cell.Row.TopPadding;

        RenderByInfos(targetX, targetY, renderInfos);
    }



    Rectangle GetInnerRect(XUnit startingHeight, Cell cell)
    {
        var bordersRenderer = new BordersRenderer(this.mergedCells.GetEffectiveBorders(cell), this.gfx);
        var formattedCell = (FormattedCell)this.formattedCells[cell];
        var width = formattedCell.InnerWidth;

        var y = this.startY;
        if (cell.Row.Index > this.lastHeaderRow)
            y += startingHeight;
        else
            y += CalcMaxTopBorderWidth(0);

        var upperBorderPos = (XUnit)this.bottomBorderMap[cell.Row.Index];

        y += upperBorderPos;
        if (cell.Row.Index > this.lastHeaderRow)
            y -= (XUnit)this.bottomBorderMap[this.startRow];

        var lowerBorderPos = (XUnit)this.bottomBorderMap[cell.Row.Index + cell.MergeDown + 1];


        XUnit height = lowerBorderPos - upperBorderPos;
        height -= bordersRenderer.GetWidth(BorderType.Bottom);

        var x = this.startX;
        for (var clmIdx = 0; clmIdx < cell.Column.Index; ++clmIdx)
        {
            x += this.table.Columns[clmIdx].Width;
        }
        x += LeftBorderOffset;

        return new Rectangle(x, y, width, height);
    }

    internal override void Render()
    {
        InitRendering();
        RenderHeaderRows();
        if (startRow < this.table.Rows.Count)
        {
            var cell = this.table[startRow, 0];

            var cellIdx = this.mergedCells.BinarySearch(this.table[startRow, 0], new CellComparer());
            while (cellIdx < this.mergedCells.Count)
            {
                cell = (Cell)this.mergedCells[cellIdx];
                if (cell.Row.Index > this.endRow)
                    break;

                RenderCell(cell);
                ++cellIdx;
            }
        }
    }

    void InitFormat(Area area, FormatInfo previousFormatInfo)
    {
        var prevTableFormatInfo = (TableFormatInfo)previousFormatInfo;
        var tblRenderInfo = new TableRenderInfo();
        tblRenderInfo.table = this.table;

        // Equalize the two borders, that are used to determine a rounded corner's border.
        // This way the innerWidth of the cell, which is got by the saved _formattedCells, is the same regardless of which corner relevant border is set.
        foreach (Row row in this.table.Rows)
        foreach (Cell cell in row.Cells)
            EqualizeRoundedCornerBorders(cell);

        this.renderInfo = tblRenderInfo;

        if (prevTableFormatInfo != null)
        {
            this.mergedCells = prevTableFormatInfo.mergedCells;
            this.formattedCells = prevTableFormatInfo.formattedCells;
            this.bottomBorderMap = prevTableFormatInfo.bottomBorderMap;
            this.lastHeaderRow = prevTableFormatInfo.lastHeaderRow;
            this.connectedRowsMap = prevTableFormatInfo.connectedRowsMap;
            this.startRow = prevTableFormatInfo.endRow + 1;
        }
        else
        {
            this.mergedCells = new MergedCellList(this.table);
            FormatCells();
            CalcLastHeaderRow();
            CreateConnectedRows();
            CreateBottomBorderMap();
            if (this.doHorizontalBreak)
            {
                CalcLastHeaderColumn();
                CreateConnectedColumns();
            }
            this.startRow = this.lastHeaderRow + 1;
        }
        ((TableFormatInfo)tblRenderInfo.FormatInfo).mergedCells = this.mergedCells;
        ((TableFormatInfo)tblRenderInfo.FormatInfo).formattedCells = this.formattedCells;
        ((TableFormatInfo)tblRenderInfo.FormatInfo).bottomBorderMap = this.bottomBorderMap;
        ((TableFormatInfo)tblRenderInfo.FormatInfo).connectedRowsMap = this.connectedRowsMap;
        ((TableFormatInfo)tblRenderInfo.FormatInfo).lastHeaderRow = this.lastHeaderRow;
    }

    void FormatCells()
    {
        this.formattedCells = new SortedList<Cell, FormattedCell>(new CellComparer());
        foreach (var cell in this.mergedCells)
        {
            var formattedCell = new FormattedCell(cell, this.documentRenderer, this.mergedCells.GetEffectiveBorders(cell), this.fieldInfos, 0, 0);
            formattedCell.Format(this.gfx);
            this.formattedCells.Add(cell, formattedCell);
        }
    }

    /// <summary>
    /// Formats (measures) the table.
    /// </summary>
    /// <param name="area">The area on which to fit the table.</param>
    /// <param name="previousFormatInfo"></param>
    internal override void Format(Area area, FormatInfo previousFormatInfo)
    {
        var elements = DocumentRelations.GetParent(this.table) as DocumentElements;
        if (elements != null)
        {
            var section = DocumentRelations.GetParent(elements) as Section;
            if (section != null)
                this.doHorizontalBreak = section.PageSetup.HorizontalPageBreak;
        }

        this.renderInfo = new TableRenderInfo();
        InitFormat(area, previousFormatInfo);

        // Don't take any Rows higher then MaxElementHeight
        var topHeight = this.CalcStartingHeight();
        var probeHeight = topHeight;
        XUnit offset = 0;
        if (this.startRow > this.lastHeaderRow + 1 &&
            this.startRow < this.table.Rows.Count)
            offset = (XUnit)this.bottomBorderMap[this.startRow] - topHeight;
        else
            offset = -CalcMaxTopBorderWidth(0);

        var probeRow = this.startRow;
        XUnit currentHeight = 0;
        XUnit startingHeight = 0;
        var isEmpty = false;

        while (probeRow < this.table.Rows.Count)
        {
            var firstProbe = probeRow == this.startRow;
            probeRow = (int)this.connectedRowsMap[probeRow];
            // Don't take any Rows higher then MaxElementHeight
            probeHeight = (XUnit)this.bottomBorderMap[probeRow + 1] - offset;
            if (firstProbe && probeHeight > MaxElementHeight - Tolerance)
                probeHeight = MaxElementHeight - Tolerance;

            //The height for the first new row(s) + headerrows:
            if (startingHeight == 0)
            {
                if (probeHeight > area.Height)
                {
                    isEmpty = true;
                    break;
                }
                startingHeight = probeHeight;
            }

            if (probeHeight > area.Height)
                break;

            else
            {
                this.currRow = probeRow;
                currentHeight = probeHeight;
                ++probeRow;
            }
        }
        if (!isEmpty)
        {
            var formatInfo = (TableFormatInfo)this.renderInfo.FormatInfo;
            formatInfo.startRow = this.startRow;
            formatInfo.isEnding = currRow >= this.table.Rows.Count - 1;
            formatInfo.endRow = this.currRow;
        }
        FinishLayoutInfo(area, currentHeight, startingHeight);
    }

    void FinishLayoutInfo(Area area, XUnit currentHeight, XUnit startingHeight)
    {
        var layoutInfo = this.renderInfo.LayoutInfo;
        layoutInfo.StartingHeight = startingHeight;
        //REM: Trailing height would have to be calculated in case tables had a keep with next property.
        layoutInfo.TrailingHeight = 0;
        if (this.currRow >= 0)
        {
            layoutInfo.ContentArea = new Rectangle(area.X, area.Y, 0, currentHeight);
            var width = LeftBorderOffset;
            foreach (Column clm in this.table.Columns)
            {
                width += clm.Width;
            }
            layoutInfo.ContentArea.Width = width;
        }
        layoutInfo.MinWidth = layoutInfo.ContentArea.Width;

        if (!this.table.Rows.IsNull("LeftIndent"))
            layoutInfo.Left = this.table.Rows.LeftIndent.Point;

        else if (this.table.Rows.Alignment == RowAlignment.Left)
        {
            if (table.Columns.Count > 0) // Errors in Wiki syntax can lead to tables w/o columns ...
            {
                var leftOffset = LeftBorderOffset;
                leftOffset += table.Columns[0].LeftPadding;
                layoutInfo.Left = -leftOffset;
            }
#if DEBUG
            else
                table.GetType();
#endif
        }

        switch (this.table.Rows.Alignment)
        {
            case RowAlignment.Left:
                layoutInfo.HorizontalAlignment = ElementAlignment.Near;
                break;

            case RowAlignment.Right:
                layoutInfo.HorizontalAlignment = ElementAlignment.Far;
                break;

            case RowAlignment.Center:
                layoutInfo.HorizontalAlignment = ElementAlignment.Center;
                break;
        }
    }

    XUnit LeftBorderOffset
    {
        get
        {
            if (this.leftBorderOffset < 0)
            {
                if (table.Rows.Count > 0 && table.Columns.Count > 0)
                {
                    var borders = this.mergedCells.GetEffectiveBorders(table[0, 0]);
                    var bordersRenderer = new BordersRenderer(borders, this.gfx);
                    this.leftBorderOffset = bordersRenderer.GetWidth(BorderType.Left);
                }
                else
                    this.leftBorderOffset = 0;
            }
            return this.leftBorderOffset;
        }
    }
    private XUnit leftBorderOffset = -1;

    /// <summary>
    /// Calcs either the height of the header rows or the height of the uppermost top border.
    /// </summary>
    /// <returns></returns>
    XUnit CalcStartingHeight()
    {
        XUnit height = 0;
        if (this.lastHeaderRow >= 0)
        {
            height = (XUnit)this.bottomBorderMap[this.lastHeaderRow + 1];
            height += CalcMaxTopBorderWidth(0);
        }
        else
        {
            if (this.table.Rows.Count > this.startRow)
                height = CalcMaxTopBorderWidth(this.startRow);
        }

        return height;
    }


    void CalcLastHeaderColumn()
    {
        this.lastHeaderColumn = -1;
        foreach (Column clm in this.table.Columns)
        {
            if (clm.HeadingFormat)
                this.lastHeaderColumn = clm.Index;
            else break;
        }
        if (this.lastHeaderColumn >= 0)
            this.lastHeaderRow = CalcLastConnectedColumn(this.lastHeaderColumn);

        //Ignore heading format if all the table is heading:
        if (this.lastHeaderRow == this.table.Rows.Count - 1)
            this.lastHeaderRow = -1;

    }

    void CalcLastHeaderRow()
    {
        this.lastHeaderRow = -1;
        foreach (Row row in this.table.Rows)
        {
            if (row.HeadingFormat)
                this.lastHeaderRow = row.Index;
            else break;
        }
        if (this.lastHeaderRow >= 0)
            this.lastHeaderRow = CalcLastConnectedRow(this.lastHeaderRow);

        //Ignore heading format if all the table is heading:
        if (this.lastHeaderRow == this.table.Rows.Count - 1)
            this.lastHeaderRow = -1;

    }

    void CreateConnectedRows()
    {
        this.connectedRowsMap = new SortedList<int, int>();
        foreach (var cell in this.mergedCells)
        {
            if (!this.connectedRowsMap.ContainsKey(cell.Row.Index))
            {
                var lastConnectedRow = CalcLastConnectedRow(cell.Row.Index);
                this.connectedRowsMap[cell.Row.Index] = lastConnectedRow;
            }
        }
    }

    void CreateConnectedColumns()
    {
        this.connectedColumnsMap = new SortedList<int, int>();
        foreach (var cell in this.mergedCells)
        {
            if (!this.connectedColumnsMap.ContainsKey(cell.Column.Index))
            {
                var lastConnectedColumn = CalcLastConnectedColumn(cell.Column.Index);
                this.connectedColumnsMap[cell.Column.Index] = lastConnectedColumn;
            }
        }
    }

    void CreateBottomBorderMap()
    {
        this.bottomBorderMap = new SortedList<int, XUnit>();
        this.bottomBorderMap.Add(0, XUnit.FromPoint(0));
        while (!this.bottomBorderMap.ContainsKey(this.table.Rows.Count))
        {
            CreateNextBottomBorderPosition();
        }
    }

    /// <summary>
    /// Calculates the top border width for the first row that is rendered or formatted.
    /// </summary>
    /// <param name="row">The row index.</param>
    XUnit CalcMaxTopBorderWidth(int row)
    {
        XUnit maxWidth = 0;
        if (this.table.Rows.Count > row)
        {
            var cellIdx = this.mergedCells.BinarySearch(this.table[row, 0], new CellComparer());
            var rowCell = this.mergedCells[cellIdx];
            while (cellIdx < this.mergedCells.Count)
            {
                rowCell = this.mergedCells[cellIdx];
                if (rowCell.Row.Index > row)
                    break;

                if (!rowCell.IsNull("Borders"))
                {
                    var bordersRenderer = new BordersRenderer(rowCell.Borders, this.gfx);
                    XUnit width = 0;
                    width = bordersRenderer.GetWidth(BorderType.Top);
                    if (width > maxWidth)
                        maxWidth = width;
                }
                ++cellIdx;
            }
        }
        return maxWidth;
    }

    /// <summary>
    /// Creates the next bottom border position.
    /// </summary>
    void CreateNextBottomBorderPosition()
    {
        var lastIdx = bottomBorderMap.Count - 1;
        var lastBorderRow = (int)bottomBorderMap.Keys[lastIdx];
        var lastPos = (XUnit)bottomBorderMap.Values[lastIdx];
        var minMergedCell = GetMinMergedCell(lastBorderRow);
        var minMergedFormattedCell = (FormattedCell)this.formattedCells[minMergedCell];
        XUnit maxBottomBorderPosition = lastPos + minMergedFormattedCell.InnerHeight;
        maxBottomBorderPosition += CalcBottomBorderWidth(minMergedCell);

        // Note: Caching the indices does speed up this function for large tables greatly.
        var minMergedCellRowIndex = minMergedCell.Row.Index;
        var minMergedCellMergeDown = minMergedCell.MergeDown;
        var mergedIndexPlusDown = minMergedCellRowIndex + minMergedCellMergeDown;
        foreach (var cell in this.mergedCells)
        {
            var rowIndex = cell.Row.Index;
            if (rowIndex > mergedIndexPlusDown)
                break;

            if (rowIndex + cell.MergeDown == mergedIndexPlusDown)
            {
                var formattedCell = (FormattedCell)this.formattedCells[cell];
                var topBorderPos = (XUnit)this.bottomBorderMap[rowIndex];
                XUnit bottomBorderPos = topBorderPos + formattedCell.InnerHeight;
                bottomBorderPos += CalcBottomBorderWidth(cell);
                if (bottomBorderPos > maxBottomBorderPosition)
                    maxBottomBorderPosition = bottomBorderPos;
            }
        }
        this.bottomBorderMap.Add(mergedIndexPlusDown + 1, maxBottomBorderPosition);
    }

    /// <summary>
    /// Calculates bottom border width of a cell.
    /// </summary>
    /// <param name="cell">The cell the bottom border of the row that is probed.</param>
    /// <returns>The calculated border width.</returns>
    XUnit CalcBottomBorderWidth(Cell cell)
    {
        var borders = this.mergedCells.GetEffectiveBorders(cell);
        if (borders != null)
        {
            var bordersRenderer = new BordersRenderer(borders, this.gfx);
            return bordersRenderer.GetWidth(BorderType.Bottom);
        }
        return 0;
    }

    /// <summary>
    /// Gets the first cell in the given row that is merged down minimally.
    /// </summary>
    /// <param name="row">The row to prope.</param>
    /// <returns>The first cell with minimal vertical merge.</returns>
    Cell GetMinMergedCell(int row)
    {
        var minMerge = this.table.Rows.Count;
        Cell minCell = null;
        foreach (var cell in this.mergedCells)
        {
            var rowIndex = cell.Row.Index; // Note: Taking index only once speeds up large tables.
            if (rowIndex <= row && rowIndex + cell.MergeDown >= row)
            {
                if (rowIndex == row && cell.MergeDown == 0)
                {
                    // Perfect match: non-merged cell in the desired row.
                    minCell = cell;
                    break;
                }
                else if (rowIndex + cell.MergeDown - row < minMerge)
                {
                    minMerge = rowIndex + cell.MergeDown - row;
                    minCell = cell;
                }
            }
            else if (rowIndex > row)
                break;
        }
        return minCell;
    }


    /// <summary>
    /// Calculates the last row that is connected with the given row.
    /// </summary>
    /// <param name="row">The row that is probed for downward connection.</param>
    /// <returns>The last row that is connected with the given row.</returns>
    int CalcLastConnectedRow(int row)
    {
        var lastConnectedRow = row;
        foreach (var cell in this.mergedCells)
        {
            var index = cell.Row.Index; // Note: Caching index here for speedup for large tables.
            if (index <= lastConnectedRow)
            {
                var downConnection = Math.Max(cell.Row.KeepWith, cell.MergeDown);
                if (lastConnectedRow < index + downConnection)
                    lastConnectedRow = index + downConnection;
            }
        }
        return lastConnectedRow;
    }

    /// <summary>
    /// Calculates the last column that is connected with the specified column.
    /// </summary>
    /// <param name="column">The column that is probed for downward connection.</param>
    /// <returns>The last column that is connected with the given column.</returns>
    int CalcLastConnectedColumn(int column)
    {
        var lastConnectedColumn = column;
        foreach (var cell in this.mergedCells)
        {
            if (cell.Column.Index <= lastConnectedColumn)
            {
                var rightConnection = Math.Max(cell.Column.KeepWith, cell.MergeRight);
                if (lastConnectedColumn < cell.Column.Index + rightConnection)
                    lastConnectedColumn = cell.Column.Index + rightConnection;
            }
        }
        return lastConnectedColumn;
    }



    Table table;
    MergedCellList mergedCells;
    SortedList<Cell, FormattedCell> formattedCells;
    SortedList<int, XUnit> bottomBorderMap;
    SortedList<int, int> connectedRowsMap;
    SortedList<int, int> connectedColumnsMap;

    int lastHeaderRow;
    int lastHeaderColumn;
    int startRow;
    int currRow;
    int endRow = -1;

    bool doHorizontalBreak = false;
    XUnit startX;
    XUnit startY;

}