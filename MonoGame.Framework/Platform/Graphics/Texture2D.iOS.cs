// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.IO;

using Foundation;
using UIKit;
using CoreGraphics;
using System;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class Texture2D : Texture
    {
        private static Texture2D PlatformFromStream(GraphicsDevice graphicsDevice, Stream stream)
        {
            using (var uiImage = UIImage.LoadFromData(NSData.FromStream(stream)))
            {
                var cgImage = uiImage.CGImage;
                return PlatformFromStream(graphicsDevice, cgImage);
            }
        }

        private UIImage CreateImageFromTexture()
        {
            byte[] pixels = new byte[Width * Height * 4];

            GetData(pixels);

            var colorSpace = CGColorSpace.CreateDeviceRGB();
            var provider = new CGDataProvider(pixels);

            var imageRef = new CGImage(Width,
                                       Height,
                                       8,
                                       8 * 4,
                                       Width * 4,
                                       colorSpace,
                                       CGBitmapFlags.PremultipliedLast,
                                       provider,
                                       null,
                                       false, CGColorRenderingIntent.Default);


            UIImage image = new UIImage(imageRef);

            provider.Dispose();
            colorSpace.Dispose();

            return image;
        }

        private void PlatformSaveAsJpeg(Stream stream, int width, int height)
        {
            UIImage image = CreateImageFromTexture();

            if (width != Width && height != Height)
            {
                UIImage scaledImage = image.Scale(new CGSize(width, height));

                image.Dispose();
                image = scaledImage;
            }

            NSData data = image.AsJPEG();
            Stream imageStream = data.AsStream();
            imageStream.CopyTo(stream);
            imageStream.Dispose();
            data.Dispose();
            image.Dispose();
        }

        private void PlatformSaveAsPng(Stream stream, int width, int height)
        {
            UIImage image = CreateImageFromTexture();

            if (width != Width && height != Height)
            {
                UIImage scaledImage = image.Scale(new CGSize(width, height));

                image.Dispose();
                image = scaledImage;
            }

            NSData data = image.AsPNG();
            Stream imageStream = data.AsStream();
            imageStream.CopyTo(stream);
            imageStream.Dispose();
            data.Dispose();
            image.Dispose();
        }
    }
}
