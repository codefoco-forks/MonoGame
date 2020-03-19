// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.ApplicationModel.Activation;

namespace Microsoft.Xna.Framework
{
    class UAPFrameworkView: IFrameworkView
    {
        Func<IFrameworkView, Game> _onGetFrameworkViewGame;

        public UAPFrameworkView(Func<IFrameworkView, Game> onGetFrameworkViewGame)
        {
            _onGetFrameworkViewGame = onGetFrameworkViewGame;
        }

        private CoreApplicationView _applicationView;
        private Game _game;

        public void Initialize(CoreApplicationView applicationView)
        {
            _applicationView = applicationView;
            _applicationView.Activated += ViewActivated;
        }

        private void ViewActivated(CoreApplicationView sender, IActivatedEventArgs args)
        {
            if (args.Kind == ActivationKind.Launch)
            {
                // Save any launch parameters to be parsed by the platform.
                UAPGamePlatform.LaunchParameters = ((LaunchActivatedEventArgs)args).Arguments;
                UAPGamePlatform.PreviousExecutionState = ((LaunchActivatedEventArgs)args).PreviousExecutionState;

                _game = _onGetFrameworkViewGame(this);
            }
            else if (args.Kind == ActivationKind.Protocol)
            {
                // Save any protocol launch parameters to be parsed by the platform.
                var protocolArgs = args as ProtocolActivatedEventArgs;
                UAPGamePlatform.LaunchParameters = protocolArgs.Uri.AbsoluteUri;
                UAPGamePlatform.PreviousExecutionState = protocolArgs.PreviousExecutionState;

                if (_game == null)
                {
                    _game = _onGetFrameworkViewGame(this);
                }
            }
        }

        public void Load(string entryPoint)
        {
        }


        public void Run()
        {
            // Initialize and run the game.
            _game.Run();
        }
       
        public void SetWindow(CoreWindow window)
        {
            // Initialize the singleton window.
            UAPGameWindow.Instance.Initialize(window, null, UAPGamePlatform.TouchQueue);
        }

        public void Uninitialize()
        {
            // TODO: I have no idea when and if this is
            // called... as of Win8 build 8250 this seems 
            // like its never called.
        }
    }
}
