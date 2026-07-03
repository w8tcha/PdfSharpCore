using System;

using AwesomeAssertions;

using NUnit.Framework;

using PdfSharpCore.Pdf.Content.Objects;

namespace PdfSharpCore.Test.Pdfs.Content.Objects;

public class CNameTests
{
       
    [TestCase("/Foo")]
    public void SetNameTests(string name)
    {
        var cName = new CName
        {
            Name = name
        };

        cName.Name.Should().Be(name);
    }

    [Test]
    public void SetNameNullThrowsException()
    {
        Action act = () => new CName
        {
            Name = null
        };
        act.Should().Throw<ArgumentNullException>();
    }

       
    [TestCase("Foo")]
    [TestCase("")]
    public void SetNameWithoutPrefixThrowsException(string name)
    {
        Action act = () => new CName
        {
            Name = name
        };
        act.Should().Throw<ArgumentException>();
    }
}