// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.IO;
using Android.Graphics;
using Java.Nio;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class Texture2D : Texture
    {
        private static Texture2D PlatformFromStream(GraphicsDevice graphicsDevice, Stream stream)
        {
            using (Bitmap image = BitmapFactory.DecodeStream(stream, null, new BitmapFactory.Options
            {
                InScaled = false,
                InDither = false,
                InJustDecodeBounds = false,
                InPurgeable = true,
                InInputShareable = true,
            }))
            {
                return PlatformFromStream(graphicsDevice, image);
            }
        }

        Bitmap CreateBitmapFromTexture()
        {
            int[] pixels = new int[width * height];
            GetData(pixels);
            ConvertToABGR(Height, Width, pixels);

            var bitmap = Bitmap.CreateBitmap(Width, Height, Bitmap.Config.Argb8888);
            var bitmapBuffer = IntBuffer.Wrap(pixels);

            bitmap.CopyPixelsFromBuffer(bitmapBuffer);

            return bitmap;
        }

        private void PlatformSaveAsJpeg(Stream stream, int width, int height)
        {
            PlatformSaveAsImage(Bitmap.CompressFormat.Jpeg, 85, stream, width, height);
        }

        private void PlatformSaveAsImage(Bitmap.CompressFormat format, int quality, Stream stream, int width, int height)
        {
            using (Bitmap image = CreateBitmapFromTexture())
            {
                if ((width == image.Width) && (height == image.Height))
                {
                    image.Compress(format, quality, stream);
                    image.Recycle();
                    return;
                }

                using (Bitmap newImage = Bitmap.CreateScaledBitmap(image, width, height, false))
                {
                    newImage.Compress(format, quality, stream);
                    newImage.Recycle();
                }

                image.Recycle();
            }
        }

        private void PlatformSaveAsPng(Stream stream, int width, int height)
        {
            PlatformSaveAsImage(Bitmap.CompressFormat.Png, 100, stream, width, height);
        }
    }
}
