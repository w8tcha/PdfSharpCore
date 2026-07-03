//
// Authors:
//   Stefan Lange (mailto:Stefan.Lange@PdfSharpCore.com)
//   Klaus Potzesny (mailto:Klaus.Potzesny@PdfSharpCore.com)
//   David Stephensen (mailto:David.Stephensen@PdfSharpCore.com)
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

using System.Diagnostics;

using MigraDocCore.DocumentObjectModel.Internals;

namespace MigraDocCore.DocumentObjectModel;

/// <summary>
/// Represents the page setup of a section.
/// </summary>
public class PageSetup : DocumentObject
{
    /// <summary>
    /// Initializes a new instance of the PageSetup class.
    /// </summary>
    public PageSetup()
    {
    }

    /// <summary>
    /// Initializes a new instance of the PageSetup class with the specified parent.
    /// </summary>
    internal PageSetup(DocumentObject parent) : base(parent) { }

    /// <summary>
    /// Creates a deep copy of this object.
    /// </summary>
    public new PageSetup Clone()
    {
        return (PageSetup)DeepCopy();
    }

    /// <summary>
    /// Gets the page's size and height for the given PageFormat.
    /// </summary>
    public static void GetPageSize(PageFormat pageFormat, out Unit pageWidth, out Unit pageHeight)
    {
        //Sizes in mm:
        pageWidth = 0;
        pageHeight = 0;
        var A0Height = 1189;
        var A0Width = 841;
        var height = 0;
        var width = 0;
        switch (pageFormat)
        {
            case PageFormat.A0:
                height = A0Height;
                width = A0Width;
                break;
            case PageFormat.A1:
                height = A0Width;
                width = A0Height / 2;
                break;
            case PageFormat.A2:
                height = A0Height / 2;
                width = A0Width / 2;
                break;
            case PageFormat.A3:
                height = A0Width / 2;
                width = A0Height / 4;
                break;
            case PageFormat.A4:
                height = A0Height / 4;
                width = A0Width / 4;
                break;
            case PageFormat.A5:
                height = A0Width / 4;
                width = A0Height / 8;
                break;
            case PageFormat.A6:
                height = A0Height / 8;
                width = A0Width / 8;
                break;
            case PageFormat.B5:
                height = 257;
                width = 182;
                break;
            case PageFormat.Letter:
                pageWidth = Unit.FromPoint(612);
                pageHeight = Unit.FromPoint(792);
                break;
            case PageFormat.Legal:
                pageWidth = Unit.FromPoint(612);
                pageHeight = Unit.FromPoint(1008);
                break;
            case PageFormat.Ledger:
                pageWidth = Unit.FromPoint(1224);
                pageHeight = Unit.FromPoint(792);
                break;
            case PageFormat.P11x17:
                pageWidth = Unit.FromPoint(792);
                pageHeight = Unit.FromPoint(1224);
                break;
        }
        if (height > 0)
            pageHeight = Unit.FromMillimeter(height);
        if (width > 0)
            pageWidth = Unit.FromMillimeter(width);
    }

    /// <summary>
    /// Gets or sets a value which defines whether the section starts on next, odd or even page.
    /// </summary>
    public BreakType SectionStart
    {
        get => (BreakType)this.sectionStart.Value;
        set => this.sectionStart.Value = (int)value;
    }
    [DV(Type = typeof(BreakType))]
    internal NEnum sectionStart = NEnum.NullValue(typeof(BreakType));

    /// <summary>
    /// Gets or sets the page orientation of the section.
    /// </summary>
    public Orientation Orientation
    {
        get => (Orientation)this.orientation.Value;
        set => this.orientation.Value = (int)value;
    }
    [DV(Type = typeof(Orientation))]
    internal NEnum orientation = NEnum.NullValue(typeof(Orientation));

    /// <summary>
    /// Gets or sets the page width.
    /// </summary>
    public Unit PageWidth
    {
        get => this.pageWidth;
        set => this.pageWidth = value;
    }
    [DV]
    internal Unit pageWidth = Unit.NullValue;

