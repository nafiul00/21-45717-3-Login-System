using System;
using System.Windows.Forms;

namespace ShiftDesk
{
    /// <summary>
    /// The screen behind the login. Reaching it is the proof that SQL Server
    /// matched the username and the password hash against a row in tbl_users.
    /// </summary>
    public partial class frmDashboard : Form
    {
        private readonly frmLogin _loginWindow;
        private readonly string _username;

        /// <summary>
        /// The login window and the signed-in username are required, so the
        /// dashboard cannot be constructed in a state where logout has nowhere
        /// to go back to.
        /// </summary>
        public frmDashboard(frmLogin loginWindow, string username)
        {
            InitializeComponent();

            _loginWindow = loginWindow;
            _username = username;
        }

        private void frmDashboard_Load(object sender, EventArgs e)
        {
            lblWelcome.Text = "Signed in as " + _username;
        }

        /// <summary>
        /// Logging out means going back to the sign-in screen. The version this
        /// project started from called Application.Exit() here, which quit the
        /// program - that is closing the application, not logging out of it.
        ///
        /// This only asks the question. Actually restoring the sign-in window is
        /// done in FormClosed, so that it happens however this form was closed.
        /// </summary>
        private void btnLogout_Click(object sender, EventArgs e)
        {
            DialogResult answer = MessageBox.Show("Log out of ShiftDesk?", "Log out",
                                                  MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (answer == DialogResult.Yes)
            {
                Close();
            }
        }

        /// <summary>
        /// Runs whether the user pressed Log out or closed this window with the
        /// X in the corner. Both have to bring the sign-in window back.
        ///
        /// If only the Log out button did it, closing the dashboard with the X
        /// would leave the sign-in form hidden and the process alive with
        /// nothing at all on screen - gone from the taskbar, still in Task
        /// Manager. Application.Run is holding that form, so it is the thing
        /// that has to be visible again.
        ///
        /// The original login window is re-shown rather than a new one being
        /// created. A second frmLogin would leave the first alive and invisible
        /// and put the process back in exactly the state described above.
        /// </summary>
        private void frmDashboard_FormClosed(object sender, FormClosedEventArgs e)
        {
            _loginWindow.ResetForNextUser();
            _loginWindow.Show();
        }
    }
}
