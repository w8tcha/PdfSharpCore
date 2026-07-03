using MigraDocCore.DocumentObjectModel;

namespace MigraDocCore.Rendering.UnitTest;

/// <summary>
/// Summary description for ParagraphRenderer.
/// </summary>
public class TestParagraphRenderer
{
    public static void TextAndBlanks(string pdfOutputFile)
    {
        var document = new Document();
        var section = document.AddSection();
        var par = section.AddParagraph("Dies");
        for (var idx = 0; idx <= 40; ++idx)
        {
            par.AddCharacter(SymbolName.Blank);
            par.AddText(idx.ToString());
            par.AddCharacter(SymbolName.Blank);
            par.AddText((idx + 1).ToString());
            par.AddCharacter(SymbolName.Blank);
            par.AddText((idx + 2).ToString());
        }
        var printer = new PdfDocumentRenderer()
        {
            Document = document
        };
        printer.RenderDocument();
        printer.PdfDocument.Save(pdfOutputFile);
    }

    public static void Formatted(string pdfOutputFile)
    {
        var document = new Document();
        var section = document.AddSection();
        var par = section.AddParagraph();
        FillFormattedParagraph(par);
        var printer = new PdfDocumentRenderer()
        {
            Document = document
        };
        printer.RenderDocument();
        printer.PdfDocument.Save(pdfOutputFile);
    }

    internal static void FillFormattedParagraph(Paragraph par)
    {
        for (var idx = 0; idx <= 200; ++idx)
        {
            if (idx < 60)
            {
                var formText = par.AddFormattedText((idx).ToString(), TextFormat.Bold);
                formText.Font.Size = 16;
                formText.AddText(" ");
            }
            else if (idx < 100)
            {
                par.AddText((idx).ToString());
                par.AddText(" ");
            }
            else if (idx < 140)
            {
                var formText = par.AddFormattedText((idx).ToString(), TextFormat.Italic);
                formText.Font.Size = 6;
                formText.AddText(" ");
            }
            // Strikethrough tests
            else if (idx < 150)
            {
                var formText = par.AddFormattedText((idx).ToString());
                formText.Font.Size = 16;
                formText.Font.Strikethrough = Strikethrough.Single;
                formText.AddText(" ");
            }
            else if (idx < 160)
            {
                var formText = par.AddFormattedText((idx).ToString());
                formText.Font.Size = 8;
                formText.Font.Strikethrough = Strikethrough.DotDash;
                formText.AddText(" ");
            }
            else if (idx < 170)
            {
                var formText = par.AddFormattedText((idx).ToString());
                formText.Font.Size = 14;
                formText.Font.Strikethrough = Strikethrough.DotDotDash;
                formText.AddText(" ");
            }
            else if (idx < 180)
            {
                var formText = par.AddFormattedText((idx).ToString());
                formText.Font.Size = 20;
                formText.Font.Strikethrough = Strikethrough.Dotted;
                formText.AddText(" ");
            }

            if (idx % 50 == 0)
                par.AddLineBreak();
        }
        par.AddText(" ...ready.");
    }

    public static void Alignment(string pdfOutputFile)
    {
        var document = new Document();
        var section = document.AddSection();
        section.PageSetup.LeftMargin = 0;
        section.PageSetup.RightMargin = 0;
        var par = section.AddParagraph();
        //      FillFormattedParagraph(par);
        //      par.Format.Alignment = ParagraphAlignment.Left;

        //      par = section.AddParagraph();
        //      FillFormattedParagraph(par);
        //      par.Format.Alignment = ParagraphAlignment.Right;

        //      par = section.AddParagraph();
        FillFormattedParagraph(par);
        par.Format.Alignment = ParagraphAlignment.Center;
        //
        //      par = section.AddParagraph();
        //      FillFormattedParagraph(par);
        //      par.Format.Alignment = ParagraphAlignment.Justify;

        par.Format.FirstLineIndent = "-2cm";
        par.Format.LeftIndent = "2cm";
        par.Format.RightIndent = "3cm";
        var printer = new PdfDocumentRenderer()
        {
            Document = document
        };
        printer.RenderDocument();
        printer.PdfDocument.Save(pdfOutputFile);
    }

    public static void Tabs(string pdfOutputFile)
    {
        var document = new Document();
        var section = document.AddSection();
        section.PageSetup.LeftMargin = 0;
        section.PageSetup.RightMargin = 0;
        var par = section.AddParagraph();
        par.Format.TabStops.AddTabStop("20cm", TabAlignment.Right);
        par.AddText(" text before tab bla bla bla. text before tab bla bla bla. text before tab bla bla bla. text before tab bla bla bla.");
        //par.AddTab();
        par.AddText(" ............ after tab bla bla bla.");
        var printer = new PdfDocumentRenderer()
        {
            Document = document
        };
        printer.RenderDocument();
        printer.PdfDocument.Save(pdfOutputFile);
    }

    internal static void GiveBorders(Paragraph par)
    {
        var borders = par.Format.Borders;
        borders.Top.Color = Colors.Gray;
        borders.Top.Width = 4;
        borders.Top.Style = BorderStyle.DashDot;
        borders.Left.Color = Colors.Red;
        borders.Left.Style = BorderStyle.Dot;
        borders.Left.Width = 7;
        borders.Bottom.Color = Colors.Red;
        borders.Bottom.Width = 3;
        borders.Bottom.Style = BorderStyle.DashLargeGap;
        borders.Right.Style = BorderStyle.DashSmallGap;
        borders.Right.Width = 3;

        borders.DistanceFromBottom = "1cm";
        borders.DistanceFromTop = "1.5cm";

        borders.DistanceFromLeft = "0.5cm";
        borders.DistanceFromRight = "2cm";

        par.Format.Shading.Color = Colors.LightBlue;
    }

    public static void Borders(string outputFile)
    {
        var document = new Document();
        var section = document.AddSection();
        var par = section.AddParagraph();
        FillFormattedParagraph(par);
        GiveBorders(par);

        var printer = new PdfDocumentRenderer()
        {
            Document = document
        };
        printer.RenderDocument();
        printer.PdfDocument.Save(outputFile);
    }

    public static void Fields(string outputFile)
    {
        var document = new Document();
        var section = document.AddSection();
        var par = section.AddParagraph();
        par.AddText("Section: ");
        par.AddSectionField().Format = "ALPHABETIC";
        par.AddLineBreak();

        par.AddText("SectionPages: ");
        par.AddSectionField().Format = "alphabetic";
        par.AddLineBreak();

        par.AddText("Page: ");
        par.AddPageField().Format = "ROMAN";
        par.AddLineBreak();

        par.AddText("NumPages: ");
        par.AddNumPagesField();
        par.AddLineBreak();

        par.AddText("Date: ");
        par.AddDateField();
        par.AddLineBreak();

        par.AddText("Bookmark: ");
        par.AddBookmark("Egal");
        par.AddLineBreak();

        par.AddText("PageRef: ");
        par.AddPageRefField("Egal");

        var printer = new PdfDocumentRenderer()
        {
            Document = document
        };
        printer.RenderDocument();
        printer.PdfDocument.Save(outputFile);
    }
}