using System.Collections.Generic;
using SharpFont;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics.Font
{
    internal class FontFaceCache
    {
        public Dictionary<uint, GlyphData> GlyphMaps;
        public Dictionary<uint, char> CharMap;

        public Face FontFace;

        public FontFaceCache(Face face)
        {
            GlyphMaps = new Dictionary<uint, GlyphData>();
            CharMap = new Dictionary<uint, char>();
            FontFace = face;
        }
    }
}

