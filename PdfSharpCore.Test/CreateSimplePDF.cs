using System.IO;
using System.Text;

using AwesomeAssertions;

using MigraDocCore.DocumentObjectModel.MigraDoc.DocumentObjectModel.Shapes;

using NUnit.Framework;

using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using PdfSharpCore.Test.Helpers;
using PdfSharpCore.Utils;

using SkiaSharp;

namespace PdfSharpCore.Test;

public class CreateSimplePdf
{
    private readonly string _rootPath = PathHelper.GetInstance().RootDir;
    private const string OutputDirName = "Out";

    [Test]
    public void CreateTestPdf()
    {
        const string outName = "test1.pdf";

        ValidateTargetAvailable(outName);

        var document = new PdfDocument();

        var pageNewRenderer = document.AddPage();

        var renderer = XGraphics.FromPdfPage(pageNewRenderer);

        renderer.DrawString("Testy Test Test", new XFont("Arial", 12), XBrushes.Black, new XPoint(12, 12));

        SaveDocument(document, outName);
        ValidateFileIsPdf(outName);
    }

    [Test]
    public void CreateTestPdfWithUnicodeMetadata()
    {
        const string data = "English, Ελληνικά, 漢語";

        var document = new PdfDocument();
        document.Info.Title = data;
        document.Info.Subject = data;
        document.Info.Author = data;

        using var ms = new MemoryStream();
        document.AddPage();
        document.Save(ms);
        ms.Position = 0;

        var generatedDocument = Pdf.IO.PdfReader.Open(ms);

        generatedDocument.Info.Title.Should().Be(data);
        generatedDocument.Info.Subject.Should().Be(data);
        generatedDocument.Info.Author.Should().Be(data);
    }

    [Test]
    public void CreateTestPdfWithImage()
    {
        using var stream = new MemoryStream();
        var document = new PdfDocument();

        var pageNewRenderer = document.AddPage();

        var renderer = XGraphics.FromPdfPage(pageNewRenderer);

        renderer.DrawImage(XImage.FromFile(PathHelper.GetInstance().GetAssetPath("lenna.png")), new XPoint(0, 0));

        document.Save(stream);
        stream.Position = 0;
        Assert.True(stream.Length > 1);
        ReadStreamAndVerifyPdfHeaderSignature(stream);
    }

    [Test]
    public void CreateTestPdfWithImageViaSkiaSharp()
    {
        ImageSource.ImageSourceImpl = new SkiaSharpImageSource();

        using var stream = new MemoryStream();
        var document = new PdfDocument();

        var pageNewRenderer = document.AddPage();
        var renderer = XGraphics.FromPdfPage(pageNewRenderer);

        // Load image with SkiaSharp and apply a grayscale colour matrix
        using var original = SKBitmap.Decode(PathHelper.GetInstance().GetAssetPath("lenna.png"));
        using var surface = SKSurface.Create(new SKImageInfo(original.Width, original.Height));
        var grayscaleMatrix = new float[]
        {
            0.21f, 0.72f, 0.07f, 0, 0,
            0.21f, 0.72f, 0.07f, 0, 0,
            0.21f, 0.72f, 0.07f, 0, 0,
            0,     0,     0,     1, 0
        };
        using var paint = new SKPaint
        {
            ColorFilter = SKColorFilter.CreateColorMatrix(grayscaleMatrix)
        };
        surface.Canvas.DrawBitmap(original, 0, 0, paint);
        using var gray = SKBitmap.FromImage(surface.Snapshot());

        var source = SkiaSharpImageSource.FromSkiaSharpBitmap(gray, isTransparent: false);
        var img = XImage.FromImageSource(source);
        renderer.DrawImage(img, new XPoint(0, 0));

        document.Save(stream);
        stream.Position = 0;
        Assert.True(stream.Length > 1);
        ReadStreamAndVerifyPdfHeaderSignature(stream);
    }

    private void SaveDocument(PdfDocument document, string name)
    {
        var outFilePath = GetOutFilePath(name);
        var dir = Path.GetDirectoryName(outFilePath);
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        document.Save(outFilePath);
    }

    private void ValidateFileIsPdf(string v)
    {
        var path = GetOutFilePath(v);
        Assert.True(File.Exists(path));
        var fi = new FileInfo(path);
        Assert.True(fi.Length > 1);

        using var stream = File.OpenRead(path);
        ReadStreamAndVerifyPdfHeaderSignature(stream);
    }

    private static void ReadStreamAndVerifyPdfHeaderSignature(Stream stream)
    {
        var readBuffer = new byte[5];
        var pdfSignature = Encoding.ASCII.GetBytes("%PDF-"); // PDF must start with %PDF-

        stream.ReadExactly(readBuffer, 0, readBuffer.Length);
        readBuffer.Should().Equal(pdfSignature);
    }

    private void ValidateTargetAvailable(string file)
    {
        var path = GetOutFilePath(file);
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        Assert.False(File.Exists(path));
    }

    private string GetOutFilePath(string name)
    {
        return Path.Combine(_rootPath, OutputDirName, name);
    }
}