// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;


#if !NETSTANDARD
using BigGustave;


namespace Microsoft.Xna.Framework.Graphics
{
    static class ImageWriter
    {
        public enum ImageWriterFormat
        {
            Jpg,
            Png
        }

        private static unsafe PngBuilder CreateFromPixelData(uint [] textureData, int width, int height)
        {
            var builder = PngBuilder.Create(width, height, hasAlphaChannel: true);

            fixed (uint* byteData = &textureData[0])
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int i = x + y * width;

                        byte r = (byte)(byteData[i] & 0x000000FF);
                        byte g = (byte)((byteData[i] & 0x0000FF00) >> 8);
                        byte b = (byte)((byteData[i] & 0x00FF0000) >> 16);
                        byte a = (byte)((byteData[i] & 0xFF000000) >> 24);

                        Pixel imagePixel = new Pixel(r, g, b, a, false);

                        builder.SetPixel(imagePixel, x, y);
                    }
                }
            }

            return builder;
        }

        public static void SaveAsImage(uint [] textureData,
                                       int textureWidth,
                                       int textureHeight,
                                       Stream stream, int width, int height, ImageWriterFormat format)
        {
            if (textureData == null)
            {
                throw new ArgumentNullException("textureData", "'textureData' cannot be null (Nothing in Visual Basic)");
            }
            if (stream == null)
            {
                throw new ArgumentNullException("stream", "'stream' cannot be null (Nothing in Visual Basic)");
            }
            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException("width", width, "'width' cannot be less than or equal to zero");
            }
            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException("height", height, "'height' cannot be less than or equal to zero");
            }
            if (format == ImageWriterFormat.Jpg)
            {
                throw new ArgumentException("'format' Jpeg isn't supported on .NET 6", "format");
            }

            PngBuilder builder = CreateFromPixelData(textureData, textureWidth, textureHeight);

            if (textureWidth != width || textureHeight != height)
            {
                throw new ArgumentException("Save texture with different size isn't supported on .NET 6", "width");
            }

            builder.Save(stream);
        }
    }
}

#endif
