// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics.Font;
using SharpFont;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    // Uses FreeType to rasterize TrueType fonts into a series of glyph bitmaps.
    internal class SharpFontImporter : IFontImporter
    {
        // Properties hold the imported font data.
        public IEnumerable<Glyph> Glyphs { get; private set; }

        public float LineSpacing { get; private set; }

        public int YOffsetMin { get; private set; }

        // Size of the temp surface used for GDI+ rasterization.
        const int MaxGlyphSize = 1024;

        Library lib = null;

        public void Import(FontDescription options, string[] fontNames)
        {
            lib = new Library();
            // Create a bunch of GDI+ objects.
            FontFaceCache[] faces = CreateFontFaces(options, fontNames);
            try
            {
                // Which characters do we want to include?
                var characters = options.Characters;

                var glyphList = new List<Glyph>();

                // Rasterize each character in turn.
                foreach (char character in characters)
                {
                    foreach (FontFaceCache faceCache in faces)
                    {
                        Face face = faceCache.FontFace;
                        Dictionary<uint, GlyphData> glyphMaps = faceCache.GlyphMaps;

                        uint glyphIndex = face.GetCharIndex(character);
                        if (glyphIndex == 0)
                            continue;

                        if (!glyphMaps.TryGetValue(glyphIndex, out GlyphData glyphData))
                        {
                            glyphData = ImportGlyph(glyphIndex, face);
                            glyphMaps.Add(glyphIndex, glyphData);
                        }

                        var glyph = new Glyph(character, glyphData);
                        glyphList.Add(glyph);
                        break;
                    }
                }
                Glyphs = glyphList;

                Face firstFace = faces[0].FontFace;

                // Store the font height.
                LineSpacing = firstFace.Size.Metrics.Height >> 6;

                // The height used to calculate the Y offset for each character.
                YOffsetMin = -firstFace.Size.Metrics.Ascender >> 6;
            }
            finally
            {
                foreach (FontFaceCache face in faces)
                {
                    if (face != null)
                        face.FontFace.Dispose();
                }

                if (lib != null)
                {
                    lib.Dispose();
                    lib = null;
                }
            }
        }

        private FontFaceCache[] CreateFontFaces(FontDescription options, string[] fontNames)
        {
            int count = fontNames.Length;
            FontFaceCache[] faces = new FontFaceCache[count];
            for (int i = 0; i < count; i++)
            {
                faces[i] = CreateFontFace(options, fontNames[i]);
            }
            return faces;
        }


        // Attempts to instantiate the requested GDI+ font object.
        private FontFaceCache CreateFontFace(FontDescription options, string fontName)
        {
            try
            {
                uint dpi = 96;
                if (options.Scale != 1.0f && options.Scale != 0.0f)
                {
                    dpi = (uint)(dpi * options.Scale);
                }

                var face = lib.NewFace(fontName, 0);
                var fixedSize = ((int)options.Size << 6);
                face.SetCharSize(0, fixedSize, dpi, dpi);

                if (face.FamilyName == "Microsoft Sans Serif" && options.FontName != "Microsoft Sans Serif")
                    throw new PipelineException(string.Format("Font {0} is not installed on this computer.", options.FontName));

                return new FontFaceCache(face);

                // A font substitution must have occurred.
                //throw new Exception(string.Format("Can't find font '{0}'.", options.FontName));
            }
            catch
            {
                throw;
            }
        }

        // Rasterizes a single character glyph.
        private GlyphData ImportGlyph(uint glyphIndex, Face face)
        {
            face.LoadGlyph(glyphIndex, LoadFlags.Default, LoadTarget.Normal);
            face.Glyph.Outline.Embolden(64);
            face.Glyph.RenderGlyph(RenderMode.Normal);

            // Render the character.
            BitmapContent glyphBitmap = null;
            if (face.Glyph.Bitmap.Width > 0 && face.Glyph.Bitmap.Rows > 0)
            {
                glyphBitmap = new PixelBitmapContent<byte>(face.Glyph.Bitmap.Width, face.Glyph.Bitmap.Rows);
                byte[] gpixelAlphas = new byte[face.Glyph.Bitmap.Width * face.Glyph.Bitmap.Rows];
                //if the character bitmap has 1bpp we have to expand the buffer data to get the 8bpp pixel data
                //each byte in bitmap.bufferdata contains the value of to 8 pixels in the row
                //if bitmap is of width 10, each row has 2 bytes with 10 valid bits, and the last 6 bits of 2nd byte must be discarded
                if (face.Glyph.Bitmap.PixelMode == PixelMode.Mono)
                {
                    //variables needed for the expansion, amount of written data, length of the data to write
                    int written = 0, length = face.Glyph.Bitmap.Width * face.Glyph.Bitmap.Rows;
                    for (int i = 0; written < length; i++)
                    {
                        //width in pixels of each row
                        int width = face.Glyph.Bitmap.Width;
                        while (width > 0)
                        {
                            //valid data in the current byte
                            int stride = MathHelper.Min(8, width);
                            //copy the valid bytes to pixeldata
                            //System.Array.Copy(ExpandByte(face.Glyph.Bitmap.BufferData[i]), 0, gpixelAlphas, written, stride);
                            ExpandByteAndCopy(face.Glyph.Bitmap.BufferData[i], stride, gpixelAlphas, written);
                            written += stride;
                            width -= stride;
                            if (width > 0)
                                i++;
                        }
                    }
                }
                else
                    Marshal.Copy(face.Glyph.Bitmap.Buffer, gpixelAlphas, 0, gpixelAlphas.Length);
                glyphBitmap.SetPixelData(gpixelAlphas);
            }

            if (glyphBitmap == null)
            {
                var gHA = face.Glyph.Metrics.HorizontalAdvance >> 6;
                var gVA = face.Size.Metrics.Height >> 6;

                gHA = gHA > 0 ? gHA : gVA;
                gVA = gVA > 0 ? gVA : gHA;

                glyphBitmap = new PixelBitmapContent<byte>(gHA, gVA);
            }

            // not sure about this at all
            var abc = new ABCFloat();
            abc.A = face.Glyph.Metrics.HorizontalBearingX >> 6;
            abc.B = face.Glyph.Metrics.Width >> 6;
            abc.C = (face.Glyph.Metrics.HorizontalAdvance >> 6) - (abc.A + abc.B);
            abc.A -= face.Glyph.BitmapLeft;
            abc.B += face.Glyph.BitmapLeft;

            // Construct the output Glyph object.
            return new GlyphData(glyphIndex, glyphBitmap)
            {
                XOffset = -(face.Glyph.Advance.X >> 6),
                XAdvance = face.Glyph.Metrics.HorizontalAdvance >> 6,
                YOffset = -(face.Glyph.Metrics.HorizontalBearingY >> 6),
                CharacterWidths = abc
            };
        }


        /// <summary>
        /// Reads each individual bit of a byte from left to right and expands it to a full byte, 
        /// ones get byte.maxvalue, and zeros get byte.minvalue.
        /// </summary>
        /// <param name="origin">Byte to expand and copy</param>
        /// <param name="length">Number of Bits of the Byte to copy, from 1 to 8</param>
        /// <param name="destination">Byte array where to copy the results</param>
        /// <param name="startIndex">Position where to begin copying the results in destination</param>
        private static void ExpandByteAndCopy(byte origin, int length, byte[] destination, int startIndex)
        {
            byte tmp;
            for (int i = 7; i > 7 - length; i--)
            {
                tmp = (byte)(1 << i);
                if (origin / tmp == 1)
                {
                    destination[startIndex + 7 - i] = byte.MaxValue;
                    origin -= tmp;
                }
                else
                    destination[startIndex + 7 - i] = byte.MinValue;
            }
        }
    }
}
