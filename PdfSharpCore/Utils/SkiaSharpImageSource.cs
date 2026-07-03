using System;
using System.IO;
using MigraDocCore.DocumentObjectModel.MigraDoc.DocumentObjectModel.Shapes;
using SkiaSharp;

namespace PdfSharpCore.Utils;

public class SkiaSharpImageSource : ImageSource
{
    /// <summary>
    /// Wraps a pre-loaded <see cref="SKBitmap"/> so it can be embedded in a PDF.
    /// The bitmap is copied, so the caller may dispose the original after this call.
    /// </summary>
    public static IImageSource FromSkiaSharpBitmap(SKBitmap bitmap, bool isTransparent = false, int quality = 75)
    {
        var name = "*" + Guid.NewGuid().ToString("B");
        return new Impl(name, bitmap.Copy(), quality, isTransparent);
    }

    protected override IImageSource FromFileImpl(string path, int? quality = 75)
    {
        var bitmap = DecodeToSrgb(SKCodec.Create(path), path);
        return new Impl(path, bitmap, quality ?? 75, IsPngFile(path));
    }

    protected override IImageSource FromBinaryImpl(string name, Func<byte[]> imageSource, int? quality = 75)
    {
        var bytes = imageSource.Invoke();
        var bitmap = DecodeToSrgb(SKCodec.Create(new SKMemoryStream(bytes)), name);
        return new Impl(name, bitmap, quality ?? 75, IsPngBytes(bytes));
    }

    protected override IImageSource FromStreamImpl(string name, Func<Stream> imageStream, int? quality = 75)
    {
        using var stream = imageStream.Invoke();
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        var bytes = ms.ToArray();
        var bitmap = DecodeToSrgb(SKCodec.Create(new SKMemoryStream(bytes)), name);
        return new Impl(name, bitmap, quality ?? 75, IsPngBytes(bytes));
    }

    // Decode using an explicit sRGB (non-linear) target so that no gamma/color-space
    // conversion alters the pixel values stored in the source image. Without this,
    // SKBitmap.Decode honours the PNG's embedded profile and may linearise the colours,
    // which then appear darker in the PDF because /DeviceRGB is interpreted as sRGB.
    private static SKBitmap DecodeToSrgb(SKCodec codec, string name)
    {
        using (codec)
        {
            if (codec == null)
                throw new InvalidOperationException($"SkiaSharp could not decode image: {name}");
            var info = new SKImageInfo(
                codec.Info.Width, codec.Info.Height,
                SKColorType.Bgra8888, SKAlphaType.Unpremul,
                SKColorSpace.CreateSrgb());
            var bitmap = new SKBitmap(info);
            codec.GetPixels(info, bitmap.GetPixels());
            return bitmap;
        }
    }

    private static bool IsPngFile(string path)
    {
        Span<byte> hdr = stackalloc byte[4];
        using var f = File.OpenRead(path);
        return f.Read(hdr) == 4 && IsPngBytes(hdr);
    }

    private static bool IsPngBytes(ReadOnlySpan<byte> data) =>
        data.Length >= 4 &&
        data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47;

    private sealed class Impl : IImageSource
    {
        private readonly SKBitmap _bitmap;
        private readonly int _quality;

        public Impl(string name, SKBitmap bitmap, int quality, bool transparent)
        {
            Name = name;
            _bitmap = bitmap;
            _quality = quality;
            Transparent = transparent;
        }

        public int Width => _bitmap.Width;
        public int Height => _bitmap.Height;
        public string Name { get; }
        public bool Transparent { get; }

        public void Dispose() => _bitmap.Dispose();

        public void SaveAsJpeg(MemoryStream ms)
        {
            using var data = _bitmap.Encode(SKEncodedImageFormat.Jpeg, _quality);
            data.SaveTo(ms);
        }

        public void SaveAsPdfBitmap(MemoryStream ms)
        {
            int width = _bitmap.Width;
            int height = _bitmap.Height;
            int rowStride = width * 4;
            int pixelDataSize = rowStride * height;
            const int fileHeaderSize = 14;
            const int infoHeaderSize = 40;
            int fileSize = fileHeaderSize + infoHeaderSize + pixelDataSize;

            using var bw = new BinaryWriter(ms, System.Text.Encoding.UTF8, leaveOpen: true);

            // BITMAPFILEHEADER
            bw.Write((byte)'B');
            bw.Write((byte)'M');
            bw.Write(fileSize);
            bw.Write((short)0);                        // reserved1
            bw.Write((short)0);                        // reserved2
            bw.Write(fileHeaderSize + infoHeaderSize);  // offset to pixel data

            // BITMAPINFOHEADER (40 bytes)
            bw.Write(infoHeaderSize);
            bw.Write(width);
            bw.Write(height);    // positive = bottom-up, which PdfImage expects
            bw.Write((short)1);  // planes
            bw.Write((short)32); // bitsPerPixel (BGRA)
            bw.Write(0);         // BI_RGB (no compression)
            bw.Write(pixelDataSize);
            bw.Write(0);         // xPelsPerMeter
            bw.Write(0);         // yPelsPerMeter
            bw.Write(0);         // clrUsed
            bw.Write(0);         // clrImportant

            // SKBitmap.Pixels always returns straight (unpremultiplied) alpha as SKColor[].
            // Using this avoids the premultiplied-alpha bug where CopyTo/Marshal.Copy would
            // write darkened RGB values for semi-transparent pixels (anti-aliased edges).
            // BMP is bottom-up; SkiaSharp stores rows top-down, so iterate in reverse.
            var colors = _bitmap.Pixels;
            var row = new byte[rowStride];
            for (int y = height - 1; y >= 0; y--)
            {
                int src = y * width;
                for (int x = 0; x < width; x++)
                {
                    var c = colors[src + x];
                    row[x * 4 + 0] = c.Blue;
                    row[x * 4 + 1] = c.Green;
                    row[x * 4 + 2] = c.Red;
                    row[x * 4 + 3] = c.Alpha;
                }
                bw.Write(row);
            }
        }
    }
}
