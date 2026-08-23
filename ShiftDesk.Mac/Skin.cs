using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace ShiftDesk.Mac
{
    /// <summary>
    /// The palette and the shared pieces of the interface.
    ///
    /// Named Skin rather than Theme because Avalonia already puts a Theme
    /// property on every Control, and a static class called Theme is shadowed
    /// by it inside any class deriving from Control.
    ///
    /// The register is a light, typographic product interface: warm off-white
    /// ground, near-black type and buttons, one burnt-orange accent used only
    /// for the brand mark, focus rings and links. No gradients, no glass, no
    /// decorative colour.
    ///
    /// Every value below was measured against the surface behind it:
    ///
    ///   Ink      #1C1917 on white  17.5:1
    ///   Body     #57534E on white   7.6:1
    ///   Muted    #78716C on white   4.8:1
    ///   Accent   #C2410C on white   5.2:1   (text and focus ring alike)
    ///   Border   #948E88 on white   3.2:1   (clears the 3:1 minimum for a
    ///                                        control boundary; #A8A29E, the
    ///                                        obvious choice, is 2.5:1 and does not)
    /// </summary>
    internal static class Skin
    {
        internal static readonly IBrush Canvas = New(250, 250, 249);   // #FAFAF9 window ground
        internal static readonly IBrush Surface = New(255, 255, 255);  // #FFFFFF cards, inputs
        internal static readonly IBrush Hairline = New(231, 229, 228); // #E7E5E4 decorative rules only
        internal static readonly IBrush Border = New(148, 142, 136);   // #948E88 control boundaries
        internal static readonly IBrush Ink = New(28, 25, 23);         // #1C1917 headings, buttons
        internal static readonly IBrush InkHover = New(41, 37, 36);    // #292524
        internal static readonly IBrush InkPressed = New(12, 10, 9);   // #0C0A09
        internal static readonly IBrush Body = New(87, 83, 78);        // #57534E
        internal static readonly IBrush Muted = New(120, 113, 108);    // #78716C
        internal static readonly IBrush Accent = New(194, 65, 12);     // #C2410C
        internal static readonly IBrush AccentDeep = New(154, 52, 18); // #9A3412
        internal static readonly IBrush AccentWash = New(255, 247, 237);// #FFF7ED
        internal static readonly IBrush Danger = New(190, 24, 93);     // #BE185D

        private static SolidColorBrush New(byte r, byte g, byte b)
        {
            return new SolidColorBrush(Color.FromRgb(r, g, b));
        }

        // A single type stack, so the three windows cannot drift apart.
        internal const string Display = "Helvetica Neue, Segoe UI, Arial, sans-serif";
        internal const string Mono = "SF Mono, Menlo, Consolas, monospace";

        // One spacing scale. Every gap in the application is a value from here,
        // which is what stops the layout looking hand-placed.
        internal const double S1 = 4, S2 = 8, S3 = 12, S4 = 16, S5 = 24, S6 = 32, S7 = 40, S8 = 48;

        /// <summary>The wordmark: a small accent tile and the product name.</summary>
        internal static Control Wordmark()
        {
            Border tile = new Border
            {
                Background = Accent,
                Width = 30,
                Height = 30,
                CornerRadius = new CornerRadius(8),
                Child = new TextBlock
                {
                    Text = "S",
                    FontFamily = new FontFamily(Display),
                    FontSize = 16,
                    FontWeight = FontWeight.Bold,
                    Foreground = Brushes.White,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
            };

            StackPanel row = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = S3,
                VerticalAlignment = VerticalAlignment.Center
            };
            row.Children.Add(tile);
            row.Children.Add(new TextBlock
            {
                Text = "ShiftDesk",
                FontFamily = new FontFamily(Display),
                FontSize = 17,
                FontWeight = FontWeight.SemiBold,
                Foreground = Ink,
                VerticalAlignment = VerticalAlignment.Center
            });
            return row;
        }

        internal static TextBlock Heading(string text)
        {
            return new TextBlock
            {
                Text = text,
                FontFamily = new FontFamily(Display),
                FontSize = 27,
                FontWeight = FontWeight.SemiBold,
                Foreground = Ink,
                LineHeight = 34
            };
        }

        internal static TextBlock Sub(string text)
        {
            return new TextBlock
            {
                Text = text,
                FontFamily = new FontFamily(Display),
                FontSize = 14,
                Foreground = Muted,
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 21
            };
        }

        internal static TextBlock Label(string text)
        {
            return new TextBlock
            {
                Text = text,
                FontFamily = new FontFamily(Display),
                FontSize = 13,
                FontWeight = FontWeight.Medium,
                Foreground = Body
            };
        }

        internal static TextBlock Footnote(string text)
        {
            return new TextBlock
            {
                Text = text,
                FontFamily = new FontFamily(Display),
                FontSize = 12,
                Foreground = Muted,
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 18
            };
        }

        internal static Border Rule()
        {
            return new Border { Background = Hairline, Height = 1 };
        }

        /// <summary>Vertical gap from the spacing scale.</summary>
        internal static Control Gap(double height)
        {
            return new Panel { Height = height };
        }

        internal static TextBox Input(string watermark = "")
        {
            return new TextBox { Watermark = watermark, FontFamily = new FontFamily(Display) };
        }

        internal static Button Primary(string caption)
        {
            Button b = new Button { Content = caption };
            b.Classes.Add("primary");
            return b;
        }

        internal static Button Ghost(string caption)
        {
            // Ghost buttons always sit in a grid column and fill it. Left to
            // themselves they size to their text, which leaves a ragged gap
            // between two of them sitting side by side.
            Button b = new Button
            {
                Content = caption,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            b.Classes.Add("ghost");
            return b;
        }

        /// <summary>
        /// A link with no colour of its own. Used where a second accent link
        /// would compete with the real one - the password reveal toggle sits
        /// next to "Create one", and only one of the two should pull the eye.
        /// </summary>
        internal static Button QuietLink(string caption)
        {
            Button b = new Button { Content = caption };
            b.Classes.Add("quietlink");
            return b;
        }

        internal static Button Link(string caption)
        {
            Button b = new Button { Content = caption };
            b.Classes.Add("link");
            return b;
        }
    }
}
