// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Original code from SilverSprite Project
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;

namespace Microsoft.Xna.Framework.Graphics 
{

	public sealed class SpriteFont 
    {
		internal static class Errors 
        {
			public const string TextContainsUnresolvableCharacters =
				"Text contains characters that cannot be resolved by this SpriteFont.";
			public const string UnresolvableCharacter =
				"Character cannot be resolved by this SpriteFont.";
		}

        private readonly Glyph[] _glyphs;
        private readonly CharacterRegion[] _regions;
        private char? _defaultCharacter;
        private int _defaultGlyphIndex = -1;
        private readonly Dictionary<char, Dictionary<char, short>> _kerning;

		private readonly Texture2D _texture;

        private readonly float scale = 1f;

		/// <summary>
		/// All the glyphs in this SpriteFont.
		/// </summary>
		public Glyph[] Glyphs { get { return _glyphs; } }

		class CharComparer: IEqualityComparer<char>
		{
			public bool Equals(char x, char y)
			{
				return x == y;
			}

			public int GetHashCode(char b)
			{
				return (b);
			}

			static public readonly CharComparer Default = new CharComparer();
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="SpriteFont" /> class.
        /// </summary>
        /// <param name="texture">The font texture.</param>
        /// <param name="glyphBounds">The rectangles in the font texture containing letters.</param>
        /// <param name="cropping">The cropping rectangles, which are applied to the corresponding glyphBounds to calculate the bounds of the actual character.</param>
        /// <param name="characters">The characters.</param>
        /// <param name="lineSpacing">The line spacing (the distance from baseline to baseline) of the font.</param>
        /// <param name="spacing">The spacing (tracking) between characters in the font.</param>
        /// <param name="kerningAdvance">The letters kernings (X - left side bearing, Y - width and Z - right side bearing).</param>
        /// <param name="defaultCharacter">The character that will be substituted when a given character is not included in the font.</param>
        /// <param name="fontScale">The character that will be substituted when a given character is not included in the font.</param>
        public SpriteFont (Texture2D texture,
                           List<Rectangle> glyphBounds,
                           List<Rectangle> cropping,
                           List<char> characters,
                           int lineSpacing,
                           float spacing,
                           List<Vector3> offsets,
                           Dictionary<char, Dictionary<char, short>> kerningAdvance,
                           char? defaultCharacter,
                           float fontScale)
        {
            Characters = new ReadOnlyCollection<char>(characters.ToArray());
            _texture = texture;
            LineSpacing = lineSpacing;
            Spacing = spacing;
            _kerning = kerningAdvance;

            int count = characters.Count;
            _glyphs = new Glyph[count];
            var regions = new Stack<CharacterRegion>();
            for (var i = 0; i < characters.Count; i++) 
            {
                _glyphs[i] = new Glyph 
                {
                    BoundsInTexture = glyphBounds[i],
                    Cropping = cropping[i],
                    Character = characters[i],

                    OffsetX = offsets[i].X,
                    OffsetY = offsets[i].Y,
                    Advance = offsets[i].Z
                };
                
                if(regions.Count == 0 || characters[i] > (regions.Peek().End+1))
                {
                    // Start a new region
                    regions.Push(new CharacterRegion(characters[i], i));
                } 
                else if(characters[i] == (regions.Peek().End+1))
                {
                    var currentRegion = regions.Pop();
                    // include character in currentRegion
                    currentRegion.End++;
                    regions.Push(currentRegion);
                }
                else // characters[i] < (regions.Peek().End+1)
                {
                    throw new InvalidOperationException("Invalid SpriteFont. Character map must be in ascending order.");
                }
            }

            _regions = regions.ToArray();
            Array.Reverse(_regions);

            DefaultCharacter = defaultCharacter;
            scale = fontScale;
        }

        /// <summary>
        /// Gets the texture that this SpriteFont draws from.
        /// </summary>
        /// <remarks>Can be used to implement custom rendering of a SpriteFont</remarks>
        public Texture2D Texture { get { return _texture; } }

        /// <summary>
        /// Returns a copy of the dictionary containing the glyphs in this SpriteFont.
        /// </summary>
        /// <returns>A new Dictionary containing all of the glyphs in this SpriteFont</returns>
        /// <remarks>Can be used to calculate character bounds when implementing custom SpriteFont rendering.</remarks>
        public Dictionary<char, Glyph> GetGlyphs()
        {
            var glyphsDictionary = new Dictionary<char, Glyph>(_glyphs.Length, CharComparer.Default);
            foreach(var glyph in _glyphs)
                glyphsDictionary.Add(glyph.Character, glyph);
            return glyphsDictionary;
        }

