// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Microsoft.Xna.Framework.Platform.Windows;
using MonoGame.Framework;


namespace Microsoft.Xna.Framework.Windows
{
    internal static class MessageExtensions
    {     
        public static int GetPointerId(this Message msg)
        {
            return (short)msg.WParam;
        }

        public static System.Drawing.Point GetPointerLocation(this Message msg)
        {
            var lowword = msg.LParam.ToInt32();

            return new System.Drawing.Point()
                       {
                           X = (short)(lowword),
                           Y = (short)(lowword >> 16),
                       };
        }
    }

    [System.ComponentModel.DesignerCategory("Code")]
    internal sealed class WinFormsGameForm : Form
    {
        private readonly WinFormsGameWindow _window;

        public const int WM_MOUSEHWHEEL = 0x020E;
        public const int WM_KEYDOWN = 0x0100;
        public const int WM_KEYUP = 0x0101;
        public const int WM_SYSKEYDOWN = 0x0104;
        public const int WM_SYSKEYUP = 0x0105;
        public const int WM_TABLET_QUERYSYSTEMGESTURESTA = (0x02C0 + 12);

        public const int WM_ENTERSIZEMOVE = 0x0231;
        public const int WM_EXITSIZEMOVE = 0x0232;
        public const int WM_DROPFILES = 0x0233;

        public const int WM_SYSCOMMAND = 0x0112;
        public const int WM_DPICHANGED = 0x02E0;

        const uint SWP_NOACTIVATE = 0x0010;
        const uint SWP_NOZORDER = 0x0004;

        public WinFormsGameForm(WinFormsGameWindow window, Control gameView)
        {
            _window = window;
            gameView.Dock = DockStyle.Fill;
            Controls.Add(gameView);
        }

        public static void CenterOnPrimaryMonitor(Form self)
        {
            self.Location = new System.Drawing.Point(
                 (Screen.PrimaryScreen.WorkingArea.Width  - self.Width) / 2,
                 (Screen.PrimaryScreen.WorkingArea.Height - self.Height) / 2);
        }

        protected override void WndProc(ref Message m)
        {
            WndProc(ref m, _window);
            base.WndProc(ref m);
        }

        public static void WndProc(ref Message m, WinFormsGameWindow window)
        {
            Control gameView = window.GameView;
            Form form = window.GameForm;

            switch (m.Msg)
            {
                case WM_DPICHANGED:
                    {
#if NET_4_0
                        RECT suggestedRect = (RECT)Marshal.PtrToStructure(m.LParam, typeof(RECT));
#else
                        RECT suggestedRect = Marshal.PtrToStructure<RECT>(m.LParam);
#endif

                        SetWindowPos(window.Handle,
                                     IntPtr.Zero,
                                     suggestedRect.left,
                                     suggestedRect.top,
                                     suggestedRect.right - suggestedRect.left,
                                     suggestedRect.bottom - suggestedRect.top,
                                     SWP_NOZORDER | SWP_NOACTIVATE);

                        m.Result = IntPtr.Zero;
                    }
                    break;
                case WM_TABLET_QUERYSYSTEMGESTURESTA:
                    {
                        // This disables the windows touch helpers, popups, and 
                        // guides that get in the way of touch gameplay.
                        const int flags = 0x00000001 | // TABLET_DISABLE_PRESSANDHOLD
                                          0x00000008 | // TABLET_DISABLE_PENTAPFEEDBACK
                                          0x00000010 | // TABLET_DISABLE_PENBARRELFEEDBACK
                                          0x00000100 | // TABLET_DISABLE_TOUCHUIFORCEON
                                          0x00000200 | // TABLET_DISABLE_TOUCHUIFORCEOFF
                                          0x00008000 | // TABLET_DISABLE_TOUCHSWITCH
                                          0x00010000 | // TABLET_DISABLE_FLICKS
                                          0x00080000 | // TABLET_DISABLE_SMOOTHSCROLLING 
                                          0x00100000; // TABLET_DISABLE_FLICKFALLBACKKEYS
                        m.Result = new IntPtr(flags);
                        return;
                    }
#if (WINDOWS && DIRECTX)
                case WM_KEYDOWN:
                    HandleKeyMessage(ref m, window);
                    switch (m.WParam.ToInt32())
                    {
                        case 0x5B:  // Left Windows Key
                        case 0x5C: // Right Windows Key

                            if (window.IsFullScreen && window.HardwareModeSwitch && form != null)
                                form.WindowState = FormWindowState.Minimized;

                            break;
                    }
                    break;
                case WM_SYSKEYDOWN:
                    HandleKeyMessage(ref m, window);
                    break;
                case WM_KEYUP:
                case WM_SYSKEYUP:
                    HandleKeyMessage(ref m, window);
                    break;

                case WM_DROPFILES:
                    HandleDropMessage(ref m, window);
                    break;
#endif
                case WM_SYSCOMMAND:

                    var wParam = m.WParam.ToInt32();

                    if (!window.AllowAltF4 && wParam == 0xF060 && m.LParam.ToInt32() == 0 && gameView.Focused)
                    {
                        m.Result = IntPtr.Zero;
                        return;
                    }

                    // Disable the system menu from being toggled by
                    // keyboard input so we can own the ALT key.
                    if (wParam == 0xF100) // SC_KEYMENU
                    {
                        m.Result = IntPtr.Zero;
                        return;
                    }
                    break;

                case WM_MOUSEHWHEEL:
                    var delta = (short)(((ulong)m.WParam >> 16) & 0xffff); ;
                    var handler = window.MouseHorizontalWheel;

                    if (handler != null)
                        handler(window, new HorizontalMouseWheelEventArgs(delta));
                    break;
                case WM_ENTERSIZEMOVE:
                    window.IsResizing = true;
                    break;
                case WM_EXITSIZEMOVE:
                    window.IsResizing = false;
                    break;
            }
        }

