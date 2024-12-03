using System;
using System.Reflection;
using System.Runtime.CompilerServices;

#if __MACOS__ || __TVOS__ || __WATCHOS__ || __IOS__ || __MACCATALYST__

using Foundation;

#if NET
[assembly: AssemblyMetadata("IsTrimmable", "True")]
#else
[assembly: LinkerSafe]
#endif

#endif

// [assembly:InternalsVisibleTo("MonoGame.Tests")]
// [assembly:InternalsVisibleTo("2MGFX")]
[assembly: AssemblyDescription("MonoGame is an open source implementation of the Microsoft XNA 4.x Framework")]
[assembly: AssemblyCompany("MonoGame Team")]
[assembly: AssemblyProduct("MonoGame.Framework")]
[assembly: AssemblyCopyright("Copyright © 2009-2024 MonoGame Team (modified by Codefoco)")]
[assembly: AssemblyVersion("3.8.0.0")]
[assembly: AssemblyFileVersion("3.8.0.0")]
[assembly: AssemblyTrademark("MonoGame® is a registered trademark of the MonoGame Team")]
[assembly: AssemblyInformationalVersion("3.8.0-codefoco-build.1+528.Branch.codefoco-build.Sha.d3ec5d11fcd14df10d5a93d6efba0686fe13fd9e")]

// Information about this assembly is defined by the following attributes. 
// Change them to the values specific to your project.

#if NETCOREAPP
[assembly: AssemblyTitle("MonoGame.Framework (.NET)")]
#elif WINDOWS_UWP
[assembly: AssemblyTitle("MonoGame.Framework (Windows Universal)")]
#elif __ANDROID__
[assembly: AssemblyTitle("MonoGame.Framework (Android)")]
#elif NETSTANDARD
[assembly: AssemblyTitle("MonoGame.Framework (.NET Standard)")]
#elif __TVOS__
[assembly: AssemblyTitle ("MonoGame.Framework (tvOS)")]
#elif __IOS__
[assembly: AssemblyTitle("MonoGame.Framework (iOS)")]
#elif __MACOS__
[assembly: AssemblyTitle("MonoGame.Framework (Mac)")]
#elif DESKTOPGL
#if NET_4_0
        [assembly: AssemblyTitle("MonoGame.Framework [OpenGL] (.NET Framework 4.0)")]
#else
        [assembly: AssemblyTitle("MonoGame.Framework [OpenGL] (.NET Framework 4.6)")]
#endif
#elif DIRECTX
#if NET_4_0
            [assembly: AssemblyTitle("MonoGame.Framework [DirectX] (.NET Framework 4.0)")]
#else
            [assembly: AssemblyTitle("MonoGame.Framework [DirectX] (.NET Framework 4.6)")]
#endif
#endif



