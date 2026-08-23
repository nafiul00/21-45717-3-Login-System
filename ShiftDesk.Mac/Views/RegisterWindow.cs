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
    /// The registration window. Three checks run before anything is written:
    /// no empty fields, the two passwords agree, and the username is free.
    ///
    /// Like the sign-in window it holds no SQL and no connection string. It
    /// calls Data.UserStore, which hashes the password on the way in.
    /// </summary>
    public class RegisterWindow : Window
    {
        private readonly TextBox _username = Skin.Input();
        private readonly TextBox _password = Skin.Input();
        private readonly TextBox _confirm = Skin.Input();
        private readonly Button _reveal = Skin.QuietLink("Show");

        public RegisterWindow()
        {
            Title = "Create an account";
            Width = 480;
            Height = 730;
            CanResize = false;
            Background = Skin.Surface;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            _password.PasswordChar = '•';
            _confirm.PasswordChar = '•';

            Content = BuildLayout();

            _reveal.Click += (s, e) =>
            {
                bool hidden = _password.PasswordChar != '\0';
                char mask = hidden ? '\0' : '•';
                _password.PasswordChar = mask;
                _confirm.PasswordChar = mask;
                _reveal.Content = hidden ? "Hide" : "Show";
            };

            KeyDown += async (s, e) =>
            {
                if (e.Key == Key.Enter)
                {
                    await Register();
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

            page.Children.Add(Skin.Heading("Create an account"));
            page.Children.Add(Skin.Gap(Skin.S2));
            page.Children.Add(Skin.Sub("The password is turned into a SHA-256 hash before it reaches the table."));
            page.Children.Add(Skin.Gap(Skin.S6));

            page.Children.Add(Skin.Label("Username"));
            page.Children.Add(Skin.Gap(Skin.S2));
            page.Children.Add(_username);
            page.Children.Add(Skin.Gap(Skin.S5));

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
            page.Children.Add(Skin.Gap(Skin.S5));

            page.Children.Add(Skin.Label("Confirm password"));
            page.Children.Add(Skin.Gap(Skin.S2));
            page.Children.Add(_confirm);
            page.Children.Add(Skin.Gap(Skin.S6));

            Button create = Skin.Primary("Create account");
            create.HorizontalAlignment = HorizontalAlignment.Stretch;
            create.Click += async (s, e) => await Register();
            page.Children.Add(create);
            page.Children.Add(Skin.Gap(Skin.S3));

            Button clear = Skin.Ghost("Clear");
            clear.Click += (s, e) => ClearFields();

            Button back = Skin.Ghost("Back to sign in");
            back.Click += (s, e) => Close();

            Grid secondary = new Grid { ColumnDefinitions = new ColumnDefinitions("*,12,*") };
            Grid.SetColumn(clear, 0);
            Grid.SetColumn(back, 2);
            secondary.Children.Add(clear);
            secondary.Children.Add(back);
            page.Children.Add(secondary);

            page.Children.Add(Skin.Gap(Skin.S6));
            page.Children.Add(Skin.Rule());
            page.Children.Add(Skin.Gap(Skin.S4));

            TextBlock footer = Skin.Footnote("Usernames are unique. The table enforces it as well as this screen.");
            footer.TextAlignment = TextAlignment.Center;
            page.Children.Add(footer);

            return page;
        }

        private async Task Register()
        {
            string username = (_username.Text ?? "").Trim();
            string password = _password.Text ?? "";
            string confirmation = _confirm.Text ?? "";

            if (username.Length == 0 || password.Length == 0 || confirmation.Length == 0)
            {
                await Dialog.Error(this, "Cannot create account",
                                   "Fill in every field before creating the account.");
                return;
            }

            if (password != confirmation)
            {
                await Dialog.Error(this, "Cannot create account",
                                   "The two passwords are not the same. Type them again.");

                _password.Text = "";
                _confirm.Text = "";
                _password.Focus();
                return;
            }

            try
            {
                // Checked here so the user gets a sentence instead of a
                // constraint violation. The UNIQUE column is what enforces it.
                if (UserStore.UsernameIsTaken(username))
                {
                    await Dialog.Error(this, "Cannot create account",
                        "The username " + username + " is already in use. Pick another one.");

                    _username.SelectAll();
                    _username.Focus();
                    return;
                }

                UserStore.CreateUser(username, password);

                await Dialog.Info(this, "Account created",
                                  "Account created. You can sign in with it now.");

                ClearFields();
            }
            catch (Exception ex)
            {
                await Dialog.Error(this, "Database error",
                                   "Could not reach SQL Server.\n\n" + ex.Message);
            }
        }

        private void ClearFields()
        {
            _username.Text = "";
            _password.Text = "";
            _confirm.Text = "";
            _username.Focus();
        }
    }
}
