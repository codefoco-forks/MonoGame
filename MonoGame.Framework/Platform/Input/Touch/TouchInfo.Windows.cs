using Microsoft.Xna.Framework.Platform.Windows;
using System;
using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework.Input.Touch
{
    /// <summary>
    /// Get info about touch input
    /// </summary>
    internal static class TouchInfo
    {
        /// <summary>
        /// The type of input device used
        /// </summary>
        internal enum POINTER_INPUT_TYPE : UInt32
        {
            PT_POINTER = 0x00000001,
            PT_TOUCH = 0x00000002,
            PT_PEN = 0x00000003,
            PT_MOUSE = 0x00000004,
            PT_TOUCHPAD = 0x00000005
        }

        [Flags]
        internal enum POINTER_FLAGS
        {
            POINTER_FLAG_NONE = 0x00000000, // Default
            POINTER_FLAG_NEW = 0x00000001, // New pointer
            POINTER_FLAG_INRANGE = 0x00000002, // Pointer has not departed
            POINTER_FLAG_INCONTACT = 0x00000004, // Pointer is in contact
            POINTER_FLAG_FIRSTBUTTON = 0x00000010, // Primary action
            POINTER_FLAG_SECONDBUTTON = 0x00000020, // Secondary action
            POINTER_FLAG_THIRDBUTTON = 0x00000040, // Third button
            POINTER_FLAG_FOURTHBUTTON = 0x00000080, // Fourth button
            POINTER_FLAG_FIFTHBUTTON = 0x00000100, // Fifth button
            POINTER_FLAG_PRIMARY = 0x00002000, // Pointer is primary for system
            POINTER_FLAG_CONFIDENCE = 0x00004000, // Pointer is considered unlikely to be accidental
            POINTER_FLAG_CANCELED = 0x00008000, // Pointer is departing in an abnormal manner
            POINTER_FLAG_DOWN = 0x00010000, // Pointer transitioned to down state (made contact)
            POINTER_FLAG_UPDATE = 0x00020000, // Pointer update
            POINTER_FLAG_UP = 0x00040000, // Pointer transitioned from down state (broke contact)
            POINTER_FLAG_WHEEL = 0x00080000, // Vertical wheel
            POINTER_FLAG_HWHEEL = 0x00100000, // Horizontal wheel
            POINTER_FLAG_CAPTURECHANGED = 0x00200000, // Lost capture
            POINTER_FLAG_HASTRANSFORM = 0x00400000, // Input has a transform associated with it
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct POINT
        {
            public int x;
            public int y;
        }

        [Flags]
        public enum POINTER_MOD
        {
            POINTER_MOD_NONE = 0x0000,
            POINTER_MOD_SHIFT = 0x0004,    // Shift key is held down.
            POINTER_MOD_CTRL = 0x0008,    // Ctrl key is held down.
        }

        public enum POINTER_BUTTON_CHANGE_TYPE
        {
            POINTER_CHANGE_NONE,
            POINTER_CHANGE_FIRSTBUTTON_DOWN,
            POINTER_CHANGE_FIRSTBUTTON_UP,
            POINTER_CHANGE_SECONDBUTTON_DOWN,
            POINTER_CHANGE_SECONDBUTTON_UP,
            POINTER_CHANGE_THIRDBUTTON_DOWN,
            POINTER_CHANGE_THIRDBUTTON_UP,
            POINTER_CHANGE_FOURTHBUTTON_DOWN,
            POINTER_CHANGE_FOURTHBUTTON_UP,
            POINTER_CHANGE_FIFTHBUTTON_DOWN,
            POINTER_CHANGE_FIFTHBUTTON_UP,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINTER_INFO
        {
            public POINTER_INPUT_TYPE pointerType;
            public int pointerId;
            public int frameId;
            public POINTER_FLAGS pointerFlags;
            public IntPtr sourceDevice;
            public IntPtr hwndTarget;
            public POINT ptPixelLocation;
            public POINT ptHimetricLocation;
            public POINT ptPixelLocationRaw;
            public POINT ptHimetricLocationRaw;
            public uint dwTime;
            public int historyCount;
            public int InputData;
            public POINTER_MOD dwKeyStates;
            public long PerformanceCount;
            public POINTER_BUTTON_CHANGE_TYPE ButtonChangeType;
        }

        [Flags]
        internal enum TOUCH_FLAGS : UInt32
        {
            TOUCH_FLAG_NONE = 0x00000000,
        }

        /// <summary>
        /// Determines if the various fields of the touch info are valid
        /// </summary>
        [Flags]
        internal enum TOUCH_MASK : UInt32
        {
            TOUCH_MASK_NONE = 0x00000000,
            TOUCH_MASK_CONTACTAREA = 0x00000001,
            TOUCH_MASK_ORIENTATION = 0x00000002,
            TOUCH_MASK_PRESSURE = 0x00000004,
        }

        /// <summary>
        /// Describes various button states and pen properties
        /// </summary>
        [Flags]
        internal enum PEN_FLAGS : UInt32
        {
            PEN_FLAG_NONE = 0x00000000,
            PEN_FLAG_BARREL = 0x00000001,
            PEN_FLAG_INVERTED = 0x00000002,
            PEN_FLAG_ERASER = 0x00000004,
        }

        /// <summary>
        /// Determines if the various fields of the pen info are valid
        /// </summary>
        [Flags]
        internal enum PEN_MASK : UInt32
        {
            PEN_MASK_NONE = 0x00000000,
            PEN_MASK_PRESSURE = 0x00000001,
            PEN_MASK_ROTATION = 0x00000002,
            PEN_MASK_TILT_X = 0x00000004,
            PEN_MASK_TILT_Y = 0x00000008,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINTER_TOUCH_INFO
        {
            public POINTER_INFO pointerInfo;
            public TOUCH_FLAGS touchFlags;
            public TOUCH_MASK touchMask;
            public RECT rcContact;
            public RECT rcContactRaw;
            public uint orientation;
            public uint pressure;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct POINTER_PEN_INFO
        {
            internal POINTER_INFO pointerInfo;
            internal PEN_FLAGS penFlags;
            internal PEN_MASK penMask;
            internal UInt32 pressure;
            internal UInt32 rotation;
            internal Int32 tiltX;
            internal Int32 tiltY;
        }


        /// <summary>
        /// Gets the data for the current pointer event for the passed pointer id
        /// </summary>
        [DllImport("user32.dll", EntryPoint = "GetPointerInfo", SetLastError = true)]
        private static extern bool GetPointerInfo([In] UInt32 pointerId, [In, Out] ref POINTER_INFO pointerInfo);

        /// <summary>
        /// Gets the pen information for the given pen pointer
        /// </summary>
        [DllImport("user32.dll", EntryPoint = "GetPointerPenInfo", SetLastError = true)]
        private static extern bool GetPointerPenInfo([In] UInt32 pointerId, [In, Out] ref POINTER_PEN_INFO penInfo);

        /// <summary>
        /// Gets the touch information for the given touch pointer
        /// </summary>
        [DllImport("user32.dll", EntryPoint = "GetPointerTouchInfo", SetLastError = true)]
        internal static extern bool GetPointerTouchInfo([In] UInt32 pointerId, [In, Out] ref POINTER_TOUCH_INFO touchInfo);

        private const float PRESSURE_FACTOR = 512f;

        internal static void GetPointerData(int pointerId, out DeviceType deviceType, out float pressure, out float rotation)
        {
            deviceType = DeviceType.Touch;
            pressure = 0f;
            rotation = 0f;

            POINTER_INFO info = new POINTER_INFO();
            bool valid = GetPointerInfo((uint)pointerId, ref info);

            if (!valid)
                return;

            if (info.pointerType == POINTER_INPUT_TYPE.PT_MOUSE || info.pointerType == POINTER_INPUT_TYPE.PT_TOUCHPAD)
            {
                deviceType = DeviceType.Mouse;
                return;
            }

            if (info.pointerType == POINTER_INPUT_TYPE.PT_TOUCH)
            {
                POINTER_TOUCH_INFO touchInfo = new POINTER_TOUCH_INFO();

                valid = GetPointerTouchInfo((uint)pointerId, ref touchInfo);

                if (!valid)
                    return;

                if ((touchInfo.touchMask & TOUCH_MASK.TOUCH_MASK_PRESSURE) == TOUCH_MASK.TOUCH_MASK_PRESSURE)
                    pressure = touchInfo.pressure / PRESSURE_FACTOR;

                if ((touchInfo.touchMask & TOUCH_MASK.TOUCH_MASK_ORIENTATION) == TOUCH_MASK.TOUCH_MASK_ORIENTATION)
                    rotation = (float)((touchInfo.orientation / 180f) / Math.PI);

                return;
            }

            if (info.pointerType == POINTER_INPUT_TYPE.PT_PEN)
            {
                deviceType = DeviceType.Pen;

                POINTER_PEN_INFO penInfo = new POINTER_PEN_INFO();

                valid = GetPointerPenInfo((uint)pointerId, ref penInfo);

                if (!valid)
                    return;

                if ((penInfo.penMask & PEN_MASK.PEN_MASK_PRESSURE) == PEN_MASK.PEN_MASK_PRESSURE)
                    pressure = penInfo.pressure / PRESSURE_FACTOR;

                if ((penInfo.penMask & PEN_MASK.PEN_MASK_ROTATION) == PEN_MASK.PEN_MASK_ROTATION)
                    rotation = (float)((penInfo.rotation / 180f) / Math.PI);

                return;
            }
        }
    }
}
