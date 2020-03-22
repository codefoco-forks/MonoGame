// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;



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

        public static void SaveAsImage(Stream stream, int width, int height, ImageWriterFormat format)
        {
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

            var image = Image.FromStream(stream);

            if (image.Width == width && image.Height == height)
            {
                image.Save(stream, imageFormat);
                return;
            }

            var resizeImage = image.GetThumbnailImage(width, height, null, IntPtr.Zero);
            resizeImage.Save(stream, imageFormat);
        }
    }
}
