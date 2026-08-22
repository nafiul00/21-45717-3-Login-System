using System;
using System.Windows.Forms;
using ShiftDesk.Data;
using ShiftDesk.UI;

namespace ShiftDesk
{
    /// <summary>
    /// The registration screen. Three checks run before anything is written:
    /// no empty fields, the two passwords agree, and the username is free.
    ///
    /// Like the login form, it holds no SQL and no connection string. It calls
    /// Data.UserStore, which hashes the password on the way in.
    /// </summary>
    public partial class frmRegister : Form
    {
        public frmRegister()
        {
            InitializeComponent();

            Theme.AttachFocusLine(txtUsername, lineUsername);
            Theme.AttachFocusLine(txtPassword, linePassword);
            Theme.AttachFocusLine(txtConPassword, lineConPassword);
        }

        private void frmRegister_Load(object sender, EventArgs e)
        {
            txtUsername.Focus();
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;
            string confirmation = txtConPassword.Text;

            if (username.Length == 0 || password.Length == 0 || confirmation.Length == 0)
            {
                Warn("Fill in every field before creating the account.");
                return;
            }

            if (password != confirmation)
            {
                Warn("The two passwords are not the same. Type them again.");

                txtPassword.Clear();
                txtConPassword.Clear();
                txtPassword.Focus();
                return;
            }

            try
            {
                // Checked here so the user gets a sentence instead of a
                // constraint violation. The UNIQUE column is what actually
                // enforces it.
                if (UserStore.UsernameIsTaken(username))
                {
                    Warn("The username " + username + " is already in use. Pick another one.");

                    txtUsername.SelectAll();
                    txtUsername.Focus();
                    return;
                }

                UserStore.CreateUser(username, password);

                MessageBox.Show("Account created. You can sign in with it now.",
                                "Account created", MessageBoxButtons.OK, MessageBoxIcon.Information);

                ClearFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not reach SQL Server." + Environment.NewLine + Environment.NewLine +
                                ex.Message,
                                "Database error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearFields();
        }

        private void btnBackToLogin_Click(object sender, EventArgs e)
        {
            // Opened with ShowDialog from the login screen, so closing returns
            // there on its own.
            Close();
        }

        private void chkShowPassword_CheckedChanged(object sender, EventArgs e)
        {
            bool mask = !chkShowPassword.Checked;

            txtPassword.UseSystemPasswordChar = mask;
            txtConPassword.UseSystemPasswordChar = mask;
        }

        private void ClearFields()
        {
            txtUsername.Clear();
            txtPassword.Clear();
            txtConPassword.Clear();
            txtUsername.Focus();
        }

        private void Warn(string message)
        {
            MessageBox.Show(message, "Cannot create account", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
