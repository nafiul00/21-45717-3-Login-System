using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace ShiftDesk.Mac.Views
{
    /// <summary>
    /// The window behind the sign-in. Reaching it is the proof that SQL Server
    /// matched the username and the password hash against a row in tbl_users.
    /// </summary>
    public class DashboardWindow : Window
    {
        private readonly LoginWindow _loginWindow;
        private bool _loginRestored;

        /// <summary>
        /// The sign-in window and the username are required, so the dashboard
        /// cannot be built in a state where logging out has nowhere to go back to.
        /// </summary>
        public DashboardWindow(LoginWindow loginWindow, string username)
        {
            _loginWindow = loginWindow;

            Title = "ShiftDesk - Roster console";
            Width = 660;
            Height = 444;
            CanResize = false;
            Background = Skin.Surface;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            Content = BuildLayout(username);

            // Runs whether the user pressed Log out or closed this window with
            // the red button. Both have to bring the sign-in window back - if
            // only Log out did it, closing this window would leave the sign-in
            // window hidden and the application running with nothing on screen.
            Closed += (s, e) => RestoreLogin();
        }

        private Control BuildLayout(string username)
        {
            StackPanel page = new StackPanel
            {
                Margin = new Thickness(Skin.S8, 40, Skin.S8, Skin.S7)
            };

            // Wordmark on the left, the signed-in user on the right.
            Grid top = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto") };

            Control mark = Skin.Wordmark();
            Grid.SetColumn(mark, 0);
            top.Children.Add(mark);

            Border chip = new Border
            {
                Background = Skin.AccentWash,
                CornerRadius = new CornerRadius(999),
                Padding = new Thickness(Skin.S3, 6, Skin.S3, 6),
                VerticalAlignment = VerticalAlignment.Center,
                Child = new TextBlock
                {
                    Text = username,
                    FontFamily = new FontFamily(Skin.Mono),
                    FontSize = 12,
                    FontWeight = FontWeight.SemiBold,
                    Foreground = Skin.Accent
                }
            };
            Grid.SetColumn(chip, 1);
            top.Children.Add(chip);

            page.Children.Add(top);
            page.Children.Add(Skin.Gap(Skin.S7));

            page.Children.Add(Skin.Heading("You are signed in"));
            page.Children.Add(Skin.Gap(Skin.S2));
            page.Children.Add(Skin.Sub("There is no way to reach this screen without a successful sign in."));
            page.Children.Add(Skin.Gap(Skin.S6));

            // The one explanatory panel, in a washed accent card.
            StackPanel cardBody = new StackPanel { Margin = new Thickness(Skin.S5, Skin.S4, Skin.S5, Skin.S4) };

            cardBody.Children.Add(new TextBlock
            {
                Text = "HOW YOU GOT HERE",
                FontFamily = new FontFamily(Skin.Mono),
                FontSize = 11,
                FontWeight = FontWeight.Bold,
                Foreground = Skin.Accent
            });

            cardBody.Children.Add(Skin.Gap(Skin.S2));

            cardBody.Children.Add(new TextBlock
            {
                Text = "SQL Server was asked how many rows of tbl_users carry both this username " +
                       "and the SHA-256 hash of the password that was typed. The answer came back " +
                       "as exactly one, so this window opened.",
                FontFamily = new FontFamily(Skin.Display),
                FontSize = 13,
                Foreground = Skin.Body,
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 20
            });

            page.Children.Add(new Border
            {
                Background = Skin.AccentWash,
                CornerRadius = new CornerRadius(10),
                Child = cardBody
            });

            page.Children.Add(Skin.Gap(Skin.S6));

            Button logout = Skin.Primary("Log out");
            logout.Width = 200;
            logout.HorizontalAlignment = HorizontalAlignment.Left;
            logout.Click += async (s, e) =>
            {
                // This only asks the question. Restoring the sign-in window is
                // done in Closed, so it happens however this window was closed.
                bool sure = await Dialog.Confirm(this, "Log out", "Log out of ShiftDesk?");

                if (sure)
                {
                    Close();
                }
            };
            page.Children.Add(logout);

            return page;
        }

        private void RestoreLogin()
        {
            if (_loginRestored)
            {
                return;
            }

            _loginRestored = true;

            _loginWindow.ResetForNextUser();
            _loginWindow.Show();
            _loginWindow.Activate();
        }
    }
}
