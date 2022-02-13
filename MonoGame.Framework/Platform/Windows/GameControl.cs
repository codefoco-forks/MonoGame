using System;
using System.ComponentModel;
using System.Windows.Forms;

using Microsoft.Xna.Framework.Input.Touch;

namespace Microsoft.Xna.Framework.Windows
{
    /// <summary>
    /// GameView to render game scene on Forms application
    /// </summary>
    [DesignerCategory("Code")]
    public class GameControl : Control
    {
        private GameWindow _window;

        private const int WM_POINTERUP = 0x0247;
        private const int WM_POINTERDOWN = 0x0246;
        private const int WM_POINTERUPDATE = 0x0245;

        /// <summary>
        /// Constructor
        /// </summary>
        public GameControl(GameWindow window)
        {
            _window = window;

            SetStyle(ControlStyles.Opaque, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint,
                     true);
        }

        /// <summary>
        /// Paint event
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
        }

        /// <summary>
        /// Paint Background event
        /// </summary>
        /// <param name="pevent"></param>
        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
        }

        /// <summary>
        /// Handle Win32 messages
        /// </summary>
        /// <param name="m"></param>
        protected override void WndProc(ref Message m)
        {
            bool callBase;
            WndProc(ref m, _window, this, out callBase);

            if (callBase)
                base.WndProc(ref m);
        }

        /// <summary>
        /// When embedding an WinForms Control as GameView use this WndProc
        /// to handle the touch events
        /// </summary>
        /// <param name="m"></param>
        /// <param name="window"></param>
        /// <param name="gameView"></param>
        public static void WndProc(ref Message m, GameWindow window, Control gameView, out bool callBase)
        {
            TouchLocationState state = TouchLocationState.Invalid;
            callBase = true;

            switch (m.Msg)
            {
                case WinFormsGameForm.WM_TABLET_QUERYSYSTEMGESTURESTA:
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
                case WM_POINTERUP:
                    state = TouchLocationState.Released;
                    break;
                case WM_POINTERDOWN:
                    state = TouchLocationState.Pressed;
                    break;
                case WM_POINTERUPDATE:
                    state = TouchLocationState.Moved;
                    break;
            }

            if (state == TouchLocationState.Invalid)
                return;

            callBase = false;
            int id = m.GetPointerId();
            DeviceType deviceType;
            float pressure;
            float rotation;
            TouchInfo.GetPointerData(id, out deviceType, out pressure, out rotation);

            var position = m.GetPointerLocation();
            position = gameView.PointToClient(position);

            var vec = new Vector2(position.X, position.Y);

            m.Result = IntPtr.Zero;

            window.TouchPanelState.AddEvent(id, state, vec, deviceType, pressure, rotation);
        }
    }
}
