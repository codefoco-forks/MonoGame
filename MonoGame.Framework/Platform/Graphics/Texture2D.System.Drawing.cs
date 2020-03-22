// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;

using System.Drawing.Imaging;
using System.Drawing;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class Texture2D
    {
        private static Texture2D PlatformFromStream(GraphicsDevice graphicsDevice, Stream stream)
        {
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
                                              PixelFormat.Format32bppPArgb);
            unsafe {

                uint* byteData = (uint*)data.Scan0;
                fixed (uint * baseAddress = &textureData[0])
                {
                    uint * pData = baseAddress;

                    for (int i = 0; i < textureDataLength; i++)
                    {
                        uint pixel = (byteData[i] & 0x000000FF) << 16   |
                                     (byteData[i] & 0x0000FF00) |
                                     (byteData[i] & 0x00FF0000) >> 16|
                                     (byteData[i] & 0xFF000000);
                        *pData = pixel;
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
        }

        private void PlatformSaveAsJpeg(Stream stream, int width, int height)
        {
            ImageWriter.SaveAsImage(stream, width, height, ImageWriter.ImageWriterFormat.Jpg);
        }

        private void PlatformSaveAsPng(Stream stream, int width, int height)
        {
            ImageWriter.SaveAsImage(stream, width, height, ImageWriter.ImageWriterFormat.Png);
        }
    }
}
