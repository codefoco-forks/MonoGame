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

        private static bool _isARM64;

        [DllImport("libc")]
        static extern int uname(IntPtr buf);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool IsWow64Process2(
            IntPtr process, 
            out ushort processMachine, 
            out ushort nativeMachine
        );

        private static bool CheckIsARM64Windows()
        {
            ushort processMachine;
            ushort nativeMachine;

            // IsWow64Process2 is only available on Windows 10
            if (Environment.OSVersion.Version.Major < 10 || Environment.OSVersion.Version.Build < 1511)
                return false;

            IntPtr handle = System.Diagnostics.Process.GetCurrentProcess().Handle;

            IsWow64Process2(handle, out processMachine, out nativeMachine);

            return nativeMachine == 0xaa64;
        }

        internal static bool IsARM64
        {
            get
            {
                return _isARM64;
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
                    _isARM64 = CheckIsARM64Windows();
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

                        if (uname(buf) == 0)
                        {
                            if (Marshal.PtrToStringAnsi(buf) == "Linux")
                                _os = OS.Linux;


                            // https://pubs.opengroup.org/onlinepubs/009696899/basedefs/sys/utsname.h.html
                            // Read machine field from uname to get the CPU arch
                            long offset = IntPtr.Size * 4;
                            IntPtr m = new IntPtr(buf.ToInt64() + offset);
                            string machine = Marshal.PtrToStringAnsi(m);

                            if (machine.StartsWith("arm64", StringComparison.OrdinalIgnoreCase) ||
                                machine.StartsWith("aarch64", StringComparison.OrdinalIgnoreCase))
                                _isARM64 = true;
                        }
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
                    if (CurrentPlatform.IsARM64)
                        return "win-arm64";
                    if (Environment.Is64BitProcess)
                        return "win-x64";
                    return "win-x86";
                }
                if (CurrentPlatform.OS == OS.Linux)
                {
                    if (CurrentPlatform.IsARM64)
                        return "linux-arm64";
                    return "linux-x64";
                }
                if (CurrentPlatform.OS == OS.MacOSX)
                    return "osx";
                else
                    return "unknown";
            }
        }
    }
}