        public static void HandleKeyMessage(ref Message m, GameWindow window)
        {
            long virtualKeyCode = m.WParam.ToInt64();
            bool extended = (m.LParam.ToInt64() & 0x01000000) != 0;
            long scancode = (m.LParam.ToInt64() & 0x00ff0000) >> 16;
            var key = KeyCodeTranslate(
                (Keys)virtualKeyCode,
                extended,
                scancode);
            if (Input.KeysHelper.IsKey((int)key))
            {
                switch (m.Msg)
                {
                    case WM_KEYDOWN:
                    case WM_SYSKEYDOWN:
                        window.OnKeyDown(new InputKeyEventArgs(key));
                        break;
                    case WM_KEYUP:
                    case WM_SYSKEYUP:
                        window.OnKeyUp(new InputKeyEventArgs(key));
                        break;
                    default:
                        break;
                }
            }

        }

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern uint DragQueryFile(IntPtr hDrop, uint iFile,
            [Out] StringBuilder lpszFile, uint cch);

        private static void HandleDropMessage(ref Message m, WinFormsGameWindow window)
        {
            IntPtr hdrop = m.WParam;

            uint count = DragQueryFile(hdrop, uint.MaxValue, null, 0);

            string[] files = new string[count];
            for (uint i = 0; i < count; i++)
            {
                uint buffSize = DragQueryFile(hdrop, i, null, int.MaxValue);
                StringBuilder builder = new StringBuilder((int)buffSize);
                DragQueryFile(hdrop, i, builder, buffSize);
                files[i] = builder.ToString();
            }

            window.OnFileDrop(new FileDropEventArgs(files));
            m.Result = IntPtr.Zero;
        }

        private static Input.Keys KeyCodeTranslate(
            Keys keyCode, bool extended, long scancode)
        {
            switch (keyCode)
            {
                // WinForms does not distinguish between left/right keys
                // We have to check for special keys such as control/shift/alt/ etc
                case Keys.ControlKey:
                    return extended
                        ? Input.Keys.RightControl
                        : Input.Keys.LeftControl;
                case Keys.ShiftKey:
                    // left shift is 0x2A, right shift is 0x36. IsExtendedKey is always false.
                    return ((scancode & 0x1FF) == 0x36)
                               ? Input.Keys.RightShift
                                : Input.Keys.LeftShift;
                // Note that the Alt key is now refered to as Menu.
                case Keys.Menu:
                case Keys.Alt:
                    return extended
                        ? Input.Keys.RightAlt
                        : Input.Keys.LeftAlt;

                default:
                    return (Input.Keys)keyCode;
            }
        }
    }
}