		/// <summary>
		/// Gets a collection of the characters in the font.
		/// </summary>
		public ReadOnlyCollection<char> Characters { get; private set; }

		/// <summary>
		/// Gets or sets the character that will be substituted when a
		/// given character is not included in the font.
		/// </summary>
		public char? DefaultCharacter
        {
            get { return _defaultCharacter; }
            set
            {   
                // Get the default glyph index here once.
                if (value.HasValue)
                {
                    if(!TryGetGlyphIndex(value.Value, out _defaultGlyphIndex))
                        throw new ArgumentException(Errors.UnresolvableCharacter);
                }
                else
                    _defaultGlyphIndex = -1;

                _defaultCharacter = value;
            }
        }

		/// <summary>
		/// Gets or sets the line spacing (the distance from baseline
		/// to baseline) of the font.
		/// </summary>
		public int LineSpacing { get; set; }

		/// <summary>
		/// Gets or sets the spacing (tracking) between characters in
		/// the font.
		/// </summary>
		public float Spacing { get; set; }

        /// <summary>
        /// Font scale
        /// </summary>
        public float Scale
        {
            get
            {
                return scale;
            }
        }

        /// <summary>
        /// Returns the size of a string when rendered in this font.
        /// </summary>
        /// <param name="text">The text to measure.</param>
        /// <returns>The size, in pixels, of 'text' when rendered in
        /// this font.</returns>
        public Vector2 MeasureString(string text)
		{
			Vector2 size;
			MeasureString(text, out size);
			return size;
		}

		/// <summary>
		/// Returns the size of the contents of a StringBuilder when
		/// rendered in this font.
		/// </summary>
		/// <param name="text">The text to measure.</param>
		/// <returns>The size, in pixels, of 'text' when rendered in
		/// this font.</returns>
		public Vector2 MeasureString(StringBuilder text)
		{
			Vector2 size;
			MeasureString(text, out size);
			return size;
		}

        internal unsafe void MeasureString(StringBuilder text, out Vector2 size)
        {
            int count = text.Length;

            if (count == 0)
            {
                size = Vector2.Zero;
                return;
            }

            char prev_char = char.MinValue;
            char ch = text[0];
            int lineSpacing = LineSpacing + 1;

            fixed (Glyph* pGlyphs = Glyphs)
            {
                int currentGlyphIndex = GetGlyphIndexOrDefault(ch);
                Glyph* pCurrentGlyph = pGlyphs + currentGlyphIndex;

                Vector2 offset = new Vector2(pCurrentGlyph->Advance + Spacing, lineSpacing);

                if (ch == '\n')
                {
                    offset.Y += lineSpacing;
                }

                for (int i = 1; i < count; ++i)
                {
                    ch = text[i];

                    if (ch == '\r')
                        continue;

                    if (ch == '\n')
                    {
                        offset.X = 0;
                        offset.Y += lineSpacing;
                        continue;
                    }

                    currentGlyphIndex = GetGlyphIndexOrDefault(ch);
                    Debug.Assert(currentGlyphIndex >= 0 && currentGlyphIndex < Glyphs.Length, "currentGlyphIndex was outside the bounds of the array.");
                    pCurrentGlyph = pGlyphs + currentGlyphIndex;

                    int kerning = GetKerning(prev_char, ch);
                    offset.X += pCurrentGlyph->Advance + kerning + Spacing;

                    prev_char = ch;
                }

                size.X = (offset.X + 4) * scale;
                size.Y = (offset.Y + 4) * scale;
            }
        }

        public void MeasureString(string text, out Vector2 size)
        {
            MeasureString(text, out size, 0, 0f);
        }

