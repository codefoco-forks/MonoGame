

using System.ComponentModel;
using System.Windows.Forms;

namespace Microsoft.Xna.Framework.Windows
{
    /// <summary>
    /// GameView to render game scene on Forms application
    /// </summary>
    [DesignerCategory("Code")]
    internal class GameControl : Control
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public GameControl()
        {
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
    }
}
