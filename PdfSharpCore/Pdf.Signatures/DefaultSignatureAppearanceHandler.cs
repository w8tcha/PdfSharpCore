// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf.Annotations;
using PdfSharpCore.Drawing;
using PdfSharpCore.Drawing.Layout;
using System;

using PdfSharpCore.Drawing.Layout.enums;

namespace PdfSharp.Pdf.Signatures;

internal class DefaultSignatureAppearanceHandler : IAnnotationAppearanceHandler
{
    public string? Location { get; set; }
    public string? Reason { get; set; }
    public string? Signer { get; set; }


    public void DrawAppearance(XGraphics gfx, XRect rect)
    {
        var backColor = XColor.Empty;
        var defaultText = $"Signed by: {Signer}\nLocation: {Location}\nReason: {Reason}\nDate: {DateTime.Now}";

        var font = new XFont("Verdana", 7, XFontStyle.Regular);

        var txtFormat = new XTextFormatter(gfx);

        var currentPosition = new XPoint(0, 0);

        var alignment = new TextFormatAlignment(){Vertical = XVerticalAlignment.Top, Horizontal = XParagraphAlignment.Left};

        txtFormat.DrawString(defaultText,
            font,
            new XSolidBrush(XColor.FromKnownColor(XKnownColor.Black)),
            new XRect(currentPosition.X, currentPosition.Y, rect.Width - currentPosition.X, rect.Height),
            alignment);
    }
}