using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using ShiftDesk.Data;

namespace ShiftDesk.Mac.Views
{
    /// <summary>
    /// The sign-in window, and the window the application runs on.
    ///
    /// There is no data access here. The username and password go to
    /// Data.UserStore - the same file the Windows Forms build compiles - which
    /// owns the connection string and every SQL statement in the project.
    /// </summary>
    public class LoginWindow : Window
    {
        private readonly TextBox _username = Skin.Input();
        private readonly TextBox _password = Skin.Input();
        private readonly Button _reveal = Skin.QuietLink("Show");

        public LoginWindow()
        {
            Title = "ShiftDesk";
            Width = 480;
            Height = 664;
            CanResize = false;
            Background = Skin.Surface;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            _password.PasswordChar = '•';

            Content = BuildLayout();

            _reveal.Click += (s, e) =>
            {
                bool hidden = _password.PasswordChar != '\0';
                _password.PasswordChar = hidden ? '\0' : '•';
                _reveal.Content = hidden ? "Hide" : "Show";
            };

            KeyDown += async (s, e) =>
            {
                if (e.Key == Key.Enter)
                {
                    await SignIn();
                }
            };

            Opened += (s, e) => _username.Focus();
        }

        private Control BuildLayout()
        {
            StackPanel page = new StackPanel
            {
                Margin = new Thickness(Skin.S8, 44, Skin.S8, Skin.S7)
            };

            page.Children.Add(Skin.Wordmark());
            page.Children.Add(Skin.Gap(Skin.S7));

            page.Children.Add(Skin.Heading("Welcome back"));
            page.Children.Add(Skin.Gap(Skin.S2));
            page.Children.Add(Skin.Sub("Sign in to the roster console. Accounts live in SQL Server."));
            page.Children.Add(Skin.Gap(Skin.S6));

            page.Children.Add(Skin.Label("Username"));
            page.Children.Add(Skin.Gap(Skin.S2));
            page.Children.Add(_username);
            page.Children.Add(Skin.Gap(Skin.S5));

            // Label on the left, the reveal toggle pushed to the right of it.
            Grid passwordRow = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto") };
            Control passwordLabel = Skin.Label("Password");
            passwordLabel.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(passwordLabel, 0);
            passwordRow.Children.Add(passwordLabel);

            _reveal.Padding = new Thickness(Skin.S2, 0, 0, 0);
            _reveal.MinHeight = 20;
            Grid.SetColumn(_reveal, 1);
            passwordRow.Children.Add(_reveal);

            page.Children.Add(passwordRow);
            page.Children.Add(Skin.Gap(Skin.S2));
            page.Children.Add(_password);
            page.Children.Add(Skin.Gap(Skin.S6));

            Button signIn = Skin.Primary("Sign in");
            signIn.HorizontalAlignment = HorizontalAlignment.Stretch;
            signIn.Click += async (s, e) => await SignIn();
            page.Children.Add(signIn);
            page.Children.Add(Skin.Gap(Skin.S3));

            Button clear = Skin.Ghost("Clear");
            clear.Click += (s, e) =>
            {
                _username.Text = "";
                _password.Text = "";
                _username.Focus();
            };

            Button exit = Skin.Ghost("Exit");
            exit.Click += (s, e) => Close();

            Grid secondary = new Grid { ColumnDefinitions = new ColumnDefinitions("*,12,*") };
            Grid.SetColumn(clear, 0);
            Grid.SetColumn(exit, 2);
            secondary.Children.Add(clear);
            secondary.Children.Add(exit);
            page.Children.Add(secondary);

            page.Children.Add(Skin.Gap(Skin.S6));
            page.Children.Add(Skin.Rule());
            page.Children.Add(Skin.Gap(Skin.S4));

            StackPanel signUp = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Spacing = Skin.S1
            };
            TextBlock prompt = Skin.Footnote("No account yet?");
            prompt.VerticalAlignment = VerticalAlignment.Center;
            signUp.Children.Add(prompt);

            Button create = Skin.Link("Create one");
            create.Click += async (s, e) =>
            {
                RegisterWindow window = new RegisterWindow();
                await window.ShowDialog(this);
                _username.Focus();
            };
            signUp.Children.Add(create);
            page.Children.Add(signUp);

            page.Children.Add(Skin.Gap(Skin.S4));

            TextBlock footer = Skin.Footnote("Passwords are stored as SHA-256 hashes, never as readable text.");
            footer.TextAlignment = TextAlignment.Center;
            page.Children.Add(footer);

            return page;
        }

        private async Task SignIn()
        {
            string username = (_username.Text ?? "").Trim();
            string password = _password.Text ?? "";

            if (username.Length == 0 || password.Length == 0)
            {
                await Dialog.Error(this, "Sign in", "Enter a username and a password.");
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
                // purpose. Saying which one was wrong tells anyone guessing that
                // they have found a real account.
                await Dialog.Error(this, "Sign in failed", "Those details do not match an account.");

                _password.Text = "";
                _password.Focus();
            }
            catch (Exception ex)
            {
                await Dialog.Error(this, "Database error",
                    "Could not reach SQL Server.\n\n" + ex.Message +
                    "\n\nCheck the SQL Server container is running and that database.sql has been run.");
            }
        }

        /// <summary>
        /// Shows the dashboard and hides this window rather than closing it.
        /// This is the application's main window, so closing it here would end
        /// the program. The dashboard is handed a reference back so that logging
        /// out can bring this same window forward again.
        /// </summary>
        private void OpenDashboard(string username)
        {
            DashboardWindow dashboard = new DashboardWindow(this, username);
            dashboard.Show();
            Hide();
        }

        /// <summary>
        /// Called by the dashboard just before it shows this window again, so
        /// the next person starts with empty boxes.
        /// </summary>
        internal void ResetForNextUser()
        {
            _username.Text = "";
            _password.Text = "";
            _password.PasswordChar = '•';
            _reveal.Content = "Show";
            _username.Focus();
        }
    }
}
