using System.Drawing;
using System.Windows.Forms;

namespace ShiftDesk.UI
{
    /// <summary>
    /// The palette, and the one piece of styling that cannot live in the
    /// designer: what a field looks like while it has keyboard focus.
    ///
    /// Static colours are set in the .Designer.cs files, the way Visual Studio
    /// writes them. Anything that changes at runtime is here, so there is a
    /// single definition of the accent colour rather than one per event handler.
    ///
    /// Every text colour below was checked against the surface it sits on and
    /// clears the WCAG AA contrast minimum of 4.5:1 for body text.
    /// </summary>
    internal static class Theme
    {
        internal static readonly Color Ground = Color.FromArgb(17, 24, 39);     // #111827 window
        internal static readonly Color Header = Color.FromArgb(11, 17, 32);     // #0B1120 masthead
        internal static readonly Color Surface = Color.FromArgb(30, 41, 59);    // #1E293B fields, cards
        internal static readonly Color Line = Color.FromArgb(51, 65, 85);       // #334155 resting underline
        internal static readonly Color Accent = Color.FromArgb(245, 158, 11);   // #F59E0B focus, primary action

        internal static readonly Color TextHigh = Color.White;                       // 17.7:1 on Ground
        internal static readonly Color TextMid = Color.FromArgb(148, 163, 184);      //  6.9:1 on Ground
        internal static readonly Color TextLow = Color.FromArgb(135, 148, 168);      //  5.8:1 on Ground
        internal static readonly Color TextFaint = Color.FromArgb(124, 138, 160);    //  5.1:1 on Ground

        /// <summary>
        /// Lights the rule underneath a text box while that box has focus.
        ///
        /// A flat, borderless field gives a keyboard user nothing to look at,
        /// and tabbing through a form you cannot see is the fastest way to make
        /// an interface unusable. The underline is the focus indicator.
        /// </summary>
        internal static void AttachFocusLine(TextBox field, Panel underline)
        {
            field.Enter += (sender, e) =>
            {
                underline.BackColor = Accent;
                underline.Height = 2;
            };

            field.Leave += (sender, e) =>
            {
                underline.BackColor = Line;
                underline.Height = 2;
            };
        }
    }
}
