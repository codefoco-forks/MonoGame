// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content
{
    internal class SpriteFontReader : ContentTypeReader<SpriteFont>
    {
        public SpriteFontReader()
        {
        }

        protected internal override SpriteFont Read(ContentReader input, SpriteFont existingInstance)
        {
            if (existingInstance != null)
            {
                // Read the texture into the existing texture instance
                input.ReadObject<Texture2D>(existingInstance.Texture);
                
                // discard the rest of the SpriteFont data as we are only reloading GPU resources for now
                input.ReadObject<List<Rectangle>>();
                input.ReadObject<List<Rectangle>>();
                input.ReadObject<List<char>>();
                input.ReadInt32();
                input.ReadSingle();
                input.ReadObject<List<Vector3>>();
                if (input.ReadBoolean())
                {
                    input.ReadChar();
                }
                if (input.BaseStream.Length > input.BaseStream.Position)
                {
                    input.ReadSingle();
                }

                return existingInstance;
            }
            else
            {
                // Create a fresh SpriteFont instance
                Texture2D texture = input.ReadObject<Texture2D>();
                List<Rectangle> glyphs = input.ReadObject<List<Rectangle>>();
                List<Rectangle> croppings = input.ReadObject<List<Rectangle>>();
                List<char> charMap = input.ReadObject<List<char>>();
                int lineSpacing = input.ReadInt32();
                float spacing = input.ReadSingle();

                Dictionary<char, Dictionary<char, short>> kerning = new Dictionary<char, Dictionary<char, short>>();
                List<char> leftKerningChars = input.ReadObject<List<char>>();

                foreach (char left in leftKerningChars)
                {
                    Dictionary<char, short> rightDistance = new Dictionary<char, short>();

                    List<char> rightChars = input.ReadObject<List<char>>();
                    List<short> distances = input.ReadObject<List<short>>();
                    for (int i = 0; i < rightChars.Count; i++)
                    {
                        char right = rightChars[i];
                        short distance = distances[i];

                        rightDistance[right] = distance;
                    }

                    kerning[left] = rightDistance;
                }

                List<Vector3> offsets = input.ReadObject<List<Vector3>>();
                char? defaultCharacter = null;
                float scale = 1f;
                if (input.ReadBoolean())
                {
                    defaultCharacter = new char?(input.ReadChar());
                }
                if (input.BaseStream.Length > input.BaseStream.Position)
                {
                    scale = input.ReadSingle();
                }
                if (input.BaseStream.Length > input.BaseStream.Position)
                {
                    scale = input.ReadSingle();
                }
                return new SpriteFont(texture,
                                      glyphs,
                                      croppings,
                                      charMap,
                                      lineSpacing,
                                      spacing,
                                      offsets,
                                      kerning,
                                      defaultCharacter,
                                      scale);
            }
        }
    }
}
