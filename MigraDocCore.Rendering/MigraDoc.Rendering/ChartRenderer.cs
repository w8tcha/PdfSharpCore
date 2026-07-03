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

using MigraDocCore.DocumentObjectModel.Internals;
using MigraDocCore.DocumentObjectModel.Tables;
using PdfSharpCore.Drawing;

using MigraDocCore.DocumentObjectModel.Shapes;
using MigraDocCore.DocumentObjectModel.Shapes.Charts;

namespace MigraDocCore.Rendering;

/// <summary>
/// Renders a chart to an XGraphics object.
/// </summary>
internal class ChartRenderer : ShapeRenderer
{
    internal ChartRenderer(XGraphics gfx, Chart chart, FieldInfos fieldInfos)
        : base(gfx, chart, fieldInfos)
    {
        this.chart = chart;
        var renderInfo = new ChartRenderInfo();
        renderInfo.shape = this.shape;
        this.renderInfo = renderInfo;
    }

    internal ChartRenderer(XGraphics gfx, RenderInfo renderInfo, FieldInfos fieldInfos)
        : base(gfx, renderInfo, fieldInfos)
    {
        this.chart = (Chart)renderInfo.DocumentObject;
    }

    FormattedTextArea GetFormattedTextArea(TextArea area, XUnit width)
    {
        if (area == null)
            return null;

        var formattedTextArea = new FormattedTextArea(this.documentRenderer, area, this.fieldInfos);

        if (!double.IsNaN(width))
            formattedTextArea.InnerWidth = width;

        formattedTextArea.Format(this.gfx);
        return formattedTextArea;
    }

    FormattedTextArea GetFormattedTextArea(TextArea area)
    {
        return GetFormattedTextArea(area, double.NaN);
    }

    void GetLeftRightVerticalPosition(out XUnit top, out XUnit bottom)
    {
        //REM: Line width is still ignored while layouting charts.
        var contentArea = this.renderInfo.LayoutInfo.ContentArea;
        var formatInfo = (ChartFormatInfo)this.renderInfo.FormatInfo;
        top = contentArea.Y;

        if (formatInfo.formattedHeader != null)
            top += formatInfo.formattedHeader.InnerHeight;

        bottom = contentArea.Y + contentArea.Height;
        if (formatInfo.formattedFooter != null)
            bottom -= formatInfo.formattedFooter.InnerHeight;
    }

    Rectangle GetLeftRect()
    {
        var contentArea = this.renderInfo.LayoutInfo.ContentArea;
        var formatInfo = (ChartFormatInfo)this.renderInfo.FormatInfo;
        XUnit top;
        XUnit bottom;
        GetLeftRightVerticalPosition(out top, out bottom);

        var left = contentArea.X;
        var width = formatInfo.formattedLeft.InnerWidth;

        return new Rectangle(left, top, width, bottom - top);
    }

    Rectangle GetRightRect()
    {
        var contentArea = this.renderInfo.LayoutInfo.ContentArea;
        var formatInfo = (ChartFormatInfo)this.renderInfo.FormatInfo;
        XUnit top;
        XUnit bottom;
        GetLeftRightVerticalPosition(out top, out bottom);

        XUnit left = contentArea.X + contentArea.Width - formatInfo.formattedRight.InnerWidth;
        var width = formatInfo.formattedRight.InnerWidth;

        return new Rectangle(left, top, width, bottom - top);
    }

    Rectangle GetHeaderRect()
    {
        var contentArea = this.renderInfo.LayoutInfo.ContentArea;
        var formatInfo = (ChartFormatInfo)this.renderInfo.FormatInfo;

        var left = contentArea.X;
        var top = contentArea.Y;
        var width = contentArea.Width;
        var height = formatInfo.formattedHeader.InnerHeight;

        return new Rectangle(left, top, width, height);
    }

    Rectangle GetFooterRect()
    {
        var contentArea = this.renderInfo.LayoutInfo.ContentArea;
        var formatInfo = (ChartFormatInfo)this.renderInfo.FormatInfo;

        var left = contentArea.X;
        XUnit top = contentArea.Y + contentArea.Height - formatInfo.formattedFooter.InnerHeight;
        var width = contentArea.Width;
        var height = formatInfo.formattedFooter.InnerHeight;

        return new Rectangle(left, top, width, height);
    }

