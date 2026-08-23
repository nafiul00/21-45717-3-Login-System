using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace ShiftDesk.Mac
{
    /// <summary>
    /// Avalonia has no MessageBox, so this stands in for the MessageBox.Show
    /// calls the Windows Forms build makes: say something, report a problem,
    /// and ask a yes/no question before logging out.
    /// </summary>
    internal static class Dialog
    {
        internal static Task Info(Window owner, string title, string message)
        {
            return Show(owner, title, message, Skin.Accent, false);
        }

        internal static Task Error(Window owner, string title, string message)
        {
            return Show(owner, title, message, Skin.Danger, false);
        }

        internal static async Task<bool> Confirm(Window owner, string title, string message)
        {
            return await Show(owner, title, message, Skin.Accent, true);
        }

        private static async Task<bool> Show(Window owner, string title, string message,
                                             IBrush accent, bool askYesNo)
        {
            bool answer = false;

            Window dialog = new Window
            {
                Title = title,
                Width = 420,
                SizeToContent = SizeToContent.Height,
                CanResize = false,
                Background = Skin.Surface,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ShowInTaskbar = false
            };

            StackPanel body = new StackPanel { Margin = new Thickness(Skin.S6, Skin.S6, Skin.S6, Skin.S5) };

            // A short accent-coloured kicker instead of a heavy title bar.
            body.Children.Add(new TextBlock
            {
                Text = title.ToUpperInvariant(),
                FontFamily = new FontFamily(Skin.Mono),
                FontSize = 11,
                FontWeight = FontWeight.Bold,
                Foreground = accent
            });

            body.Children.Add(Skin.Gap(Skin.S3));

            body.Children.Add(new TextBlock
            {
                Text = message,
                FontFamily = new FontFamily(Skin.Display),
                FontSize = 14,
                Foreground = Skin.Ink,
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 21
            });

            body.Children.Add(Skin.Gap(Skin.S6));

            StackPanel buttons = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Spacing = Skin.S2
            };

            if (askYesNo)
            {
                Button no = Skin.Ghost("Cancel");
                no.Width = 104;
                no.Click += (s, e) => { answer = false; dialog.Close(); };

                Button yes = Skin.Primary("Log out");
                yes.Width = 118;
                yes.Click += (s, e) => { answer = true; dialog.Close(); };

                buttons.Children.Add(no);
                buttons.Children.Add(yes);
            }
            else
            {
                Button ok = Skin.Primary("OK");
                ok.Width = 118;
                ok.Click += (s, e) => dialog.Close();
                buttons.Children.Add(ok);
            }

            body.Children.Add(buttons);
            dialog.Content = body;

            await dialog.ShowDialog(owner);
            return answer;
        }
    }
}
