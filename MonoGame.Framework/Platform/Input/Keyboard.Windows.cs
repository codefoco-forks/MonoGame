// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework.Input
{
    public static partial class Keyboard
    {
        private static readonly byte[] DefinedKeyCodes;

        private static readonly byte[] _keyState = new byte[256];
        private static readonly List<Keys> _keys = new List<Keys>(10);

        private static bool _isActive;

        [DllImport("user32.dll")]
        private static extern bool GetKeyboardState(byte[] lpKeyState);

        private static readonly Predicate<Keys> IsKeyReleasedPredicate = key => IsKeyReleased((byte)key);

        static Keyboard()
        {
            Keys[] definedKeys =
            {
                Keys.None,
                Keys.Back,
                Keys.Tab,
                Keys.Enter,
                Keys.CapsLock,
                Keys.Escape,
                Keys.Space,
                Keys.PageUp,
                Keys.PageDown,
                Keys.End,
                Keys.Home,
                Keys.Left,
                Keys.Up,
                Keys.Right,
                Keys.Down,
                Keys.Select,
                Keys.Print,
                Keys.Execute,
                Keys.PrintScreen,
                Keys.Insert,
                Keys.Delete,
                Keys.Help,
                Keys.D0,
                Keys.D1,
                Keys.D2,
                Keys.D3,
                Keys.D4,
                Keys.D5,
                Keys.D6,
                Keys.D7,
                Keys.D8,
                Keys.D9,
                Keys.A,
                Keys.B,
                Keys.C,
                Keys.D,
                Keys.E,
                Keys.F,
                Keys.G,
                Keys.H,
                Keys.I,
                Keys.J,
                Keys.K,
                Keys.L,
                Keys.M,
                Keys.N,
                Keys.O,
                Keys.P,
                Keys.Q,
                Keys.R,
                Keys.S,
                Keys.T,
                Keys.U,
                Keys.V,
                Keys.W,
                Keys.X,
                Keys.Y,
                Keys.Z,
                Keys.LeftWindows,
                Keys.RightWindows,
                Keys.Apps,
                Keys.Sleep,
                Keys.NumPad0,
                Keys.NumPad1,
                Keys.NumPad2,
                Keys.NumPad3,
                Keys.NumPad4,
                Keys.NumPad5,
                Keys.NumPad6,
                Keys.NumPad7,
                Keys.NumPad8,
                Keys.NumPad9,
                Keys.Multiply,
                Keys.Add,
                Keys.Separator,
                Keys.Subtract,
                Keys.Decimal,
                Keys.Divide,
                Keys.F1,
                Keys.F2,
                Keys.F3,
                Keys.F4,
                Keys.F5,
                Keys.F6,
                Keys.F7,
                Keys.F8,
                Keys.F9,
                Keys.F10,
                Keys.F11,
                Keys.F12,
                Keys.F13,
                Keys.F14,
                Keys.F15,
                Keys.F16,
                Keys.F17,
                Keys.F18,
                Keys.F19,
                Keys.F20,
                Keys.F21,
                Keys.F22,
                Keys.F23,
                Keys.F24,
                Keys.NumLock,
                Keys.Scroll,
                Keys.LeftShift,
                Keys.RightShift,
                Keys.LeftControl,
                Keys.RightControl,
                Keys.LeftAlt,
                Keys.RightAlt,
                Keys.BrowserBack,
                Keys.BrowserForward,
                Keys.BrowserRefresh,
                Keys.BrowserStop,
                Keys.BrowserSearch,
                Keys.BrowserFavorites,
                Keys.BrowserHome,
                Keys.VolumeMute,
                Keys.VolumeDown,
                Keys.VolumeUp,
                Keys.MediaNextTrack,
                Keys.MediaPreviousTrack,
                Keys.MediaStop,
                Keys.MediaPlayPause,
                Keys.LaunchMail,
                Keys.SelectMedia,
                Keys.LaunchApplication1,
                Keys.LaunchApplication2,
                Keys.OemSemicolon,
                Keys.OemPlus,
                Keys.OemComma,
                Keys.OemMinus,
                Keys.OemPeriod,
                Keys.OemQuestion,
                Keys.OemTilde,
                Keys.OemOpenBrackets,
                Keys.OemPipe,
                Keys.OemCloseBrackets,
                Keys.OemQuotes,
                Keys.Oem8,
                Keys.OemBackslash,
                Keys.ProcessKey,
                Keys.Attn,
                Keys.Crsel,
                Keys.Exsel,
                Keys.EraseEof,
                Keys.Play,
                Keys.Zoom,
                Keys.Pa1,
                Keys.OemClear,
                Keys.ChatPadGreen,
                Keys.ChatPadOrange,
                Keys.Pause,
                Keys.ImeConvert,
                Keys.ImeNoConvert,
                Keys.Kana,
                Keys.Kanji,
                Keys.OemAuto,
                Keys.OemCopy,
                Keys.OemEnlW
            };

            var keyCodes = new List<byte>(Math.Min(definedKeys.Length, 255));
            for (int i = 0; i < definedKeys.Length; i++)
            {
                Keys key = definedKeys[i];
                var keyCode = (int)key;
                if ((keyCode >= 1) && (keyCode <= 255))
                    keyCodes.Add((byte)keyCode);
            }
            DefinedKeyCodes = keyCodes.ToArray();
        }

        private static KeyboardState PlatformGetState()
        {
            if (_isActive && GetKeyboardState(_keyState))
            {
                _keys.RemoveAll(IsKeyReleasedPredicate);

                for (int i = 0; i < DefinedKeyCodes.Length; i++)
                {
                    byte keyCode = DefinedKeyCodes[i];
                    if (IsKeyReleased(keyCode))
                        continue;

                    var key = (Keys)keyCode;
                    _keys.Add(key);
                }
            }

            return new KeyboardState(_keys, Console.CapsLock, Console.NumberLock);
        }

        private static bool IsKeyReleased(byte keyCode)
        {
            return ((_keyState[keyCode] & 0x80) == 0);
        }

        internal static void SetActive(bool isActive)
        {
            _isActive = isActive;
            if (!_isActive)
                _keys.Clear();
        }
    }
}
