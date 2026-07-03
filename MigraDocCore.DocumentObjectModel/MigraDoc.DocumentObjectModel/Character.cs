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

using System;

using MigraDocCore.DocumentObjectModel.Internals;

namespace MigraDocCore.DocumentObjectModel;

/// <summary>
/// Represents a special character in paragraph text.
/// </summary>
// TODO: So ändern, dass symbolName und char in unterschiedlichen Feldern gespeichert wird
public class Character : DocumentObject
{
    // \space
    public static readonly Character Blank = new(SymbolName.Blank);
    public static readonly Character En = new(SymbolName.En);
    public static readonly Character Em = new(SymbolName.Em);
    public static readonly Character EmQuarter = new(SymbolName.EmQuarter);
    public static readonly Character Em4 = new(SymbolName.Em4);

    // used to serialize as \tab, \linebreak
    public static readonly Character Tab = new(SymbolName.Tab);
    public static readonly Character LineBreak = new(SymbolName.LineBreak);
    //public static readonly Character MarginBreak         = new Character(SymbolName.MarginBreak);

    // \symbol
    public static readonly Character Euro = new(SymbolName.Euro);
    public static readonly Character Copyright = new(SymbolName.Copyright);
    public static readonly Character Trademark = new(SymbolName.Trademark);
    public static readonly Character RegisteredTrademark = new(SymbolName.RegisteredTrademark);
    public static readonly Character Bullet = new(SymbolName.Bullet);
    public static readonly Character Not = new(SymbolName.Not);
    public static readonly Character EmDash = new(SymbolName.EmDash);
    public static readonly Character EnDash = new(SymbolName.EnDash);
    public static readonly Character NonBreakableBlank = new(SymbolName.NonBreakableBlank);
    public static readonly Character HardBlank = new(SymbolName.HardBlank);

    /// <summary>
    /// Initializes a new instance of the Character class.
    /// </summary>
    public Character()
    {
    }

    /// <summary>
    /// Initializes a new instance of the Character class with the specified parent.
    /// </summary>
    internal Character(DocumentObject parent) : base(parent) { }

    /// <summary>
    /// Initializes a new instance of the Character class with the specified SymbolName.
    /// </summary>
    Character(SymbolName name)
        : this()
    {
        //DaSt: uint wird nicht akzeptiert, muss auf int casten
        //SetValue("SymbolName", (int)(uint)name);
        this.symbolName.Value = (int)name;
    }

    /// <summary>
    /// Gets or sets the SymbolName. Returns 0 if the type is defined by a character.
    /// </summary>
    public SymbolName SymbolName
    {
        get => (SymbolName)this.symbolName.Value;
        set => this.symbolName.Value = (int)value;
    }
    [DV(Type = typeof(SymbolName))]
    internal NEnum symbolName = NEnum.NullValue(typeof(SymbolName));

    /// <summary>
    /// Gets or sets the SymbolName as character. Returns 0 if the type is defined via an enum.
    /// </summary>
    public char Char
    {
        get
        {
            if (((uint)symbolName.Value & 0xF0000000) == 0)
                return (char)symbolName.Value;
            else
                return '\0';
        }
        set => this.symbolName.Value = (int)value;
    }

    /// <summary>
    /// Gets or sets the number of times the character is repeated.
    /// </summary>
    public int Count
    {
        get => this.count.Value;
        set => this.count.Value = value;
    }
    [DV]
    internal NInt count = new(1);

    /// <summary>
    /// Converts Character into DDL.
    /// </summary>
    internal override void Serialize(Serializer serializer)
    {
        var text = String.Empty;
        if (count == 1)
        {
            if ((SymbolName)symbolName.Value == SymbolName.Tab)
                text = "\\tab ";
            else if ((SymbolName)symbolName.Value == SymbolName.LineBreak)
                text = "\\linebreak\x0D\x0A";
            else if ((SymbolName)symbolName.Value == SymbolName.ParaBreak)
                text = "\x0D\x0A\x0D\x0A";
            //else if (symbolType == SymbolName.MarginBreak)
            //  text = "\\marginbreak ";

            if (text != "")
            {
                serializer.Write(text);
                return;
            }
        }

        if (((uint)symbolName.Value & 0xF0000000) == 0xF0000000)
        {
            // SymbolName == SpaceType?
            if (((uint)symbolName.Value & 0xF1000000) == 0xF1000000)
            {
                if ((SymbolName)symbolName.Value == SymbolName.Blank)
                {
                    //Note: Don't try to optimize it by leaving away the braces in case a single space is added.
                    //This would lead to confusion with '(' in directly following text.
                    text = "\\space(" + Count + ")";
                }
                else
                {
                    if (count == 1)
                        text = "\\space(" + SymbolName + ")";
                    else
                        text = "\\space(" + SymbolName + ", " + Count + ")";
                }
            }
            else
            {
                text = "\\symbol(" + SymbolName + ")";
            }
        }
        else
        {
            // symbolType is a (unicode) character
            text = " \\chr(0x" + ((int)symbolName.Value).ToString("X") + ")";
        }

        serializer.Write(text);
    }

    /// <summary>
    /// Returns the meta object of this instance.
    /// </summary>
    internal override Meta Meta
    {
        get
        {
            if (meta == null)
                meta = new Meta(typeof(Character));
            return meta;
        }
    }
    static Meta meta;
}