    Rectangle GetTopRect()
    {
        var contentArea = this.renderInfo.LayoutInfo.ContentArea;
        var formatInfo = (ChartFormatInfo)this.renderInfo.FormatInfo;

        XUnit left;
        XUnit right;
        GetTopBottomHorizontalPosition(out left, out right);

        var top = contentArea.Y;
        if (formatInfo.formattedHeader != null)
            top += formatInfo.formattedHeader.InnerHeight;

        var height = formatInfo.formattedTop.InnerHeight;

        return new Rectangle(left, top, right - left, height);
    }

    Rectangle GetBottomRect()
    {
        var contentArea = this.renderInfo.LayoutInfo.ContentArea;
        var formatInfo = (ChartFormatInfo)this.renderInfo.FormatInfo;

        XUnit left;
        XUnit right;
        GetTopBottomHorizontalPosition(out left, out right);

        XUnit top = contentArea.Y + contentArea.Height - formatInfo.formattedBottom.InnerHeight;
        if (formatInfo.formattedFooter != null)
            top -= formatInfo.formattedFooter.InnerHeight;

        var height = formatInfo.formattedBottom.InnerHeight;
        return new Rectangle(left, top, right - left, height);
    }

    Rectangle GetPlotRect()
    {
        var contentArea = this.renderInfo.LayoutInfo.ContentArea;
        var formatInfo = (ChartFormatInfo)this.renderInfo.FormatInfo;
        var top = contentArea.Y;
        if (formatInfo.formattedHeader != null)
            top += formatInfo.formattedHeader.InnerHeight;

        if (formatInfo.formattedTop != null)
            top += formatInfo.formattedTop.InnerHeight;

        XUnit bottom = contentArea.Y + contentArea.Height;
        if (formatInfo.formattedFooter != null)
            bottom -= formatInfo.formattedFooter.InnerHeight;

        if (formatInfo.formattedBottom != null)
            bottom -= formatInfo.formattedBottom.InnerHeight;

        var left = contentArea.X;
        if (formatInfo.formattedLeft != null)
            left += formatInfo.formattedLeft.InnerWidth;

        XUnit right = contentArea.X + contentArea.Width;
        if (formatInfo.formattedRight != null)
            right -= formatInfo.formattedRight.InnerWidth;

        return new Rectangle(left, top, right - left, bottom - top);
    }

    internal override void Format(Area area, FormatInfo previousFormatInfo)
    {
        var formatInfo = (ChartFormatInfo)this.renderInfo.FormatInfo;

        var textArea = (TextArea)this.chart.GetValue("HeaderArea", GV.ReadOnly);
        formatInfo.formattedHeader = GetFormattedTextArea(textArea, this.chart.Width.Point);

        textArea = (TextArea)this.chart.GetValue("FooterArea", GV.ReadOnly);
        formatInfo.formattedFooter = GetFormattedTextArea(textArea, this.chart.Width.Point);

        textArea = (TextArea)this.chart.GetValue("LeftArea", GV.ReadOnly);
        formatInfo.formattedLeft = GetFormattedTextArea(textArea);

        textArea = (TextArea)this.chart.GetValue("RightArea", GV.ReadOnly);
        formatInfo.formattedRight = GetFormattedTextArea(textArea);

        textArea = (TextArea)this.chart.GetValue("TopArea", GV.ReadOnly);
        formatInfo.formattedTop = GetFormattedTextArea(textArea, GetTopBottomWidth());

        textArea = (TextArea)this.chart.GetValue("BottomArea", GV.ReadOnly);
        formatInfo.formattedBottom = GetFormattedTextArea(textArea, GetTopBottomWidth());

        base.Format(area, previousFormatInfo);
        formatInfo.chartFrame = ChartMapper.ChartMapper.Map(this.chart);
    }


    XUnit AlignVertically(VerticalAlignment vAlign, XUnit top, XUnit bottom, XUnit height)
    {
        switch (vAlign)
        {
            case VerticalAlignment.Bottom:
                return bottom - height;
            case VerticalAlignment.Center:
                return (top + bottom - height) / 2;
            default:
                return top;
        }
    }

