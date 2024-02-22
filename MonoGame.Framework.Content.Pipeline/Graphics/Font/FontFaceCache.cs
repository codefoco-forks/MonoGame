using System.Collections.Generic;
using SharpFont;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics.Font
{
    internal class FontFaceCache
    {
        public Dictionary<uint, GlyphData> GlyphMaps;
        public Face FontFace;

        public FontFaceCache(Face face)
        {
            GlyphMaps = new Dictionary<uint, GlyphData>();
            FontFace = face;
        }
    }
}