    /// <summary>
    /// Gets or sets the starting number for the first section page.
    /// </summary>
    public int StartingNumber
    {
        get => this.startingNumber.Value;
        set => this.startingNumber.Value = value;
    }
    [DV]
    internal NInt startingNumber = NInt.NullValue;

    /// <summary>
    /// Gets or sets the page height.
    /// </summary>
    public Unit PageHeight
    {
        get => this.pageHeight;
        set => this.pageHeight = value;
    }
    [DV]
    internal Unit pageHeight = Unit.NullValue;

    /// <summary>
    /// Gets or sets the top margin of the pages in the section.
    /// </summary>
    public Unit TopMargin
    {
        get => this.topMargin;
        set => this.topMargin = value;
    }
    [DV]
    internal Unit topMargin = Unit.NullValue;

    /// <summary>
    /// Gets or sets the bottom margin of the pages in the section.
    /// </summary>
    public Unit BottomMargin
    {
        get => this.bottomMargin;
        set => this.bottomMargin = value;
    }
    [DV]
    internal Unit bottomMargin = Unit.NullValue;

    /// <summary>
    /// Gets or sets the left margin of the pages in the section.
    /// </summary>
    public Unit LeftMargin
    {
        get => this.leftMargin;
        set => this.leftMargin = value;
    }
    [DV]
    internal Unit leftMargin = Unit.NullValue;

    /// <summary>
    /// Gets or sets the right margin of the pages in the section.
    /// </summary>
    public Unit RightMargin
    {
        get => this.rightMargin;
        set => this.rightMargin = value;
    }
    [DV]
    internal Unit rightMargin = Unit.NullValue;

    /// <summary>
    /// Gets or sets a value which defines whether the odd and even pages
    /// of the section have different header and footer.
    /// </summary>
    public bool OddAndEvenPagesHeaderFooter
    {
        get => this.oddAndEvenPagesHeaderFooter.Value;
        set => this.oddAndEvenPagesHeaderFooter.Value = value;
    }
    [DV]
    internal NBool oddAndEvenPagesHeaderFooter = NBool.NullValue;

    /// <summary>
    /// Gets or sets a value which define whether the section has a different
    /// first page header and footer.
    /// </summary>
    public bool DifferentFirstPageHeaderFooter
    {
        get => this.differentFirstPageHeaderFooter.Value;
        set => this.differentFirstPageHeaderFooter.Value = value;
    }
    [DV]
    internal NBool differentFirstPageHeaderFooter = NBool.NullValue;

    /// <summary>
    /// Gets or sets the distance between the header and the page top
    /// of the pages in the section.
    /// </summary>
    public Unit HeaderDistance
    {
        get => this.headerDistance;
        set => this.headerDistance = value;
    }
    [DV]
    internal Unit headerDistance = Unit.NullValue;

    /// <summary>
    /// Gets or sets the distance between the footer and the page bottom
    /// of the pages in the section.
    /// </summary>
    public Unit FooterDistance
    {
        get => this.footerDistance;
        set => this.footerDistance = value;
    }
    [DV]
    internal Unit footerDistance = Unit.NullValue;

    /// <summary>
    /// Gets or sets a value which defines whether the odd and even pages
    /// of the section should change left and right margin.
    /// </summary>
    public bool MirrorMargins
    {
        get => this.mirrorMargins.Value;
        set => this.mirrorMargins.Value = value;
    }
    [DV]
    internal NBool mirrorMargins = NBool.NullValue;

    /// <summary>
    /// Gets or sets a value which defines whether a page should break horizontally.
    /// Currently only tables are supported.
    /// </summary>
    public bool HorizontalPageBreak
    {
        get => this.horizontalPageBreak.Value;
        set => this.horizontalPageBreak.Value = value;
    }
    [DV]
    internal NBool horizontalPageBreak = NBool.NullValue;

