// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.IO;

#if !NETSTANDARD
using BigGustave;
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

            Png png = Png.Open(stream);

            int height = png.Height;
            int width = png.Width;

            uint[] textureData = new uint[height * width];

            unsafe
            {
                fixed (uint* baseAddress = &textureData[0])
                {
                    uint* pData = baseAddress;

                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            Pixel pixel = png.GetPixel(x, y);

                            uint alpha = pixel.A;
                            uint red = pixel.R;
                            uint green = pixel.G;
                            uint blue = pixel.B;

                            // Multiply alpha
                            red = (uint)(red * alpha / 255f);
                            green = (uint)(green * alpha / 255f);
                            blue = (uint)(blue * alpha / 255f);

                            alpha = alpha << 24;
                            blue = blue << 16;
                            green = green << 8;

                            uint pixelData = red |
                                         green |
                                         blue |
                                         alpha;

                            *pData = pixelData;
                            pData++;
                        }
                    }
                }
            }

            var texture = new Texture2D(graphicsDevice, width, height);
            texture.SetData(textureData);

            png = null;

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
