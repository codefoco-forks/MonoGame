// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework;
using Windows.ApplicationModel.Core;


namespace MonoGame.Framework
{
    /// <summary>
    /// UWP FrameworkViewSource
    /// </summary>
    public class GameFrameworkViewSource : IFrameworkViewSource
    {
        Action<IFrameworkView> _onGetFrameworkViewGame;
        /// <summary>
        /// ctor
        /// </summary>
        public GameFrameworkViewSource(Action<IFrameworkView> onGetFrameworkViewGame)
        {
            if(onGetFrameworkViewGame == null)
                throw new ArgumentNullException("onSetFrameworkViewGame");

            this._onGetFrameworkViewGame = onGetFrameworkViewGame;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IFrameworkView CreateView()
        {
            return new UAPFrameworkView(_onGetFrameworkViewGame);
        }
    }
}
