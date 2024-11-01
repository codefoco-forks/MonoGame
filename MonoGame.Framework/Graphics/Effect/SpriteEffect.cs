// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// The default effect used by SpriteBatch.
    /// </summary>
    public class SpriteEffect : Effect
    {
        private EffectParameter _matrixParam;

        private Matrix _projection;
        private Matrix _world;

        // We cache the world+projection to avoid matrix multiplication every frame
        private Matrix _cachedProjection;

        /// <summary>
        /// Creates a new SpriteEffect.
        /// </summary>
        public SpriteEffect(GraphicsDevice device)
            : base(device, EffectResource.SpriteEffect.Bytecode)
        {
            CacheEffectParameters();
            UpdateViewportProjectionMatrix();

            _world = Matrix.Identity;
            _cachedProjection = _projection;
        }

        /// <summary>
        /// Creates a new SpriteEffect.
        /// </summary>
        public SpriteEffect(GraphicsDevice device, ref Matrix projection)
            : base(device, EffectResource.SpriteEffect.Bytecode)
        {
            CacheEffectParameters();

            _projection = projection;

            _world = Matrix.Identity;
            _cachedProjection = _projection;
        }

        /// <summary>
        /// World transformation
        /// </summary>
        public Matrix World
        {
            get
            {
                return _world;
            }
            set
            {
                _world = value;

                Matrix.Multiply(ref _world, ref _projection, out _cachedProjection);
            }
        }

        /// <summary>
        /// Projection matrix
        /// </summary>
        public Matrix Projection
        {
            get
            {
                return _projection;
            }
            set
            {
                _projection = value;

                Matrix.Multiply(ref _world, ref _projection, out _cachedProjection);
            }
        }

        /// <summary>
        /// Creates a new SpriteEffect by cloning parameter settings from an existing instance.
        /// </summary>
        protected SpriteEffect(SpriteEffect cloneSource)
            : base(cloneSource)
        {
            CacheEffectParameters();

            _projection = cloneSource._projection;
            _cachedProjection = cloneSource._cachedProjection;
        }


        /// <summary>
        /// Creates a clone of the current SpriteEffect instance.
        /// </summary>
        public override Effect Clone()
        {
            return new SpriteEffect(this);
        }

        /// <summary>
        /// Looks up shortcut references to our effect parameters.
        /// </summary>
        void CacheEffectParameters()
        {
            _matrixParam = Parameters["MatrixTransform"];
        }

        void UpdateViewportProjectionMatrix()
        {
            var vp = GraphicsDevice.Viewport;

            // Normal 3D cameras look into the -z direction (z = 1 is in front of z = 0). The
            // sprite batch layer depth is the opposite (z = 0 is in front of z = 1).
            // --> We get the correct matrix with near plane 0 and far plane -1.
            Matrix.CreateOrthographicOffCenter(0, vp.Width, vp.Height, 0, 0, -1, out _projection);

            if (GraphicsDevice.UseHalfPixelOffset)
            {
                _projection.M41 += -0.5f * _projection.M11;
                _projection.M42 += -0.5f * _projection.M22;
            }
        }

        /// <summary>
        /// Set the _cachedProjection matrix
        /// </summary>
        protected internal override void OnApply()
        {
            _matrixParam.SetValue(_cachedProjection);
        }
    }
}
