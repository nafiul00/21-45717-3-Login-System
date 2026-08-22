using System;
using System.Windows.Forms;

namespace ShiftDesk
{
    internal static class Program
    {
        /// <summary>
        /// The application opens on the login screen. The version this lab
        /// started from opened on the registration form, which meant the first
        /// thing a user saw was a sign-up page they had not asked for.
        ///
        /// Application.Run is given this one frmLogin instance, so that form is
        /// the application's lifetime. Logout hides and re-shows it rather than
        /// creating a second one - see frmDashboard.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmLogin());
        }
    }
}
