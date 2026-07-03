using System;

using AwesomeAssertions;

using NUnit.Framework;

namespace PdfSharpCore.Test;

public class PdfInteger
{
    [Test]
    public void Should_beAbleToConvertToInt32()
    {
        var pdfInt = new Pdf.PdfInteger(10);
        var convertedInt = Convert.ToInt32(pdfInt);
            
        convertedInt.Should().Be(10);
    }
}