    /// <summary>
    /// Gets or sets the page format of the section.
    /// </summary>
    public PageFormat PageFormat
    {
        get => (PageFormat)this.pageFormat.Value;
        set => this.pageFormat.Value = (int)value;
    }
    [DV(Type = typeof(PageFormat))]
    internal NEnum pageFormat = NEnum.NullValue(typeof(PageFormat));

    /// <summary>
    /// Gets or sets a comment associated with this object.
    /// </summary>
    public string Comment
    {
        get => this.comment.Value;
        set => this.comment.Value = value;
    }
    [DV]
    internal NString comment = NString.NullValue;

    /// <summary>
    /// Gets the PageSetup of the previous section, or null, if the page setup belongs 
    /// to the first section.
    /// </summary>
    public PageSetup PreviousPageSetup()
    {
        var section = Parent as Section;
        if (section != null)
        {
            section = section.PreviousSection();
            if (section != null)
                return section.PageSetup;
        }
        return null;
    }

    /// <summary>
    /// Gets a PageSetup object with default values for all properties.
    /// </summary>
    internal static PageSetup DefaultPageSetup
    {
        get
        {
            if (PageSetup.defaultPageSetup == null)
            {
                PageSetup.defaultPageSetup = new PageSetup
                {
                    PageFormat = PageFormat.A4,
                    SectionStart = BreakType.BreakNextPage,
                    Orientation = Orientation.Portrait,
                    PageWidth = "21cm",
                    PageHeight = "29.7cm",
                    TopMargin = "2.5cm",
                    BottomMargin = "2cm",
                    LeftMargin = "2.5cm",
                    RightMargin = "2.5cm",
                    HeaderDistance = "-1.05cm",
                    FooterDistance = "1.25cm",
                    OddAndEvenPagesHeaderFooter = false,
                    DifferentFirstPageHeaderFooter = false,
                    MirrorMargins = false,
                    HorizontalPageBreak = false
                };
#if DEBUG
                PageSetup.defaultPageSetupClone = PageSetup.defaultPageSetup.Clone();
#endif
            }
#if DEBUG
            else
            {
                Debug.Assert(PageSetup.defaultPageSetup.PageFormat == PageSetup.defaultPageSetupClone.PageFormat, "DefaultPageSetup must not be modified");
                Debug.Assert(PageSetup.defaultPageSetup.SectionStart == PageSetup.defaultPageSetupClone.SectionStart, "DefaultPageSetup must not be modified");
                Debug.Assert(PageSetup.defaultPageSetup.Orientation == PageSetup.defaultPageSetupClone.Orientation, "DefaultPageSetup must not be modified");
                Debug.Assert(PageSetup.defaultPageSetup.PageWidth == PageSetup.defaultPageSetupClone.PageWidth, "DefaultPageSetup must not be modified");
                Debug.Assert(PageSetup.defaultPageSetup.PageHeight == PageSetup.defaultPageSetupClone.PageHeight, "DefaultPageSetup must not be modified");
                Debug.Assert(PageSetup.defaultPageSetup.TopMargin == PageSetup.defaultPageSetupClone.TopMargin, "DefaultPageSetup must not be modified");
                Debug.Assert(PageSetup.defaultPageSetup.BottomMargin == PageSetup.defaultPageSetupClone.BottomMargin, "DefaultPageSetup must not be modified");
                Debug.Assert(PageSetup.defaultPageSetup.LeftMargin == PageSetup.defaultPageSetupClone.LeftMargin, "DefaultPageSetup must not be modified");
                Debug.Assert(PageSetup.defaultPageSetup.RightMargin == PageSetup.defaultPageSetupClone.RightMargin, "DefaultPageSetup must not be modified");
                Debug.Assert(PageSetup.defaultPageSetup.HeaderDistance == PageSetup.defaultPageSetupClone.HeaderDistance, "DefaultPageSetup must not be modified");
                Debug.Assert(PageSetup.defaultPageSetup.FooterDistance == PageSetup.defaultPageSetupClone.FooterDistance, "DefaultPageSetup must not be modified");
                Debug.Assert(PageSetup.defaultPageSetup.OddAndEvenPagesHeaderFooter == PageSetup.defaultPageSetupClone.OddAndEvenPagesHeaderFooter, "DefaultPageSetup must not be modified");
                Debug.Assert(PageSetup.defaultPageSetup.DifferentFirstPageHeaderFooter == PageSetup.defaultPageSetupClone.DifferentFirstPageHeaderFooter, "DefaultPageSetup must not be modified");
                Debug.Assert(PageSetup.defaultPageSetup.MirrorMargins == PageSetup.defaultPageSetupClone.MirrorMargins, "DefaultPageSetup must not be modified");
                Debug.Assert(PageSetup.defaultPageSetup.HorizontalPageBreak == PageSetup.defaultPageSetupClone.HorizontalPageBreak, "DefaultPageSetup must not be modified");
            }
#endif
            return defaultPageSetup;
        }
    }
    private static PageSetup defaultPageSetup;
#if DEBUG
    private static PageSetup defaultPageSetupClone;
#endif

