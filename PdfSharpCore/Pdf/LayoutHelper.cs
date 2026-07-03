//
// Authors:
//   Stefan Lange
//
// Copyright (c) 2005-2016 empira Software GmbH, Cologne Area (Germany)
//
// http://www.PdfSharp.com
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

using PdfSharpCore.Drawing;

namespace PdfSharpCore.Pdf;

public class LayoutHelper(PdfDocument document, XUnit topPosition, XUnit bottomMargin)
{
    private XUnit _currentPosition = bottomMargin + 10000;

    // Set a value outside the page - a new page will be created on the first request.

    public XUnit GetLinePosition(XUnit requestedHeight)
    {
        return GetLinePosition(requestedHeight, -1f);
    }

    public XUnit GetLinePosition(XUnit requestedHeight, XUnit requiredHeight)
    {
        var required = requiredHeight == -1f ? requestedHeight : requiredHeight;
        if (_currentPosition + required > bottomMargin)
            CreatePage();
        var result = _currentPosition;
        _currentPosition += requestedHeight;
        return result;
    }

    public XGraphics Gfx { get; private set; }
    public PdfPage Page { get; private set; }

    private void CreatePage()
    {
        Page = document.AddPage();
        Page.Size = PageSize.A4;
        Gfx = XGraphics.FromPdfPage(Page);
        _currentPosition = topPosition;
    }
}