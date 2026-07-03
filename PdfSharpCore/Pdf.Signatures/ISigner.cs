// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.


using System.IO;

namespace PdfSharp.Pdf.Signatures;

public interface ISigner
{
    byte[] GetSignedCms(Stream stream, int pdfVersion);

    string GetName();
}