// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Runtime.InteropServices;
using System;

namespace MonoGame.Framework.Utilities
{
    /// <summary>
    /// Runtime OS identifier
    /// </summary>
    public enum OS
    {
        Windows,
        Linux,
        MacOSX,
        Unknown
    }

    /// <summary>
    /// Current runtime Platform
    /// </summary>
    public static class CurrentPlatform
    {
        private static bool _init = false;
        private static OS _os;

        [DllImport("libc")]
        static extern int uname(IntPtr buf);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool IsWow64Process2(
            IntPtr process, 
            out ushort processMachine, 
            out ushort nativeMachine
        );

        internal static bool IsArm64
        {
            get
            {
                ushort processMachine;
                ushort nativeMachine;

                IntPtr handle = System.Diagnostics.Process.GetCurrentProcess().Handle;

                IsWow64Process2(handle, out processMachine, out nativeMachine);

                return nativeMachine == 0xaa64;
            }
        }

        private static void Init()
        {
            if (_init)
                return;

            var pid = Environment.OSVersion.Platform;

            switch (pid)
            {
                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.WinCE:
                    _os = OS.Windows;
                    break;
                case PlatformID.MacOSX:
                    _os = OS.MacOSX;
                    break;
                case PlatformID.Unix:
                    _os = OS.MacOSX;

                    var buf = IntPtr.Zero;
                    
                    try
                    {
                        buf = Marshal.AllocHGlobal(8192);

                        if (uname(buf) == 0 && Marshal.PtrToStringAnsi(buf) == "Linux")
                            _os = OS.Linux;
                    }
                    catch
                    {
                    }
                    finally
                    {
                        if (buf != IntPtr.Zero)
                            Marshal.FreeHGlobal(buf);
                    }

                    break;
                default:
                    _os = OS.Unknown;
                    break;
            }

            _init = true;
        }

        public static OS OS
        {
            get
            {
                Init();
                return _os;
            }
        }

        public static string Rid
        {
            get
            {
                if (CurrentPlatform.OS == OS.Windows)
                {
                    if (CurrentPlatform.IsArm64)
                        return "win-arm64";
                    if (Environment.Is64BitProcess)
                        return "win-x64";
                    return "win-x86";
                }
                if (CurrentPlatform.OS == OS.Linux)
                    return "linux-x64";
                if (CurrentPlatform.OS == OS.MacOSX)
                    return "osx";
                else
                    return "unknown";
            }
        }
    }
}