    /// <summary>
    /// Converts PageSetup into DDL.
    /// </summary>
    internal override void Serialize(Serializer serializer)
    {
        serializer.WriteComment(this.comment.Value);
        var pos = serializer.BeginContent("PageSetup");

        if (!this.pageHeight.IsNull)
            serializer.WriteSimpleAttribute("PageHeight", this.PageHeight);

        if (!this.pageWidth.IsNull)
            serializer.WriteSimpleAttribute("PageWidth", this.PageWidth);

        if (!this.orientation.IsNull)
            serializer.WriteSimpleAttribute("Orientation", this.Orientation);

        if (!this.leftMargin.IsNull)
            serializer.WriteSimpleAttribute("LeftMargin", this.LeftMargin);

        if (!this.rightMargin.IsNull)
            serializer.WriteSimpleAttribute("RightMargin", this.RightMargin);

        if (!this.topMargin.IsNull)
            serializer.WriteSimpleAttribute("TopMargin", this.TopMargin);

        if (!this.bottomMargin.IsNull)
            serializer.WriteSimpleAttribute("BottomMargin", this.BottomMargin);

        if (!this.footerDistance.IsNull)
            serializer.WriteSimpleAttribute("FooterDistance", this.FooterDistance);

        if (!this.headerDistance.IsNull)
            serializer.WriteSimpleAttribute("HeaderDistance", this.HeaderDistance);

        if (!this.oddAndEvenPagesHeaderFooter.IsNull)
            serializer.WriteSimpleAttribute("OddAndEvenPagesHeaderFooter", this.OddAndEvenPagesHeaderFooter);

        if (!this.differentFirstPageHeaderFooter.IsNull)
            serializer.WriteSimpleAttribute("DifferentFirstPageHeaderFooter", this.DifferentFirstPageHeaderFooter);

        if (!this.sectionStart.IsNull)
            serializer.WriteSimpleAttribute("SectionStart", this.SectionStart);

        if (!this.pageFormat.IsNull)
            serializer.WriteSimpleAttribute("PageFormat", this.PageFormat);

        if (!this.mirrorMargins.IsNull)
            serializer.WriteSimpleAttribute("MirrorMargins", this.MirrorMargins);

        if (!this.horizontalPageBreak.IsNull)
            serializer.WriteSimpleAttribute("HorizontalPageBreak", this.HorizontalPageBreak);

        if (!this.startingNumber.IsNull)
            serializer.WriteSimpleAttribute("StartingNumber", this.StartingNumber);

        serializer.EndContent(pos);
    }

    /// <summary>
    /// Returns the meta object of this instance.
    /// </summary>
    internal override Meta Meta
    {
        get
        {
            if (meta == null)
                meta = new Meta(typeof(PageSetup));
            return meta;
        }
    }
    static Meta meta;
}