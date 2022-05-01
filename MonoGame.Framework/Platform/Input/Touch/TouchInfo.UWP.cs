using System;

using Windows.UI.Input;

using Windows.Devices.Input;

namespace Microsoft.Xna.Framework.Input.Touch
{
    /// <summary>
    /// Stores touches to apply them once a frame for platforms that dispatch touches asynchronously
    /// while user code is running.
    /// </summary>
    internal static class TouchInfo
    {
        internal static void GetPointerData(PointerPoint pointerPoint, out DeviceType deviceType, out float pressure, out float rotation)
        {
            deviceType = DeviceType.Touch;
            pressure = 0f;
            rotation = 0f;

            if (pointerPoint.PointerDevice.PointerDeviceType == PointerDeviceType.Mouse)
            {
                deviceType = DeviceType.Mouse;
                return;
            }

            if (pointerPoint.PointerDevice.PointerDeviceType == PointerDeviceType.Touch)
            {
                rotation = (float)((pointerPoint.Properties.Orientation / 180f) / Math.PI);
            }
            else if (pointerPoint.PointerDevice.PointerDeviceType == PointerDeviceType.Pen)
            {
                deviceType = DeviceType.Pen;
                rotation = (float)((pointerPoint.Properties.Twist / 180f) / Math.PI);
            }

            pressure = pointerPoint.Properties.Pressure;
        }
    }
}
