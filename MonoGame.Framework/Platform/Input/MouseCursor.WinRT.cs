// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

using Microsoft.Xna.Framework.Graphics;

using Windows.UI.Core;

namespace Microsoft.Xna.Framework.Input
{
    public partial class MouseCursor
    {
        private CoreCursor cursor;

        private MouseCursor(CoreCursor preDefinedCursor)
        {
            this.cursor = preDefinedCursor;
        }

        /// <summary>
        /// Return CoreCursor
        /// </summary>
        public CoreCursor Cursor
        {
            get { return cursor; }
        }

        private static void PlatformInitalize()
        {
            Arrow = new MouseCursor(new CoreCursor(CoreCursorType.Arrow, 1));
            IBeam = new MouseCursor(new CoreCursor(CoreCursorType.IBeam, 1));
            Wait = new MouseCursor(new CoreCursor(CoreCursorType.Wait, 1));
            Crosshair = new MouseCursor(new CoreCursor(CoreCursorType.Cross, 1));
            WaitArrow = new MouseCursor(new CoreCursor(CoreCursorType.Wait, 1));
            SizeNWSE = new MouseCursor(new CoreCursor(CoreCursorType.SizeNorthwestSoutheast, 1));
            SizeNESW = new MouseCursor(new CoreCursor(CoreCursorType.SizeNortheastSouthwest, 1));
            SizeWE = new MouseCursor(new CoreCursor(CoreCursorType.SizeWestEast, 1));
            SizeNS = new MouseCursor(new CoreCursor(CoreCursorType.SizeNorthSouth, 1));
            SizeAll = new MouseCursor(new CoreCursor(CoreCursorType.SizeAll, 1));
            No = new MouseCursor(new CoreCursor(CoreCursorType.UniversalNo, 1));
            Hand = new MouseCursor(new CoreCursor(CoreCursorType.Hand, 1));
        }

        private static MouseCursor PlatformFromTexture2D(Texture2D texture, int originx, int originy)
        {
            return new MouseCursor(IntPtr.Zero);
        }

        private void PlatformDispose()
        {
        }
    }
}
