// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;

#if !NETSTANDARD
using System.Drawing.Imaging;
using System.Drawing;
#endif

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class Texture2D
    {
        private static Texture2D PlatformFromStream(GraphicsDevice graphicsDevice, Stream stream)
        {
#if  NETSTANDARD
            throw new NotSupportedException("PlatformFromStream is not supported on NET Standard");
#else
            // Rewind stream if it is at end
            if (stream.CanSeek && stream.Length == stream.Position)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }

            var bitmap = new Bitmap(stream);
            var bitmapRectangle = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);

            int textureDataLength = bitmap.Width * bitmap.Height;
            uint[] textureData = new uint[textureDataLength];

            BitmapData data = bitmap.LockBits(bitmapRectangle,
                                              ImageLockMode.ReadOnly,
                                              PixelFormat.Format32bppArgb);
            unsafe
            {
                uint* byteData = (uint*)data.Scan0;
                fixed (uint* baseAddress = &textureData[0])
                {
                    uint* pData = baseAddress;

                    for (int i = 0; i < textureDataLength; i++)
                    {
                        uint alpha = (byteData[i] & 0xFF000000) >> 24;
                        uint red   = (byteData[i] & 0x00FF0000) >> 16;
                        uint green = (byteData[i] & 0x0000FF00) >> 8;
                        uint blue  = (byteData[i] & 0x000000FF);

                        // Multiply alpha
                        red   = (uint)(red   * alpha / 255f);
                        green = (uint)(green * alpha / 255f);
                        blue  = (uint)(blue  * alpha / 255f);

                        alpha = alpha << 24;
                        blue  = blue << 16;
                        green = green << 8;

                        uint pixel = red   |
                                     green |
                                     blue  |
                                     alpha;

                        * pData = pixel;
                        pData++;
                    }
                }

            }
            bitmap.UnlockBits(data);

            var texture = new Texture2D(graphicsDevice, bitmap.Width, bitmap.Height);
            texture.SetData(textureData);

            bitmap.Dispose();
            data = null;

            return texture;
#endif
        }

        private void PlatformSaveAsJpeg(Stream stream, int width, int height)
        {
#if NETSTANDARD
            throw new NotSupportedException("PlatformSaveAsJpeg not implemented in NET Standard"); ;
#else
            uint[] textureData = new uint[Width * Height];

            GetData(textureData);

            ImageWriter.SaveAsImage(textureData, Width, Height, stream, width, height, ImageWriter.ImageWriterFormat.Jpg);
#endif
        }

        private void PlatformSaveAsPng(Stream stream, int width, int height)
        {
#if NETSTANDARD
            throw new NotSupportedException("PlatformSaveAsPng not implemented in NET Standard"); ;
#else
            uint[] textureData = new uint[Width * Height];

            GetData(textureData);

            ImageWriter.SaveAsImage(textureData, Width, Height, stream, width, height, ImageWriter.ImageWriterFormat.Png);
#endif
        }
    }
}
