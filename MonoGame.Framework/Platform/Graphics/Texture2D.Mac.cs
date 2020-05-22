// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.IO;

using AppKit;
using CoreGraphics;
using Foundation;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class Texture2D
    {
        private static Texture2D PlatformFromStream(GraphicsDevice graphicsDevice, Stream stream)
        {
            using (NSImage image = NSImage.FromStream(stream))
            {
                CGImage cgImage = image.CGImage;
                return PlatformFromStream(graphicsDevice, cgImage);
            }
        }

        private NSImage CreateImageFromTexture()
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
                                       false,
                                       CGColorRenderingIntent.Default);


            NSImage image = new NSImage(imageRef, new CGSize(Width, Height));

            provider.Dispose();
            colorSpace.Dispose();

            return image;
        }

        private void SaveNSImageToStream(Stream stream, int width, int height, NSBitmapImageFileType type)
        {
            using (NSImage image = CreateImageFromTexture())
            {
                var size = new CGSize(width, height);
                var imageRep = new NSBitmapImageRep(image.CGImage);

                imageRep.Size = size;
                NSData data = imageRep.RepresentationUsingTypeProperties(type);

                using (Stream imageStream = data.AsStream())
                    imageStream.CopyTo(stream);

                data.Dispose();
            }
        }

        private void PlatformSaveAsJpeg(Stream stream, int width, int height)
        {
            SaveNSImageToStream(stream, width, height, NSBitmapImageFileType.Jpeg);
        }

        private void PlatformSaveAsPng(Stream stream, int width, int height)
        {
            SaveNSImageToStream(stream, width, height, NSBitmapImageFileType.Png);
        }
    }
}