        public unsafe void MeasureString(string text, out Vector2 size, int maxLength, float preferredWidth)
        {
            int count = text.Length;

            if (count == 0)
            {
                size = Vector2.Zero;
                return;
            }

            if (maxLength != 0 && maxLength < count)
                count = maxLength;

            char prev_char = char.MinValue;
            char ch = text[0];
            int lineSpacing = LineSpacing + 1;

            fixed (Glyph* pGlyphs = Glyphs)
            {
                int currentGlyphIndex = GetGlyphIndexOrDefault(ch);
                Glyph* pCurrentGlyph = pGlyphs + currentGlyphIndex;

                Vector2 offset = new Vector2(0, lineSpacing);
                float width = pCurrentGlyph->Advance;

                if (ch == '\n')
                {
                    offset.Y += lineSpacing;
                }

                for (int i = 1; i < count; ++i)
                {
                    ch = text[i];

                    if (ch == '\r')
                        continue;

                    if (ch == '\n')
                    {
                        if (offset.X < width)
                            offset.X = width;

                        width = 0;
                        offset.Y += lineSpacing;
                        continue;
                    }

                    currentGlyphIndex = GetGlyphIndexOrDefault(ch);
                    Debug.Assert(currentGlyphIndex >= 0 && currentGlyphIndex < Glyphs.Length, "currentGlyphIndex was outside the bounds of the array.");
                    pCurrentGlyph = pGlyphs + currentGlyphIndex;

                    int kerning = GetKerning(prev_char, ch);
                    width += pCurrentGlyph->Advance + kerning + Spacing;

                    prev_char = ch;
                }

                if (offset.X < width)
                    offset.X = width;

                size.X = (offset.X + 4) * scale;
                size.Y = (offset.Y + 4) * scale;
            }
        }

        internal unsafe bool TryGetGlyphIndex(char c, out int index)
        {
            fixed (CharacterRegion* pRegions = _regions)
            {
                // Get region Index 
                int regionIdx = -1;
                int l = 0;
                int r = _regions.Length - 1;
                int m = (c * r) / (pRegions[0].Start + pRegions[r].Start);

                while (l <= r)
                {
                    Debug.Assert(m >= 0 && m < _regions.Length, "Index was outside the bounds of the array.");
                    if (pRegions[m].End < c)
                    {
                        l = m + 1;
                    }
                    else if (pRegions[m].Start > c)
                    {
                        r = m - 1;
                    }
                    else
                    {
                        regionIdx = m;
                        break;
                    }

                    m = (l + r) >> 1;
                }

                if (regionIdx == -1)
                {
                    index = -1;
                    return false;
                }

                index = pRegions[regionIdx].StartIndex + (c - pRegions[regionIdx].Start);
            }

            return true;
        }

        internal int GetGlyphIndexOrDefault(char c)
        {
            int glyphIdx;
            if (!TryGetGlyphIndex(c, out glyphIdx))
            {
                if (_defaultGlyphIndex == -1)
                    throw new ArgumentException(Errors.TextContainsUnresolvableCharacters, "text");

                return _defaultGlyphIndex;
            }
            else
                return glyphIdx;
        }

        /// <summary>
        /// Get Kerning distance for a pair of chars
        /// </summary>
        /// <param name="prev_char"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public int GetKerning(char prev_char, char c)
        {
            if (prev_char == char.MinValue || c == char.MinValue)
                return 0;

            Dictionary<char, short> rightDistance;
            if (!_kerning.TryGetValue(prev_char, out rightDistance))
                return 0;

            short distance;
            if (!rightDistance.TryGetValue(c, out distance))
                return 0;

            return distance;
        }

        /// <summary>
        /// Glyph index and offset
        /// </summary>
        public struct GlyphRect
        {
            public Vector2 texCoordTL;
            public Vector2 texCoordBR;
            public Vector2 pos;
            public Vector2 size;
        }

        /// <summary>
        /// Struct that defines the spacing, Kerning, and bounds of a character.
        /// </summary>
        /// <remarks>Provides the data necessary to implement custom SpriteFont rendering.</remarks>
        public struct Glyph 
        {
            /// <summary>
            /// The char associated with this glyph.
            /// </summary>
            public char Character;
            /// <summary>
            /// Rectangle in the font texture where this letter exists.
            /// </summary>
            public Rectangle BoundsInTexture;
            /// <summary>
            /// Cropping applied to the BoundsInTexture to calculate the bounds of the actual character.
            /// </summary>
            public Rectangle Cropping;

            /// <summary>
            /// Glyph offset X
            /// </summary>
            public float OffsetX;

            /// <summary>
            /// Glyph offset X
            /// </summary>
            public float OffsetY;

            /// <summary>
            /// How much should be advance for the next glyph
            /// </summary>
            public float Advance;

            public static readonly Glyph Empty = new Glyph();

            public override string ToString ()
            {
                return "CharacterIndex=" + Character + ", Glyph=" + BoundsInTexture + ", Cropping=" + Cropping + ", OffsetX = " + OffsetX + ", OffsetY = " + OffsetY + ", Advance = " + Advance;
            }
        }

        private struct CharacterRegion
        {
            public char Start;
            public char End;
            public int StartIndex;

            public CharacterRegion(char start, int startIndex)
            {
                this.Start = start;                
                this.End = start;
                this.StartIndex = startIndex;
            }
        }
	}
}
