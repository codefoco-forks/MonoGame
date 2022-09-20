using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework.Platform.Windows
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct POINT
    {
        public int x;
        public int y;
    }
}
