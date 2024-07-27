// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Text;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Helper class for drawing text strings and sprites in one or more optimized batches.
    /// </summary>
	public class SpriteBatch : GraphicsResource
    {
        #region Private Fields
        readonly SpriteBatcher _batcher;

        SpriteSortMode _sortMode;
        BlendState _blendState;
        SamplerState _samplerState;
        DepthStencilState _depthStencilState;
        RasterizerState _rasterizerState;
        Effect _effect;
        bool _beginCalled;

        SpriteEffect _spriteEffect;
        readonly EffectPass _spritePass;

        Rectangle _tempRect = new Rectangle(0, 0, 0, 0);
        Vector2 _texCoordTL = new Vector2(0, 0);
        Vector2 _texCoordBR = new Vector2(0, 0);
        #endregion

        /// <summary>
        /// Constructs a <see cref="SpriteBatch"/>.
        /// </summary>
        /// <param name="graphicsDevice">The <see cref="GraphicsDevice"/>, which will be used for sprite rendering.</param>        
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="graphicsDevice"/> is null.</exception>
        public SpriteBatch(GraphicsDevice graphicsDevice) : this(graphicsDevice, 0)
        {
        }

        /// <summary>
        /// Constructs a <see cref="SpriteBatch"/>.
        /// </summary>
        /// <param name="graphicsDevice">The <see cref="GraphicsDevice"/>, which will be used for sprite rendering.</param>
        /// <param name="capacity">The initial capacity of the internal array holding batch items (the value will be rounded to the next multiple of 64).</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="graphicsDevice"/> is null.</exception>
        public SpriteBatch(GraphicsDevice graphicsDevice, int capacity)
        {
            if (graphicsDevice == null)
            {
                throw new ArgumentNullException("graphicsDevice", FrameworkResources.ResourceCreationWhenDeviceIsNull);
            }

            this.GraphicsDevice = graphicsDevice;

            _spriteEffect = new SpriteEffect(graphicsDevice);
            _spritePass = _spriteEffect.CurrentTechnique.Passes[0];

            _batcher = new SpriteBatcher(graphicsDevice, capacity);

            _beginCalled = false;
        }

        /// <summary>
        /// Constructs a <see cref="SpriteBatch"/>.
        /// </summary>
        /// <param name="graphicsDevice">The <see cref="GraphicsDevice"/>, which will be used for sprite rendering.</param>
        /// <param name="capacity">The initial capacity of the internal array holding batch items (the value will be rounded to the next multiple of 64).</param>
        /// <param name="projection">The initial projection matrix.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="graphicsDevice"/> is null.</exception>
        public SpriteBatch(GraphicsDevice graphicsDevice, int capacity, ref Matrix projection)
        {
            if (graphicsDevice == null)
            {
                throw new ArgumentNullException("graphicsDevice", FrameworkResources.ResourceCreationWhenDeviceIsNull);
            }

            GraphicsDevice = graphicsDevice;

            _spriteEffect = new SpriteEffect(graphicsDevice, ref projection);
            _spritePass = _spriteEffect.CurrentTechnique.Passes[0];

            _batcher = new SpriteBatcher(graphicsDevice, capacity);

            _beginCalled = false;
        }

        /// <summary>
        /// World matrix for this SpriteBatch
        /// </summary>
        public Matrix World
        {
            get
            {
                return _spriteEffect.World;
            }
            set
            {
                _spriteEffect.World = value;
            }
        }

        /// <summary>
        /// Projection matrix for this SpriteBatch
        /// </summary>
        public Matrix Projection
        {
            get
            {
                return _spriteEffect.Projection;
            }
            set
            {
                _spriteEffect.Projection = value;
            }
        }

        /// <summary>
        /// Begins a new sprite and text batch with the specified render state.
        /// </summary>
        /// <param name="sortMode">The drawing order for sprite and text drawing. <see cref="SpriteSortMode.Deferred"/> by default.</param>
        /// <param name="blendState">State of the blending. Uses <see cref="BlendState.AlphaBlend"/> if null.</param>
        /// <param name="samplerState">State of the sampler. Uses <see cref="SamplerState.LinearClamp"/> if null.</param>
        /// <param name="depthStencilState">State of the depth-stencil buffer. Uses <see cref="DepthStencilState.None"/> if null.</param>
        /// <param name="rasterizerState">State of the rasterization. Uses <see cref="RasterizerState.CullCounterClockwise"/> if null.</param>
        /// <param name="effect">A custom <see cref="Effect"/> to override the default sprite effect. Uses default sprite effect if null.</param>
        /// <exception cref="InvalidOperationException">Thrown if <see cref="Begin"/> is called next time without previous <see cref="End"/>.</exception>
        /// <remarks>This method uses optional parameters.</remarks>
        /// <remarks>The <see cref="Begin"/> Begin should be called before drawing commands, and you cannot call it again before subsequent <see cref="End"/>.</remarks>
        public void Begin
        (
             SpriteSortMode sortMode = SpriteSortMode.Deferred,
             BlendState blendState = null,
             SamplerState samplerState = null,
             DepthStencilState depthStencilState = null,
             RasterizerState rasterizerState = null,
             Effect effect = null
        )
        {
            if (_beginCalled)
                throw new InvalidOperationException("Begin cannot be called again until End has been successfully called.");

            // defaults
            _sortMode = sortMode;
            _blendState = blendState ?? BlendState.AlphaBlend;
            _samplerState = samplerState ?? SamplerState.LinearClamp;
            _depthStencilState = depthStencilState ?? DepthStencilState.None;
            _rasterizerState = rasterizerState ?? RasterizerState.CullCounterClockwise;
            _effect = effect;

            // Setup things now so a user can change them.
            if (sortMode == SpriteSortMode.Immediate)
            {
                Setup();
            }

            _beginCalled = true;
        }

        /// <summary>
        /// Flushes all batched text and sprites to the screen.
        /// </summary>
        /// <remarks>This command should be called after <see cref="Begin"/> and drawing commands.</remarks>
		public void End()
        {
            if (!_beginCalled)
                throw new InvalidOperationException("Begin must be called before calling End.");

            _beginCalled = false;

            if (_sortMode != SpriteSortMode.Immediate)
                Setup();

            _batcher.DrawBatch(_sortMode, _effect);
        }

        void Setup()
        {
            var gd = GraphicsDevice;
            gd.BlendState = _blendState;
            gd.DepthStencilState = _depthStencilState;
            gd.RasterizerState = _rasterizerState;
            gd.SamplerStates[0] = _samplerState;

            _spritePass.Apply();
        }

        void CheckValid(Texture2D texture)
        {
            if (texture == null)
                throw new ArgumentNullException("texture");
            if (!_beginCalled)
                throw new InvalidOperationException("Draw was called, but Begin has not yet been called. Begin must be called successfully before you can call Draw.");
        }

        void CheckValid(SpriteFont spriteFont, string text)
        {
            if (spriteFont == null)
                throw new ArgumentNullException("spriteFont");
            if (text == null)
                throw new ArgumentNullException("text");
            if (!_beginCalled)
                throw new InvalidOperationException("DrawString was called, but Begin has not yet been called. Begin must be called successfully before you can call DrawString.");
        }

        void CheckValid(SpriteFont spriteFont, StringBuilder text)
        {
            if (spriteFont == null)
                throw new ArgumentNullException("spriteFont");
            if (text == null)
                throw new ArgumentNullException("text");
            if (!_beginCalled)
                throw new InvalidOperationException("DrawString was called, but Begin has not yet been called. Begin must be called successfully before you can call DrawString.");
        }

        /// <summary>
        /// Submit a sprite for drawing in the current batch.
        /// </summary>
        /// <param name="texture">A texture.</param>
        /// <param name="position">The drawing location on screen.</param>
        /// <param name="sourceRectangle">An optional region on the texture which will be rendered. If null - draws full texture.</param>
        /// <param name="color">A color mask.</param>
        /// <param name="rotation">A rotation of this sprite.</param>
        /// <param name="origin">Center of the rotation. 0,0 by default.</param>
        /// <param name="scale">A scaling of this sprite.</param>
        /// <param name="effects">Modificators for drawing. Can be combined.</param>
        /// <param name="layerDepth">A depth of the layer of this sprite.</param>
		public void Draw(Texture2D texture,
                Vector2 position,
                Rectangle? sourceRectangle,
                Color color,
                float rotation,
                Vector2 origin,
                Vector2 scale,
                SpriteEffects effects,
                float layerDepth)
        {
            CheckValid(texture);

            var item = _batcher.CreateBatchItem();
            item.Texture = texture;

            // set SortKey based on SpriteSortMode.
            switch (_sortMode)
            {
                // Comparison of Texture objects.
                case SpriteSortMode.Texture:
                    item.SortKey = texture.SortingKey;
                    break;
                // Comparison of Depth
                case SpriteSortMode.FrontToBack:
                    item.SortKey = layerDepth;
                    break;
                // Comparison of Depth in reverse
                case SpriteSortMode.BackToFront:
                    item.SortKey = -layerDepth;
                    break;
            }

            origin = origin * scale;

            float w, h;
            if (sourceRectangle.HasValue)
            {
                var srcRect = sourceRectangle.GetValueOrDefault();
                w = srcRect.Width * scale.X;
                h = srcRect.Height * scale.Y;
                _texCoordTL.X = srcRect.X * texture.TexelWidth;
                _texCoordTL.Y = srcRect.Y * texture.TexelHeight;
                _texCoordBR.X = (srcRect.X + srcRect.Width) * texture.TexelWidth;
                _texCoordBR.Y = (srcRect.Y + srcRect.Height) * texture.TexelHeight;
            }
            else
            {
                w = texture.Width * scale.X;
                h = texture.Height * scale.Y;
                _texCoordTL = Vector2.Zero;
                _texCoordBR = Vector2.One;
            }

            if ((effects & SpriteEffects.FlipVertically) != 0)
            {
                var temp = _texCoordBR.Y;
                _texCoordBR.Y = _texCoordTL.Y;
                _texCoordTL.Y = temp;
            }
            if ((effects & SpriteEffects.FlipHorizontally) != 0)
            {
                var temp = _texCoordBR.X;
                _texCoordBR.X = _texCoordTL.X;
                _texCoordTL.X = temp;
            }

            if (rotation == 0f)
            {
                item.Set(position.X - origin.X,
                        position.Y - origin.Y,
                        w,
                        h,
                        color,
                        _texCoordTL,
                        _texCoordBR,
                        layerDepth);
            }
            else
            {
                item.Set(position.X,
                        position.Y,
                        -origin.X,
                        -origin.Y,
                        w,
                        h,
                        (float)Math.Sin(rotation),
                        (float)Math.Cos(rotation),
                        color,
                        _texCoordTL,
                        _texCoordBR,
                        layerDepth);
            }

            FlushIfNeeded();
        }

        /// <summary>
        /// Submit a sprite for drawing in the current batch.
        /// </summary>
        /// <param name="texture">A texture.</param>
        /// <param name="position">The drawing location on screen.</param>
        /// <param name="sourceRectangle">An optional region on the texture which will be rendered. If null - draws full texture.</param>
        /// <param name="color">A color mask.</param>
        /// <param name="rotation">A rotation of this sprite.</param>
        /// <param name="origin">Center of the rotation. 0,0 by default.</param>
        /// <param name="scale">A scaling of this sprite.</param>
        /// <param name="effects">Modificators for drawing. Can be combined.</param>
        /// <param name="layerDepth">A depth of the layer of this sprite.</param>
		public void Draw(Texture2D texture,
                Vector2 position,
                Rectangle? sourceRectangle,
                Color color,
                float rotation,
                Vector2 origin,
                float scale,
                SpriteEffects effects,
                float layerDepth)
        {
            var scaleVec = new Vector2(scale, scale);
            Draw(texture, position, sourceRectangle, color, rotation, origin, scaleVec, effects, layerDepth);
        }

        /// <summary>
        /// Submit a sprite for drawing in the current batch.
        /// </summary>
        /// <param name="texture">A texture.</param>
        /// <param name="destinationRectangle">The drawing bounds on screen.</param>
        /// <param name="sourceRectangle">An optional region on the texture which will be rendered. If null - draws full texture.</param>
        /// <param name="color">A color mask.</param>
        /// <param name="rotation">A rotation of this sprite.</param>
        /// <param name="origin">Center of the rotation. 0,0 by default.</param>
        /// <param name="effects">Modificators for drawing. Can be combined.</param>
        /// <param name="layerDepth">A depth of the layer of this sprite.</param>
		public void Draw(Texture2D texture,
            Rectangle destinationRectangle,
            Rectangle? sourceRectangle,
            Color color,
            float rotation,
            Vector2 origin,
            SpriteEffects effects,
            float layerDepth)
        {
            CheckValid(texture);

            var item = _batcher.CreateBatchItem();
            item.Texture = texture;

            // set SortKey based on SpriteSortMode.
            switch (_sortMode)
            {
                // Comparison of Texture objects.
                case SpriteSortMode.Texture:
                    item.SortKey = texture.SortingKey;
                    break;
                // Comparison of Depth
                case SpriteSortMode.FrontToBack:
                    item.SortKey = layerDepth;
                    break;
                // Comparison of Depth in reverse
                case SpriteSortMode.BackToFront:
                    item.SortKey = -layerDepth;
                    break;
            }

            if (sourceRectangle.HasValue)
            {
                var srcRect = sourceRectangle.GetValueOrDefault();
                _texCoordTL.X = srcRect.X * texture.TexelWidth;
                _texCoordTL.Y = srcRect.Y * texture.TexelHeight;
                _texCoordBR.X = (srcRect.X + srcRect.Width) * texture.TexelWidth;
                _texCoordBR.Y = (srcRect.Y + srcRect.Height) * texture.TexelHeight;

                if (srcRect.Width != 0)
                    origin.X = origin.X * (float)destinationRectangle.Width / (float)srcRect.Width;
                else
                    origin.X = origin.X * (float)destinationRectangle.Width * texture.TexelWidth;
                if (srcRect.Height != 0)
                    origin.Y = origin.Y * (float)destinationRectangle.Height / (float)srcRect.Height;
                else
                    origin.Y = origin.Y * (float)destinationRectangle.Height * texture.TexelHeight;
            }
            else
            {
                _texCoordTL = Vector2.Zero;
                _texCoordBR = Vector2.One;

                origin.X = origin.X * (float)destinationRectangle.Width * texture.TexelWidth;
                origin.Y = origin.Y * (float)destinationRectangle.Height * texture.TexelHeight;
            }

            if ((effects & SpriteEffects.FlipVertically) != 0)
            {
                var temp = _texCoordBR.Y;
                _texCoordBR.Y = _texCoordTL.Y;
                _texCoordTL.Y = temp;
            }
            if ((effects & SpriteEffects.FlipHorizontally) != 0)
            {
                var temp = _texCoordBR.X;
                _texCoordBR.X = _texCoordTL.X;
                _texCoordTL.X = temp;
            }

            if (rotation == 0f)
            {
                item.Set(destinationRectangle.X - origin.X,
                        destinationRectangle.Y - origin.Y,
                        destinationRectangle.Width,
                        destinationRectangle.Height,
                        color,
                        _texCoordTL,
                        _texCoordBR,
                        layerDepth);
            }
            else
            {
                item.Set(destinationRectangle.X,
                        destinationRectangle.Y,
                        -origin.X,
                        -origin.Y,
                        destinationRectangle.Width,
                        destinationRectangle.Height,
                        (float)Math.Sin(rotation),
                        (float)Math.Cos(rotation),
                        color,
                        _texCoordTL,
                        _texCoordBR,
                        layerDepth);
            }

            FlushIfNeeded();
        }

        // Mark the end of a draw operation for Immediate SpriteSortMode.
        internal void FlushIfNeeded()
        {
            if (_sortMode == SpriteSortMode.Immediate)
            {
                _batcher.DrawBatch(_sortMode, _effect);
            }
        }

        /// <summary>
        /// Submit a sprite for drawing in the current batch.
        /// </summary>
        /// <param name="texture">A texture.</param>
        /// <param name="position">The drawing location on screen.</param>
        /// <param name="sourceRectangle">An optional region on the texture which will be rendered. If null - draws full texture.</param>
        /// <param name="color">A color mask.</param>
		public void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color)
        {
            CheckValid(texture);

            var item = _batcher.CreateBatchItem();
            item.Texture = texture;

            // set SortKey based on SpriteSortMode.
            item.SortKey = _sortMode == SpriteSortMode.Texture ? texture.SortingKey : 0;

            Vector2 size;

            if (sourceRectangle.HasValue)
            {
                var srcRect = sourceRectangle.GetValueOrDefault();
                size = new Vector2(srcRect.Width, srcRect.Height);
                _texCoordTL.X = srcRect.X * texture.TexelWidth;
                _texCoordTL.Y = srcRect.Y * texture.TexelHeight;
                _texCoordBR.X = (srcRect.X + srcRect.Width) * texture.TexelWidth;
                _texCoordBR.Y = (srcRect.Y + srcRect.Height) * texture.TexelHeight;
            }
            else
            {
                size = new Vector2(texture.width, texture.height);
                _texCoordTL = Vector2.Zero;
                _texCoordBR = Vector2.One;
            }

            item.Set(position.X,
                     position.Y,
                     size.X,
                     size.Y,
                     color,
                     _texCoordTL,
                     _texCoordBR,
                     0);

            FlushIfNeeded();
        }

        /// <summary>
        /// Submit a sprite for drawing in the current batch.
        /// </summary>
        /// <param name="texture">A texture.</param>
        /// <param name="destinationRectangle">The drawing bounds on screen.</param>
        /// <param name="sourceRectangle">An optional region on the texture which will be rendered. If null - draws full texture.</param>
        /// <param name="color">A color mask.</param>
		public void Draw(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color)
        {
            CheckValid(texture);

            var item = _batcher.CreateBatchItem();
            item.Texture = texture;

            // set SortKey based on SpriteSortMode.
            item.SortKey = _sortMode == SpriteSortMode.Texture ? texture.SortingKey : 0;

            if (sourceRectangle.HasValue)
            {
                var srcRect = sourceRectangle.GetValueOrDefault();
                _texCoordTL.X = srcRect.X * texture.TexelWidth;
                _texCoordTL.Y = srcRect.Y * texture.TexelHeight;
                _texCoordBR.X = (srcRect.X + srcRect.Width) * texture.TexelWidth;
                _texCoordBR.Y = (srcRect.Y + srcRect.Height) * texture.TexelHeight;
            }
            else
            {
                _texCoordTL = Vector2.Zero;
                _texCoordBR = Vector2.One;
            }

            item.Set(destinationRectangle.X,
                     destinationRectangle.Y,
                     destinationRectangle.Width,
                     destinationRectangle.Height,
                     color,
                     _texCoordTL,
                     _texCoordBR,
                     0);

            FlushIfNeeded();
        }

        /// <summary>
        /// Submit a sprite for drawing in the current batch.
        /// </summary>
        /// <param name="texture">A texture.</param>
        /// <param name="position">The drawing location on screen.</param>
        /// <param name="color">A color mask.</param>
		public void Draw(Texture2D texture, Vector2 position, Color color)
        {
            CheckValid(texture);

            var item = _batcher.CreateBatchItem();
            item.Texture = texture;

            // set SortKey based on SpriteSortMode.
            item.SortKey = _sortMode == SpriteSortMode.Texture ? texture.SortingKey : 0;

            item.Set(position.X,
                     position.Y,
                     texture.Width,
                     texture.Height,
                     color,
                     Vector2.Zero,
                     Vector2.One,
                     0);

            FlushIfNeeded();
        }

        /// <summary>
        /// Draw a texture with given top, left, bottom, right vetices
        /// </summary>
        /// <param name="texture">Texture to draw</param>
        /// <param name="textureBL">texture coordinate of bottom left source in texel (0-1)</param>
        /// <param name="textureTR">texture coordinate of top right source in texel (0-1)</param>
        /// <param name="topLeft"></param>
        /// <param name="bottomLeft"></param>
        /// <param name="bottomRight"></param>
        /// <param name="topRight"></param>
        /// <param name="colorTL"></param>
        /// <param name="colorBL"></param>
        /// <param name="colorBR"></param>
        /// <param name="colorTR"></param>
        public void Draw(Texture2D texture,
                         ref Vector2 textureBL,
                         ref Vector2 textureTR,
                         ref Vector3 topLeft,
                         ref Vector3 bottomLeft,
                         ref Vector3 bottomRight,
                         ref Vector3 topRight,
                         Color colorTL,
                         Color colorBL,
                         Color colorBR,
                         Color colorTR)
        {
            var item = _batcher.CreateBatchItem();
            item.Texture = texture;

            item.vertexTL.Position = topLeft;
            item.vertexTL.Color = colorTL;
            item.vertexTL.TextureCoordinate.X = textureBL.X;
            item.vertexTL.TextureCoordinate.Y = textureTR.Y;

            item.vertexBL.Position = bottomLeft;
            item.vertexBL.Color = colorBL;
            item.vertexBL.TextureCoordinate = textureBL;

            item.vertexBR.Position = bottomRight;
            item.vertexBR.Color = colorBR;
            item.vertexBR.TextureCoordinate.X = textureTR.X;
            item.vertexBR.TextureCoordinate.Y = textureBL.Y;

            item.vertexTR.Position = topRight;
            item.vertexTR.Color = colorTR;
            item.vertexTR.TextureCoordinate = textureTR;
        }

        /// <summary>
        /// Draw a texture with given top, left, bottom, right vetices
        /// </summary>
        /// <param name="texture">Texture to draw</param>
        /// <param name="textureBL">texture coordinate of bottom left source in texel (0-1)</param>
        /// <param name="textureTR">texture coordinate of top right source in texel (0-1)</param>
        /// <param name="topLeft"></param>
        /// <param name="bottomLeft"></param>
        /// <param name="bottomRight"></param>
        /// <param name="topRight"></param>
        /// <param name="colorTL"></param>
        /// <param name="colorBL"></param>
        /// <param name="colorBR"></param>
        /// <param name="colorTR"></param>
        public void Draw(Texture2D texture,
                         ref Vector2 textureBL,
                         ref Vector2 textureTR,
                         ref Vector3 topLeft,
                         ref Vector3 bottomLeft,
                         ref Vector3 bottomRight,
                         ref Vector3 topRight,
                         Color color)
        {
            var item = _batcher.CreateBatchItem();
            item.Texture = texture;

            item.vertexTL.Position = topLeft;
            item.vertexTL.Color = color;
            item.vertexTL.TextureCoordinate.X = textureBL.X;
            item.vertexTL.TextureCoordinate.Y = textureTR.Y;

            item.vertexBL.Position = bottomLeft;
            item.vertexBL.Color = color;
            item.vertexBL.TextureCoordinate = textureBL;

            item.vertexBR.Position = bottomRight;
            item.vertexBR.Color = color;
            item.vertexBR.TextureCoordinate.X = textureTR.X;
            item.vertexBR.TextureCoordinate.Y = textureBL.Y;

            item.vertexTR.Position = topRight;
            item.vertexTR.Color = color;
            item.vertexTR.TextureCoordinate = textureTR;
        }

        /// <summary>
        /// Draw quad using indexes array
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="textureBL"></param>
        /// <param name="textureTR"></param>
        /// <param name="quad"></param>
        /// <param name="indexes"></param>
        /// <param name="color"></param>
        public void Draw(Texture2D texture,
                         ref Vector2 textureBL,
                         ref Vector2 textureTR,
                         Vector3[] quad,
                         int[] indexes,
                         Color color)
        {
            var item = _batcher.CreateBatchItem();
            item.Texture = texture;

            item.vertexTL.Position = quad[indexes[0]];
            item.vertexTL.Color = color;
            item.vertexTL.TextureCoordinate.X = textureBL.X;
            item.vertexTL.TextureCoordinate.Y = textureTR.Y;

            item.vertexBL.Position = quad[indexes[1]];
            item.vertexBL.Color = color;
            item.vertexBL.TextureCoordinate = textureBL;

            item.vertexBR.Position = quad[indexes[2]];
            item.vertexBR.Color = color;
            item.vertexBR.TextureCoordinate.X = textureTR.X;
            item.vertexBR.TextureCoordinate.Y = textureBL.Y;

            item.vertexTR.Position = quad[indexes[3]];
            item.vertexTR.Color = color;
            item.vertexTR.TextureCoordinate = textureTR;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="topLeft"></param>
        /// <param name="bottomLeft"></param>
        /// <param name="bottomRight"></param>
        /// <param name="topRight"></param>
        public void Draw(Texture2D texture,
                         ref Vector3 topLeft,
                         ref Vector3 bottomLeft,
                         ref Vector3 bottomRight,
                         ref Vector3 topRight
                         )
        {
            var item = _batcher.CreateBatchItem();
            item.Texture = texture;

            item.vertexTL.Position = topLeft;
            item.vertexTL.Color = Color.White;
            item.vertexTL.TextureCoordinate.X = 0;
            item.vertexTL.TextureCoordinate.Y = 1;

            item.vertexBL.Position = bottomLeft;
            item.vertexBL.Color = Color.White;
            item.vertexBL.TextureCoordinate = Vector2.Zero;

            item.vertexBR.Position = bottomRight;
            item.vertexBR.Color = Color.White;
            item.vertexBR.TextureCoordinate.X = 1;
            item.vertexBR.TextureCoordinate.Y = 0;

            item.vertexTR.Position = topRight;
            item.vertexTR.Color = Color.White;
            item.vertexTR.TextureCoordinate = Vector2.One;
        }

        /// <summary>
        /// Submit a sprite for drawing in the current batch.
        /// </summary>
        /// <param name="texture">A texture.</param>
        /// <param name="destinationRectangle">The drawing bounds on screen.</param>
        /// <param name="color">A color mask.</param>
        public void Draw(Texture2D texture, Rectangle destinationRectangle, Color color)
        {
            CheckValid(texture);

            var item = _batcher.CreateBatchItem();
            item.Texture = texture;

            // set SortKey based on SpriteSortMode.
            item.SortKey = _sortMode == SpriteSortMode.Texture ? texture.SortingKey : 0;

            item.Set(destinationRectangle.X,
                     destinationRectangle.Y,
                     destinationRectangle.Width,
                     destinationRectangle.Height,
                     color,
                     Vector2.Zero,
                     Vector2.One,
                     0);

            FlushIfNeeded();
        }

        /// <summary>
        /// Submit a text string of sprites for drawing in the current batch.
        /// </summary>
        /// <param name="spriteFont">A font.</param>
        /// <param name="text">The text which will be drawn.</param>
        /// <param name="position">The drawing location on screen.</param>
        /// <param name="color">A color mask.</param>
		public unsafe void DrawString(SpriteFont spriteFont, string text, Vector2 position, Color color)
        {
            CheckValid(spriteFont, text);

            float sortKey = (_sortMode == SpriteSortMode.Texture) ? spriteFont.Texture.SortingKey : 0;

            var offset = Vector2.Zero;
            var firstGlyphOfLine = true;

            fixed (SpriteFont.Glyph* pGlyphs = spriteFont.Glyphs)
                for (var i = 0; i < text.Length; ++i)
                {
                    var c = text[i];

                    if (c == '\r')
                        continue;

                    if (c == '\n')
                    {
                        offset.X = 0;
                        offset.Y += spriteFont.LineSpacing;
                        firstGlyphOfLine = true;
                        continue;
                    }

                    var currentGlyphIndex = spriteFont.GetGlyphIndexOrDefault(c);
                    var pCurrentGlyph = pGlyphs + currentGlyphIndex;

                    // The first character on a line might have a negative left side bearing.
                    // In this scenario, SpriteBatch/SpriteFont normally offset the text to the right,
                    //  so that text does not hang off the left side of its rectangle.
                    if (firstGlyphOfLine)
                    {
                        offset.X = Math.Max(pCurrentGlyph->LeftSideBearing, 0);
                        firstGlyphOfLine = false;
                    }
                    else
                    {
                        offset.X += spriteFont.Spacing + pCurrentGlyph->LeftSideBearing;
                    }

                    var p = offset;
                    p.X += pCurrentGlyph->Cropping.X;
                    p.Y += pCurrentGlyph->Cropping.Y;
                    p += position;

                    var item = _batcher.CreateBatchItem();
                    item.Texture = spriteFont.Texture;
                    item.SortKey = sortKey;

                    _texCoordTL.X = pCurrentGlyph->BoundsInTexture.X * spriteFont.Texture.TexelWidth;
                    _texCoordTL.Y = pCurrentGlyph->BoundsInTexture.Y * spriteFont.Texture.TexelHeight;
                    _texCoordBR.X = (pCurrentGlyph->BoundsInTexture.X + pCurrentGlyph->BoundsInTexture.Width) * spriteFont.Texture.TexelWidth;
                    _texCoordBR.Y = (pCurrentGlyph->BoundsInTexture.Y + pCurrentGlyph->BoundsInTexture.Height) * spriteFont.Texture.TexelHeight;

                    item.Set(p.X,
                             p.Y,
                             pCurrentGlyph->BoundsInTexture.Width,
                             pCurrentGlyph->BoundsInTexture.Height,
                             color,
                             _texCoordTL,
                             _texCoordBR,
                             0);

                    offset.X += pCurrentGlyph->Width + pCurrentGlyph->RightSideBearing;
                }

            // We need to flush if we're using Immediate sort mode.
            FlushIfNeeded();
        }

        /// <summary>
        /// Submit a text string of sprites for drawing in the current batch.
        /// </summary>
        /// <param name="spriteFont">A font.</param>
        /// <param name="text">The text which will be drawn.</param>
        /// <param name="position">The drawing location on screen.</param>
        /// <param name="color">A color mask.</param>
        /// <param name="rotation">A rotation of this string.</param>
        /// <param name="origin">Center of the rotation. 0,0 by default.</param>
        /// <param name="scale">A scaling of this string.</param>
        /// <param name="effects">Modificators for drawing. Can be combined.</param>
        /// <param name="layerDepth">A depth of the layer of this string.</param>
		public void DrawString(
            SpriteFont spriteFont, string text, Vector2 position, Color color,
            float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
        {
            var scaleVec = new Vector2(scale, scale);
            DrawString(spriteFont, text, position, color, rotation, origin, scaleVec, effects, layerDepth);
        }

        /// <summary>
        /// Submit a text string of sprites for drawing in the current batch.
        /// </summary>
        /// <param name="spriteFont">A font.</param>
        /// <param name="text">The text which will be drawn.</param>
        /// <param name="position">The drawing location on screen.</param>
        /// <param name="color">A color mask.</param>
        /// <param name="rotation">A rotation of this string.</param>
        /// <param name="origin">Center of the rotation. 0,0 by default.</param>
        /// <param name="scale">A scaling of this string.</param>
        /// <param name="effects">Modificators for drawing. Can be combined.</param>
        /// <param name="layerDepth">A depth of the layer of this string.</param>
		public unsafe void DrawString(
            SpriteFont spriteFont, string text, Vector2 position, Color color,
            float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
        {
            CheckValid(spriteFont, text);

            float sortKey = 0;
            // set SortKey based on SpriteSortMode.
            switch (_sortMode)
            {
                // Comparison of Texture objects.
                case SpriteSortMode.Texture:
                    sortKey = spriteFont.Texture.SortingKey;
                    break;
                // Comparison of Depth
                case SpriteSortMode.FrontToBack:
                    sortKey = layerDepth;
                    break;
                // Comparison of Depth in reverse
                case SpriteSortMode.BackToFront:
                    sortKey = -layerDepth;
                    break;
            }

            var flipAdjustment = Vector2.Zero;

            var flippedVert = (effects & SpriteEffects.FlipVertically) == SpriteEffects.FlipVertically;
            var flippedHorz = (effects & SpriteEffects.FlipHorizontally) == SpriteEffects.FlipHorizontally;

            if (flippedVert || flippedHorz)
            {
                Vector2 size;

                var source = new SpriteFont.CharacterSource(text);
                spriteFont.MeasureString(ref source, out size);

                if (flippedHorz)
                {
                    origin.X *= -1;
                    flipAdjustment.X = -size.X;
                }

                if (flippedVert)
                {
                    origin.Y *= -1;
                    flipAdjustment.Y = spriteFont.LineSpacing - size.Y;
                }
            }

            Matrix transformation = Matrix.Identity;
            float cos = 0, sin = 0;
            if (rotation == 0)
            {
                transformation.M11 = (flippedHorz ? -scale.X : scale.X);
                transformation.M22 = (flippedVert ? -scale.Y : scale.Y);
                transformation.M41 = ((flipAdjustment.X - origin.X) * transformation.M11) + position.X;
                transformation.M42 = ((flipAdjustment.Y - origin.Y) * transformation.M22) + position.Y;
            }
            else
            {
                cos = (float)Math.Cos(rotation);
                sin = (float)Math.Sin(rotation);
                transformation.M11 = (flippedHorz ? -scale.X : scale.X) * cos;
                transformation.M12 = (flippedHorz ? -scale.X : scale.X) * sin;
                transformation.M21 = (flippedVert ? -scale.Y : scale.Y) * (-sin);
                transformation.M22 = (flippedVert ? -scale.Y : scale.Y) * cos;
                transformation.M41 = (((flipAdjustment.X - origin.X) * transformation.M11) + (flipAdjustment.Y - origin.Y) * transformation.M21) + position.X;
                transformation.M42 = (((flipAdjustment.X - origin.X) * transformation.M12) + (flipAdjustment.Y - origin.Y) * transformation.M22) + position.Y;
            }

            var offset = Vector2.Zero;
            var firstGlyphOfLine = true;

            fixed (SpriteFont.Glyph* pGlyphs = spriteFont.Glyphs)
                for (var i = 0; i < text.Length; ++i)
                {
                    var c = text[i];

                    if (c == '\r')
                        continue;

                    if (c == '\n')
                    {
                        offset.X = 0;
                        offset.Y += spriteFont.LineSpacing;
                        firstGlyphOfLine = true;
                        continue;
                    }

                    var currentGlyphIndex = spriteFont.GetGlyphIndexOrDefault(c);
                    var pCurrentGlyph = pGlyphs + currentGlyphIndex;

                    // The first character on a line might have a negative left side bearing.
                    // In this scenario, SpriteBatch/SpriteFont normally offset the text to the right,
                    //  so that text does not hang off the left side of its rectangle.
                    if (firstGlyphOfLine)
                    {
                        offset.X = Math.Max(pCurrentGlyph->LeftSideBearing, 0);
                        firstGlyphOfLine = false;
                    }
                    else
                    {
                        offset.X += spriteFont.Spacing + pCurrentGlyph->LeftSideBearing;
                    }

                    var p = offset;

                    if (flippedHorz)
                        p.X += pCurrentGlyph->BoundsInTexture.Width;
                    p.X += pCurrentGlyph->Cropping.X;

                    if (flippedVert)
                        p.Y += pCurrentGlyph->BoundsInTexture.Height - spriteFont.LineSpacing;
                    p.Y += pCurrentGlyph->Cropping.Y;

                    Vector2.Transform(ref p, ref transformation, out p);

                    var item = _batcher.CreateBatchItem();
                    item.Texture = spriteFont.Texture;
                    item.SortKey = sortKey;

                    _texCoordTL.X = pCurrentGlyph->BoundsInTexture.X * spriteFont.Texture.TexelWidth;
                    _texCoordTL.Y = pCurrentGlyph->BoundsInTexture.Y * spriteFont.Texture.TexelHeight;
                    _texCoordBR.X = (pCurrentGlyph->BoundsInTexture.X + pCurrentGlyph->BoundsInTexture.Width) * spriteFont.Texture.TexelWidth;
                    _texCoordBR.Y = (pCurrentGlyph->BoundsInTexture.Y + pCurrentGlyph->BoundsInTexture.Height) * spriteFont.Texture.TexelHeight;

                    if ((effects & SpriteEffects.FlipVertically) != 0)
                    {
                        var temp = _texCoordBR.Y;
                        _texCoordBR.Y = _texCoordTL.Y;
                        _texCoordTL.Y = temp;
                    }
                    if ((effects & SpriteEffects.FlipHorizontally) != 0)
                    {
                        var temp = _texCoordBR.X;
                        _texCoordBR.X = _texCoordTL.X;
                        _texCoordTL.X = temp;
                    }

                    if (rotation == 0f)
                    {
                        item.Set(p.X,
                                p.Y,
                                pCurrentGlyph->BoundsInTexture.Width * scale.X,
                                pCurrentGlyph->BoundsInTexture.Height * scale.Y,
                                color,
                                _texCoordTL,
                                _texCoordBR,
                                layerDepth);
                    }
                    else
                    {
                        item.Set(p.X,
                                p.Y,
                                0,
                                0,
                                pCurrentGlyph->BoundsInTexture.Width * scale.X,
                                pCurrentGlyph->BoundsInTexture.Height * scale.Y,
                                sin,
                                cos,
                                color,
                                _texCoordTL,
                                _texCoordBR,
                                layerDepth);
                    }

                    offset.X += pCurrentGlyph->Width + pCurrentGlyph->RightSideBearing;
                }

            // We need to flush if we're using Immediate sort mode.
            FlushIfNeeded();
        }

#if NET_4_0
        private readonly static int[] EmptyInt = new int[0];
#endif
        /// <summary>
        /// Return the glyph index used to DrawString
        /// </summary>
        /// <param name="spriteFont"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public int[] PrepareDrawString(SpriteFont spriteFont,
                                       string text)
        {
            int count = text.Length;
#if NET_4_0
            if (count == 0)
                return EmptyInt;
#else
            if (count == 0)
                return Array.Empty<int>();
#endif

            int[] glyphsIndices = new int[count];

            for (int i = 0; i < count; ++i)
            {
                char c = text[i];

                if (c == '\r')
                {
                    glyphsIndices[i] = -2;
                    continue;
                }

                if (c == '\n')
                {
                    glyphsIndices[i] = -1;
                    continue;
                }

                glyphsIndices[i] = spriteFont.GetGlyphIndexOrDefault(c);
            }

            return glyphsIndices;
        }

        /// <summary>
        /// Submit a text string of sprites for drawing in the current batch.
        /// </summary>
        /// <param name="spriteFont"></param>
        /// <param name="glyphsIndices">Value returned by PrepareDrawString</param>
        /// <param name="origin"></param>
        /// <param name="lineHeight"></param>
        /// <param name="transform"></param>
        /// <param name="colorTop"></param>
        /// <param name="colorBottom"></param>
        public unsafe void DrawString(SpriteFont spriteFont,
                                      int[] glyphsIndices,
                                      ref Vector2 origin,
                                      float lineHeight,
                                      ref Matrix transform,
                                      Color colorTop,
                                      Color colorBottom)
        {
            int count = glyphsIndices.Length;
            if (count == 0)
                return;

            Vector2 offset = Vector2.Zero;

            int currentGlyphIndex;
            float scale = spriteFont.Scale;

            fixed (SpriteFont.Glyph* pGlyphs = spriteFont.Glyphs)
            {
                for (int i = 0; i < count; ++i)
                {
                    currentGlyphIndex = glyphsIndices[i];

                    if (currentGlyphIndex == -2)
                        continue;

                    if (currentGlyphIndex == -1)
                    {
                        offset.X = 0;
                        offset.Y += spriteFont.LineSpacing;
                        continue;
                    }

                    SpriteFont.Glyph* pCurrentGlyph = pGlyphs + currentGlyphIndex;

                    if (i != 0)
                    {
                        offset.X += (spriteFont.Spacing + pCurrentGlyph->LeftSideBearing) * scale;
                    }
                    else
                    {
                        offset.X = spriteFont.Spacing * scale;
                    }

                    Vector2 p = offset;

                    float height = pCurrentGlyph->Cropping.Height * scale;
                    float width = pCurrentGlyph->Cropping.Width * scale;

                    p.X = p.X + pCurrentGlyph->Cropping.X * scale - origin.X;
                    p.Y = -p.Y - height + lineHeight - pCurrentGlyph->Cropping.Y * scale - origin.Y;

                    offset.X += width + pCurrentGlyph->RightSideBearing * scale;

                    float right = p.X + width;
                    float top = p.Y + height;

                    SpriteBatchItem item = _batcher.CreateBatchItem();
                    item.Texture = spriteFont.Texture;

                    _texCoordTL.X = pCurrentGlyph->BoundsInTexture.X * spriteFont.Texture.TexelWidth;
                    _texCoordTL.Y = pCurrentGlyph->BoundsInTexture.Y * spriteFont.Texture.TexelHeight;
                    _texCoordBR.X = (pCurrentGlyph->BoundsInTexture.X + pCurrentGlyph->BoundsInTexture.Width) * spriteFont.Texture.TexelWidth;
                    _texCoordBR.Y = (pCurrentGlyph->BoundsInTexture.Y + pCurrentGlyph->BoundsInTexture.Height) * spriteFont.Texture.TexelHeight;

                    item.vertexTL.Position.X = (p.X * transform.M11) + (p.Y * transform.M21) + transform.M41;
                    item.vertexTL.Position.Y = (p.X * transform.M12) + (p.Y * transform.M22) + transform.M42;
                    item.vertexTL.Position.Z = transform.M43;

                    item.vertexTL.Color = colorTop;
                    item.vertexTL.TextureCoordinate.X = _texCoordTL.X;
                    item.vertexTL.TextureCoordinate.Y = _texCoordBR.Y;

                    item.vertexBL.Position.X = (p.X * transform.M11) + (top * transform.M21) + transform.M41;
                    item.vertexBL.Position.Y = (p.X * transform.M12) + (top * transform.M22) + transform.M42;
                    item.vertexBL.Position.Z = transform.M43;

                    item.vertexBL.Color = colorBottom;
                    item.vertexBL.TextureCoordinate = _texCoordTL;

                    item.vertexTR.Position.X = (right * transform.M11) + (p.Y * transform.M21) + transform.M41;
                    item.vertexTR.Position.Y = (right * transform.M12) + (p.Y * transform.M22) + transform.M42;
                    item.vertexTR.Position.Z = transform.M43;

                    item.vertexTR.Color = colorTop;
                    item.vertexTR.TextureCoordinate = _texCoordBR;

                    item.vertexBR.Position.X = (right * transform.M11) + (top * transform.M21) + transform.M41;
                    item.vertexBR.Position.Y = (right * transform.M12) + (top * transform.M22) + transform.M42;
                    item.vertexBR.Position.Z = transform.M43;

                    item.vertexBR.Color = colorBottom;
                    item.vertexBR.TextureCoordinate.X = _texCoordBR.X;
                    item.vertexBR.TextureCoordinate.Y = _texCoordTL.Y;
                }
            }
            // We need to flush if we're using Immediate sort mode.
            FlushIfNeeded();
        }

        /// <summary>
        /// Submit a text string of sprites for drawing in the current batch.
        /// </summary>
        /// <param name="spriteFont">A font.</param>
        /// <param name="text">The text which will be drawn.</param>
        /// <param name="position">The drawing location on screen.</param>
        /// <param name="color">A color mask.</param>
		public unsafe void DrawString(SpriteFont spriteFont, StringBuilder text, Vector2 position, Color color)
        {
            CheckValid(spriteFont, text);

            float sortKey = (_sortMode == SpriteSortMode.Texture) ? spriteFont.Texture.SortingKey : 0;

            var offset = Vector2.Zero;
            var firstGlyphOfLine = true;

            fixed (SpriteFont.Glyph* pGlyphs = spriteFont.Glyphs)
                for (var i = 0; i < text.Length; ++i)
                {
                    var c = text[i];

                    if (c == '\r')
                        continue;

                    if (c == '\n')
                    {
                        offset.X = 0;
                        offset.Y += spriteFont.LineSpacing;
                        firstGlyphOfLine = true;
                        continue;
                    }

                    var currentGlyphIndex = spriteFont.GetGlyphIndexOrDefault(c);
                    var pCurrentGlyph = pGlyphs + currentGlyphIndex;

                    // The first character on a line might have a negative left side bearing.
                    // In this scenario, SpriteBatch/SpriteFont normally offset the text to the right,
                    //  so that text does not hang off the left side of its rectangle.
                    if (firstGlyphOfLine)
                    {
                        offset.X = Math.Max(pCurrentGlyph->LeftSideBearing, 0);
                        firstGlyphOfLine = false;
                    }
                    else
                    {
                        offset.X += spriteFont.Spacing + pCurrentGlyph->LeftSideBearing;
                    }

                    var p = offset;
                    p.X += pCurrentGlyph->Cropping.X;
                    p.Y += pCurrentGlyph->Cropping.Y;
                    p += position;

                    var item = _batcher.CreateBatchItem();
                    item.Texture = spriteFont.Texture;
                    item.SortKey = sortKey;

                    _texCoordTL.X = pCurrentGlyph->BoundsInTexture.X * spriteFont.Texture.TexelWidth;
                    _texCoordTL.Y = pCurrentGlyph->BoundsInTexture.Y * spriteFont.Texture.TexelHeight;
                    _texCoordBR.X = (pCurrentGlyph->BoundsInTexture.X + pCurrentGlyph->BoundsInTexture.Width) * spriteFont.Texture.TexelWidth;
                    _texCoordBR.Y = (pCurrentGlyph->BoundsInTexture.Y + pCurrentGlyph->BoundsInTexture.Height) * spriteFont.Texture.TexelHeight;

                    item.Set(p.X,
                             p.Y,
                             pCurrentGlyph->BoundsInTexture.Width,
                             pCurrentGlyph->BoundsInTexture.Height,
                             color,
                             _texCoordTL,
                             _texCoordBR,
                             0);

                    offset.X += pCurrentGlyph->Width + pCurrentGlyph->RightSideBearing;
                }

            // We need to flush if we're using Immediate sort mode.
            FlushIfNeeded();
        }

        /// <summary>
        /// Submit a text string of sprites for drawing in the current batch.
        /// </summary>
        /// <param name="spriteFont">A font.</param>
        /// <param name="text">The text which will be drawn.</param>
        /// <param name="position">The drawing location on screen.</param>
        /// <param name="color">A color mask.</param>
        /// <param name="rotation">A rotation of this string.</param>
        /// <param name="origin">Center of the rotation. 0,0 by default.</param>
        /// <param name="scale">A scaling of this string.</param>
        /// <param name="effects">Modificators for drawing. Can be combined.</param>
        /// <param name="layerDepth">A depth of the layer of this string.</param>
		public void DrawString(
            SpriteFont spriteFont, StringBuilder text, Vector2 position, Color color,
            float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
        {
            var scaleVec = new Vector2(scale, scale);
            DrawString(spriteFont, text, position, color, rotation, origin, scaleVec, effects, layerDepth);
        }

        /// <summary>
        /// Submit a text string of sprites for drawing in the current batch.
        /// </summary>
        /// <param name="spriteFont">A font.</param>
        /// <param name="text">The text which will be drawn.</param>
        /// <param name="position">The drawing location on screen.</param>
        /// <param name="color">A color mask.</param>
        /// <param name="rotation">A rotation of this string.</param>
        /// <param name="origin">Center of the rotation. 0,0 by default.</param>
        /// <param name="scale">A scaling of this string.</param>
        /// <param name="effects">Modificators for drawing. Can be combined.</param>
        /// <param name="layerDepth">A depth of the layer of this string.</param>
		public unsafe void DrawString(
            SpriteFont spriteFont, StringBuilder text, Vector2 position, Color color,
            float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
        {
            CheckValid(spriteFont, text);

            float sortKey = 0;
            // set SortKey based on SpriteSortMode.
            switch (_sortMode)
            {
                // Comparison of Texture objects.
                case SpriteSortMode.Texture:
                    sortKey = spriteFont.Texture.SortingKey;
                    break;
                // Comparison of Depth
                case SpriteSortMode.FrontToBack:
                    sortKey = layerDepth;
                    break;
                // Comparison of Depth in reverse
                case SpriteSortMode.BackToFront:
                    sortKey = -layerDepth;
                    break;
            }

            var flipAdjustment = Vector2.Zero;

            var flippedVert = (effects & SpriteEffects.FlipVertically) == SpriteEffects.FlipVertically;
            var flippedHorz = (effects & SpriteEffects.FlipHorizontally) == SpriteEffects.FlipHorizontally;

            if (flippedVert || flippedHorz)
            {
                var source = new SpriteFont.CharacterSource(text);
                Vector2 size;
                spriteFont.MeasureString(ref source, out size);

                if (flippedHorz)
                {
                    origin.X *= -1;
                    flipAdjustment.X = -size.X;
                }

                if (flippedVert)
                {
                    origin.Y *= -1;
                    flipAdjustment.Y = spriteFont.LineSpacing - size.Y;
                }
            }

            Matrix transformation = Matrix.Identity;
            float cos = 0, sin = 0;
            if (rotation == 0)
            {
                transformation.M11 = (flippedHorz ? -scale.X : scale.X);
                transformation.M22 = (flippedVert ? -scale.Y : scale.Y);
                transformation.M41 = ((flipAdjustment.X - origin.X) * transformation.M11) + position.X;
                transformation.M42 = ((flipAdjustment.Y - origin.Y) * transformation.M22) + position.Y;
            }
            else
            {
                cos = (float)Math.Cos(rotation);
                sin = (float)Math.Sin(rotation);
                transformation.M11 = (flippedHorz ? -scale.X : scale.X) * cos;
                transformation.M12 = (flippedHorz ? -scale.X : scale.X) * sin;
                transformation.M21 = (flippedVert ? -scale.Y : scale.Y) * (-sin);
                transformation.M22 = (flippedVert ? -scale.Y : scale.Y) * cos;
                transformation.M41 = (((flipAdjustment.X - origin.X) * transformation.M11) + (flipAdjustment.Y - origin.Y) * transformation.M21) + position.X;
                transformation.M42 = (((flipAdjustment.X - origin.X) * transformation.M12) + (flipAdjustment.Y - origin.Y) * transformation.M22) + position.Y;
            }

            var offset = Vector2.Zero;
            var firstGlyphOfLine = true;

            fixed (SpriteFont.Glyph* pGlyphs = spriteFont.Glyphs)
                for (var i = 0; i < text.Length; ++i)
                {
                    var c = text[i];

                    if (c == '\r')
                        continue;

                    if (c == '\n')
                    {
                        offset.X = 0;
                        offset.Y += spriteFont.LineSpacing;
                        firstGlyphOfLine = true;
                        continue;
                    }

                    var currentGlyphIndex = spriteFont.GetGlyphIndexOrDefault(c);
                    var pCurrentGlyph = pGlyphs + currentGlyphIndex;

                    // The first character on a line might have a negative left side bearing.
                    // In this scenario, SpriteBatch/SpriteFont normally offset the text to the right,
                    //  so that text does not hang off the left side of its rectangle.
                    if (firstGlyphOfLine)
                    {
                        offset.X = Math.Max(pCurrentGlyph->LeftSideBearing, 0);
                        firstGlyphOfLine = false;
                    }
                    else
                    {
                        offset.X += spriteFont.Spacing + pCurrentGlyph->LeftSideBearing;
                    }

                    var p = offset;

                    if (flippedHorz)
                        p.X += pCurrentGlyph->BoundsInTexture.Width;
                    p.X += pCurrentGlyph->Cropping.X;

                    if (flippedVert)
                        p.Y += pCurrentGlyph->BoundsInTexture.Height - spriteFont.LineSpacing;
                    p.Y += pCurrentGlyph->Cropping.Y;

                    Vector2.Transform(ref p, ref transformation, out p);

                    var item = _batcher.CreateBatchItem();
                    item.Texture = spriteFont.Texture;
                    item.SortKey = sortKey;

                    _texCoordTL.X = pCurrentGlyph->BoundsInTexture.X * (float)spriteFont.Texture.TexelWidth;
                    _texCoordTL.Y = pCurrentGlyph->BoundsInTexture.Y * (float)spriteFont.Texture.TexelHeight;
                    _texCoordBR.X = (pCurrentGlyph->BoundsInTexture.X + pCurrentGlyph->BoundsInTexture.Width) * (float)spriteFont.Texture.TexelWidth;
                    _texCoordBR.Y = (pCurrentGlyph->BoundsInTexture.Y + pCurrentGlyph->BoundsInTexture.Height) * (float)spriteFont.Texture.TexelHeight;

                    if ((effects & SpriteEffects.FlipVertically) != 0)
                    {
                        var temp = _texCoordBR.Y;
                        _texCoordBR.Y = _texCoordTL.Y;
                        _texCoordTL.Y = temp;
                    }
                    if ((effects & SpriteEffects.FlipHorizontally) != 0)
                    {
                        var temp = _texCoordBR.X;
                        _texCoordBR.X = _texCoordTL.X;
                        _texCoordTL.X = temp;
                    }

                    if (rotation == 0f)
                    {
                        item.Set(p.X,
                                p.Y,
                                pCurrentGlyph->BoundsInTexture.Width * scale.X,
                                pCurrentGlyph->BoundsInTexture.Height * scale.Y,
                                color,
                                _texCoordTL,
                                _texCoordBR,
                                layerDepth);
                    }
                    else
                    {
                        item.Set(p.X,
                                p.Y,
                                0,
                                0,
                                pCurrentGlyph->BoundsInTexture.Width * scale.X,
                                pCurrentGlyph->BoundsInTexture.Height * scale.Y,
                                sin,
                                cos,
                                color,
                                _texCoordTL,
                                _texCoordBR,
                                layerDepth);
                    }

                    offset.X += pCurrentGlyph->Width + pCurrentGlyph->RightSideBearing;
                }

            // We need to flush if we're using Immediate sort mode.
            FlushIfNeeded();
        }

        /// <summary>
        /// Immediately releases the unmanaged resources used by this object.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    if (_spriteEffect != null)
                    {
                        _spriteEffect.Dispose();
                        _spriteEffect = null;
                    }
                }
            }
            base.Dispose(disposing);
        }
    }
}

