using System;
using System.Windows.Forms;
using ShiftDesk.Data;
using ShiftDesk.UI;

namespace ShiftDesk
{
    /// <summary>
    /// The sign-in screen, and the form the application runs on.
    ///
    /// There is no data access code here. The username and password are handed
    /// to Data.UserStore, which owns the connection string and the SQL. The
    /// Microsoft Access / System.Data.OleDb code this project started with has
    /// been removed from the solution entirely.
    /// </summary>
    public partial class frmLogin : Form
    {
        public frmLogin()
        {
            InitializeComponent();

            // The rule under each field turns amber while that field has focus.
            // Without it a borderless dark input gives a keyboard user nothing
            // to look at while tabbing through the form.
            Theme.AttachFocusLine(txtUsername, lineUsername);
            Theme.AttachFocusLine(txtPassword, linePassword);
        }

        private void frmLogin_Load(object sender, EventArgs e)
        {
            txtUsername.Focus();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            if (username.Length == 0 || password.Length == 0)
            {
                Warn("Enter a username and a password.");
                return;
            }

            try
            {
                if (UserStore.CredentialsAreValid(username, password))
                {
                    OpenDashboard(username);
                    return;
                }

                // The same message for a bad username and a bad password, on
                // purpose. Saying which one was wrong tells anyone guessing
                // that they have found a real account.
                MessageBox.Show("Those details do not match an account.",
                                "Sign in failed", MessageBoxButtons.OK, MessageBoxIcon.Error);

                txtPassword.Clear();
                txtPassword.Focus();
            }
            catch (Exception ex)
            {
                ShowDatabaseError(ex);
            }
        }

        /// <summary>
        /// Shows the dashboard and hides this window rather than closing it.
        ///
        /// Program.cs runs the application on this instance, so closing it here
        /// would end the process. The dashboard is handed a reference back so
        /// that logging out can bring this same window forward again.
        /// </summary>
        private void OpenDashboard(string username)
        {
            frmDashboard dashboard = new frmDashboard(this, username);

            dashboard.Show();
            Hide();
        }

        /// <summary>
        /// Called by the dashboard immediately before it shows this form again,
        /// so the next person to sign in starts with empty boxes rather than
        /// the previous user's name still sitting there.
        /// </summary>
        internal void ResetForNextUser()
        {
            txtUsername.Clear();
            txtPassword.Clear();
            chkShowPassword.Checked = false;
            txtUsername.Focus();
        }

        private void btnGoToRegister_Click(object sender, EventArgs e)
        {
            // ShowDialog leaves this window in place underneath, so closing the
            // registration form lands straight back here.
            using (frmRegister register = new frmRegister())
            {
                register.ShowDialog(this);
            }

            txtUsername.Focus();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtUsername.Clear();
            txtPassword.Clear();
            txtUsername.Focus();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void chkShowPassword_CheckedChanged(object sender, EventArgs e)
        {
            // Ticked means reveal, so the mask is the opposite of the tick.
            txtPassword.UseSystemPasswordChar = !chkShowPassword.Checked;
        }

        private void Warn(string message)
        {
            MessageBox.Show(message, "Sign in", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void ShowDatabaseError(Exception ex)
        {
            MessageBox.Show("Could not reach SQL Server." + Environment.NewLine + Environment.NewLine +
                            ex.Message + Environment.NewLine + Environment.NewLine +
                            "Check that database.sql has been run and that Data Source in App.config " +
                            "matches this machine's SQL Server.",
                            "Database error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
