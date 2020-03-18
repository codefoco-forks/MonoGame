// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;


namespace MonoGame.Framework
{
    public class GameFrameworkViewSource : IFrameworkViewSource
    {
        private Game _game;

        public GameFrameworkViewSource(Game game)
        {
            this._game = game;
        }

        [CLSCompliant(false)]
        public IFrameworkView CreateView()
        {
            return new UAPFrameworkView(_game);
        }
    }
}
