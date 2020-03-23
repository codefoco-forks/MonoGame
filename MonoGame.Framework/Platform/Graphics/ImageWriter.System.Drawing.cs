// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;


#if !NETSTANDARD
using System.Drawing.Imaging;
using System.Drawing;


namespace Microsoft.Xna.Framework.Graphics
{
    static class ImageWriter
    {
        public enum ImageWriterFormat
        {
            Jpg,
            Png
        }

        private static Bitmap CreateFromPixelData(uint [] textureData, int width, int height)
        {
            var bitmap = new Bitmap(width, height);
            var bitmapRectangle = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);

            int textureDataLength = bitmap.Width * bitmap.Height;

            BitmapData data = bitmap.LockBits(bitmapRectangle,
                                              ImageLockMode.WriteOnly,
                                              PixelFormat.Format32bppPArgb);
            unsafe
            {

                uint* baseAddress = (uint*)data.Scan0;
                fixed (uint* byteData = &textureData[0])
                {
                    uint* pData = baseAddress;

                    for (int i = 0; i < textureDataLength; i++)
                    {
                        uint pixel = (byteData[i] & 0x000000FF) << 16 |
                                     (byteData[i] & 0x0000FF00) |
                                     (byteData[i] & 0x00FF0000) >> 16 |
                                     (byteData[i] & 0xFF000000);
                        *pData = pixel;
                        pData++;
                    }
                }

            }
            bitmap.UnlockBits(data);
            return bitmap;
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


            var imageFormat = ImageFormat.Jpeg;

            if (format == ImageWriterFormat.Png)
                imageFormat = ImageFormat.Png;

            var image = CreateFromPixelData(textureData, textureWidth, textureHeight);

            if (textureWidth == width && textureHeight == height)
            {
                image.Save(stream, imageFormat);
                return;
            }

            var resizeImage = image.GetThumbnailImage(width, height, null, IntPtr.Zero);
            resizeImage.Save(stream, imageFormat);
        }
    }
}

#endif
