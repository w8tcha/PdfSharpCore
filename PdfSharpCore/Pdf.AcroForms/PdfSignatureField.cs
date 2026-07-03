//
// Authors:
//   Stefan Lange
//
// Copyright (c) 2005-2016 empira Software GmbH, Cologne Area (Germany)
//
// http://www.PdfSharpCore.com
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

using PdfSharp.Pdf.Annotations;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf.Annotations;
using System;

namespace PdfSharpCore.Pdf.AcroForms;

/// <summary>
/// Represents the signature field.
/// </summary>
public sealed class PdfSignatureField : PdfAcroField
{
    /// <summary>
    /// Initializes a new instance of PdfSignatureField.
    /// </summary>
    internal PdfSignatureField(PdfDocument document)
        : base(document)
    { }

    internal PdfSignatureField(PdfDictionary dict)
        : base(dict)
    { }

    public IAnnotationAppearanceHandler CustomAppearanceHandler { get; internal set; }

    /// <summary>
    /// Creates the custom appearance form X object for the annotation that represents
    /// this acro form text field.
    /// </summary>
    void RenderCustomAppearance()
    {
        var rect = Elements.GetRectangle(PdfAnnotation.Keys.Rect);

        var visible = !(rect.X1 + rect.X2 + rect.Y1 + rect.Y2 == 0);

        if (!visible)
            return;

        if (CustomAppearanceHandler == null)
            throw new Exception("AppearanceHandler is null");

        var form = new XForm(_document, rect.Size);
        var gfx = XGraphics.FromForm(form);

        CustomAppearanceHandler.DrawAppearance(gfx, rect.ToXRect());

        form.DrawingFinished();

        // Get existing or create new appearance dictionary
        if (Elements[PdfAnnotation.Keys.AP] is not PdfDictionary ap)
        {
            ap = new PdfDictionary(_document);
            Elements[PdfAnnotation.Keys.AP] = ap;
        }

        // Set XRef to normal state
        ap.Elements["/N"] = form.PdfForm.Reference;

        form.PdfRenderer.Close();
    }

    internal override void PrepareForSave()
    {
        base.PrepareForSave();
        if (CustomAppearanceHandler != null)
            RenderCustomAppearance();
    }

    /// <summary>
    /// Predefined keys of this dictionary.
    /// The description comes from PDF 1.4 Reference.
    /// </summary>
    public new class Keys : PdfAcroField.Keys
    {
        /// <summary>
        /// (Optional) The type of PDF object that this dictionary describes; if present,
        /// must be Sig for a signature dictionary.
        /// </summary>
        [KeyInfo(KeyType.Name | KeyType.Optional)]
        public new const string Type = "/Type";

        /// <summary>
        /// (Required; inheritable) The name of the signature handler to be used for
        /// authenticating the field’s contents, such as Adobe.PPKLite, Entrust.PPKEF,
        /// CICI.SignIt, or VeriSign.PPKVS.
        /// </summary>
        [KeyInfo(KeyType.Name | KeyType.Required)]
        public const string Filter = "/Filter";

        /// <summary>
        /// (Optional) The name of a specific submethod of the specified handler.
        /// </summary>
        [KeyInfo(KeyType.Name | KeyType.Optional)]
        public const string SubFilter = "/SubFilter";

        /// <summary>
        /// (Required) An array of pairs of integers (starting byte offset, length in bytes)
        /// describing the exact byte range for the digest calculation. Multiple discontinuous
        /// byte ranges may be used to describe a digest that does not include the
        /// signature token itself.
        /// </summary>
        [KeyInfo(KeyType.Array | KeyType.Required)]
        public const string ByteRange = "/ByteRange";

        /// <summary>
        /// (Required) The encrypted signature token.
        /// </summary>
        [KeyInfo(KeyType.String | KeyType.Required)]
        public const string Contents = "/Contents";

        /// <summary>
        /// (Optional) The name of the person or authority signing the document.
        /// </summary>
        [KeyInfo(KeyType.TextString | KeyType.Optional)]
        public const string Name = "/Name";

        /// <summary>
        /// (Optional) The time of signing. Depending on the signature handler, this
        /// may be a normal unverified computer time or a time generated in a verifiable
        /// way from a secure time server.
        /// </summary>
        [KeyInfo(KeyType.Date | KeyType.Optional)]
        public const string M = "/M";

        /// <summary>
        /// (Optional) The CPU host name or physical location of the signing.
        /// </summary>
        [KeyInfo(KeyType.TextString | KeyType.Optional)]
        public const string Location = "/Location";

        /// <summary>
        /// (Optional) The reason for the signing, such as (I agree…).
        /// </summary>
        [KeyInfo(KeyType.TextString | KeyType.Optional)]
        public const string Reason = "/Reason";

        /// <summary>
        /// Gets the KeysMeta for these keys.
        /// </summary>
        internal static DictionaryMeta Meta => _meta ?? (_meta = CreateMeta(typeof(Keys)));

        static DictionaryMeta _meta;
    }

    /// <summary>
    /// Gets the KeysMeta of this dictionary type.
    /// </summary>
    internal override DictionaryMeta Meta => Keys.Meta;
}