    /// <summary>
    /// Gets the width of the top and bottom area.
    /// Used while formatting.
    /// </summary>
    /// <returns>The width of the top and bottom area</returns>
    private XUnit GetTopBottomWidth()
    {
        var formatInfo = (ChartFormatInfo)this.renderInfo.FormatInfo;
        XUnit width = this.chart.Width.Point;
        if (formatInfo.formattedRight != null)
            width -= formatInfo.formattedRight.InnerWidth;
        if (formatInfo.formattedLeft != null)
            width -= formatInfo.formattedLeft.InnerWidth;
        return width;
    }

    /// <summary>
    /// Gets the horizontal boundaries of the top and bottom area.
    /// Used while rendering.
    /// </summary>
    /// <param name="left">The left boundary of the top and bottom area</param>
    /// <param name="right">The right boundary of the top and bottom area</param>
    private void GetTopBottomHorizontalPosition(out XUnit left, out XUnit right)
    {
        var contentArea = this.renderInfo.LayoutInfo.ContentArea;
        var formatInfo = (ChartFormatInfo)this.renderInfo.FormatInfo;
        left = contentArea.X;
        right = contentArea.X + contentArea.Width;

        if (formatInfo.formattedRight != null)
            right -= formatInfo.formattedRight.InnerWidth;
        if (formatInfo.formattedLeft != null)
            left += formatInfo.formattedLeft.InnerWidth;
    }

    void RenderArea(FormattedTextArea area, Rectangle rect)
    {
        if (area == null)
            return;


        var textArea = area.textArea;


        var fillFormatRenderer = new FillFormatRenderer((FillFormat)textArea.GetValue("FillFormat", GV.ReadOnly), this.gfx);
        fillFormatRenderer.Render(rect.X, rect.Y, rect.Width, rect.Height);

        var top = rect.Y;
        top += textArea.TopPadding;
        XUnit bottom = rect.Y + rect.Height;
        bottom -= textArea.BottomPadding;
        top = AlignVertically(textArea.VerticalAlignment, top, bottom, area.ContentHeight);

        var left = rect.X;
        left += textArea.LeftPadding;

        var renderInfos = area.GetRenderInfos();
        RenderByInfos(left, top, renderInfos);

        var lineFormatRenderer = new LineFormatRenderer((LineFormat)textArea.GetValue("LineFormat", GV.ReadOnly), this.gfx);
        lineFormatRenderer.Render(rect.X, rect.Y, rect.Width, rect.Height);
    }

    internal override void Render()
    {
        RenderFilling();
        var contentArea = this.renderInfo.LayoutInfo.ContentArea;

        var formatInfo = (ChartFormatInfo)this.renderInfo.FormatInfo;
        if (formatInfo.formattedHeader != null)
            RenderArea(formatInfo.formattedHeader, GetHeaderRect());

        if (formatInfo.formattedFooter != null)
            RenderArea(formatInfo.formattedFooter, GetFooterRect());

        if (formatInfo.formattedTop != null)
            RenderArea(formatInfo.formattedTop, GetTopRect());

        if (formatInfo.formattedBottom != null)
            RenderArea(formatInfo.formattedBottom, GetBottomRect());

        if (formatInfo.formattedLeft != null)
            RenderArea(formatInfo.formattedLeft, GetLeftRect());

        if (formatInfo.formattedRight != null)
            RenderArea(formatInfo.formattedRight, GetRightRect());

        var plotArea = (PlotArea)this.chart.GetValue("PlotArea", GV.ReadOnly);
        if (plotArea != null)
            RenderPlotArea(plotArea, GetPlotRect());

        RenderLine();
    }

    void RenderPlotArea(PlotArea area, Rectangle rect)
    {
        var chartFrame = ((ChartFormatInfo)this.renderInfo.FormatInfo).chartFrame;

        var top = rect.Y;
        top += area.TopPadding;

        XUnit bottom = rect.Y + rect.Height;
        bottom -= area.BottomPadding;

        var left = rect.X;
        left += area.LeftPadding;

        XUnit right = rect.X + rect.Width;
        right -= area.RightPadding;

        chartFrame.Location = new XPoint(left, top);
        chartFrame.Size = new XSize(right - left, bottom - top);
        chartFrame.DrawChart(this.gfx);
    }
    Chart chart;
}