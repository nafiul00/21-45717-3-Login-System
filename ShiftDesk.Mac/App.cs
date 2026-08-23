using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Presenters;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using ShiftDesk.Mac.Views;

namespace ShiftDesk.Mac
{
    /// <summary>
    /// The interface is built in C# rather than XAML, so it reads side by side
    /// with the Windows Forms designer code.
    ///
    /// The styles below matter more than they look. Fluent ships its own
    /// templates for Button, TextBox and the rest, and those templates set
    /// their own background and foreground on hover, focus and press. Setting
    /// Background on the control itself is not enough - Fluent paints over it
    /// the moment the pointer moves. Each rule here reaches into the template
    /// and replaces the part that actually does the painting.
    /// </summary>
    public class App : Application
    {
        public override void Initialize()
        {
            Styles.Add(new FluentTheme());
            RequestedThemeVariant = ThemeVariant.Light;

            BuildTextBoxStyles();
            BuildButtonStyles();
        }

        // ------------------------------------------------------------------
        // Text inputs
        // ------------------------------------------------------------------

        private void BuildTextBoxStyles()
        {
            Style box = new Style(s => s.OfType<TextBox>());
            box.Setters.Add(new Setter(TextBox.FontSizeProperty, 14.0));
            box.Setters.Add(new Setter(TextBox.ForegroundProperty, Skin.Ink));
            box.Setters.Add(new Setter(TextBox.PaddingProperty, new Thickness(Skin.S3, 10, Skin.S3, 10)));
            box.Setters.Add(new Setter(TextBox.MinHeightProperty, 42.0));
            box.Setters.Add(new Setter(TextBox.CaretBrushProperty, Skin.Accent));
            Styles.Add(box);

            // Resting: white fill, one hairline-but-visible boundary.
            Style border = TemplateBorder(s => s.OfType<TextBox>());
            border.Setters.Add(new Setter(Border.BackgroundProperty, Skin.Surface));
            border.Setters.Add(new Setter(Border.BorderBrushProperty, Skin.Border));
            border.Setters.Add(new Setter(Border.BorderThicknessProperty, new Thickness(1)));
            border.Setters.Add(new Setter(Border.CornerRadiusProperty, new CornerRadius(8)));
            Styles.Add(border);

            // Hover: darken the boundary only, so it reads as reachable.
            Style hover = TemplateBorder(s => s.OfType<TextBox>().Class(":pointerover"));
            hover.Setters.Add(new Setter(Border.BackgroundProperty, Skin.Surface));
            hover.Setters.Add(new Setter(Border.BorderBrushProperty, Skin.Body));
            Styles.Add(hover);

            // Focus: a 2px accent ring. This is the keyboard user's only signal
            // of where they are, so it is the loudest state in the interface.
            Style focus = TemplateBorder(s => s.OfType<TextBox>().Class(":focus"));
            focus.Setters.Add(new Setter(Border.BackgroundProperty, Skin.Surface));
            focus.Setters.Add(new Setter(Border.BorderBrushProperty, Skin.Accent));
            focus.Setters.Add(new Setter(Border.BorderThicknessProperty, new Thickness(2)));
            Styles.Add(focus);
        }

        private static Style TemplateBorder(System.Func<Selector, Selector> control)
        {
            return new Style(s => control(s).Template().OfType<Border>().Name("PART_BorderElement"));
        }

        // ------------------------------------------------------------------
        // Buttons
        // ------------------------------------------------------------------

        private void BuildButtonStyles()
        {
            Style all = new Style(s => s.OfType<Button>());
            all.Setters.Add(new Setter(Button.FontFamilyProperty, new FontFamily(Skin.Display)));
            all.Setters.Add(new Setter(Button.FontSizeProperty, 14.0));
            all.Setters.Add(new Setter(Button.HorizontalContentAlignmentProperty, Avalonia.Layout.HorizontalAlignment.Center));
            all.Setters.Add(new Setter(Button.VerticalContentAlignmentProperty, Avalonia.Layout.VerticalAlignment.Center));
            all.Setters.Add(new Setter(Button.CursorProperty, new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand)));
            Styles.Add(all);

            // -- primary: solid near-black, white label -----------------------
            Fill("primary", null, Skin.Ink, Brushes.White, 0);
            Fill("primary", ":pointerover", Skin.InkHover, Brushes.White, 0);
            Fill("primary", ":pressed", Skin.InkPressed, Brushes.White, 0);
            Weight("primary", FontWeight.SemiBold, 46);

            // -- ghost: outlined, quiet ---------------------------------------
            Fill("ghost", null, Skin.Surface, Skin.Body, 1);
            Fill("ghost", ":pointerover", Skin.Canvas, Skin.Ink, 1);
            Fill("ghost", ":pressed", Skin.Hairline, Skin.Ink, 1);
            Weight("ghost", FontWeight.Normal, 46);

            // -- quiet link: no chrome, no colour -----------------------------
            Fill("quietlink", null, Brushes.Transparent, Skin.Muted, 0);
            Fill("quietlink", ":pointerover", Brushes.Transparent, Skin.Ink, 0);
            Fill("quietlink", ":pressed", Brushes.Transparent, Skin.Ink, 0);
            Weight("quietlink", FontWeight.Medium, 24);
            Size("quietlink", 13.0);

            // -- link: no chrome at all, accent label --------------------------
            Fill("link", null, Brushes.Transparent, Skin.Accent, 0);
            Fill("link", ":pointerover", Skin.AccentWash, Skin.AccentDeep, 0);
            Fill("link", ":pressed", Skin.AccentWash, Skin.AccentDeep, 0);
            Weight("link", FontWeight.SemiBold, 32);
        }

        /// <summary>
        /// Replaces the background, label colour and border of one button class
        /// in one state, inside Fluent's template where the painting happens.
        /// </summary>
        private void Fill(string cls, string pseudo, IBrush background, IBrush foreground, double borderWidth)
        {
            Style style = new Style(s =>
            {
                Selector sel = s.OfType<Button>().Class(cls);
                if (pseudo != null)
                {
                    sel = sel.Class(pseudo);
                }
                return sel.Template().OfType<ContentPresenter>();
            });

            style.Setters.Add(new Setter(ContentPresenter.BackgroundProperty, background));
            style.Setters.Add(new Setter(ContentPresenter.ForegroundProperty, foreground));
            style.Setters.Add(new Setter(ContentPresenter.BorderBrushProperty, Skin.Border));
            style.Setters.Add(new Setter(ContentPresenter.BorderThicknessProperty, new Thickness(borderWidth)));
            style.Setters.Add(new Setter(ContentPresenter.CornerRadiusProperty, new CornerRadius(8)));
            Styles.Add(style);
        }

        private void Size(string cls, double fontSize)
        {
            Style style = new Style(s => s.OfType<Button>().Class(cls));
            style.Setters.Add(new Setter(Button.FontSizeProperty, fontSize));
            Styles.Add(style);
        }

        private void Weight(string cls, FontWeight weight, double height)
        {
            Style style = new Style(s => s.OfType<Button>().Class(cls));
            style.Setters.Add(new Setter(Button.FontWeightProperty, weight));
            style.Setters.Add(new Setter(Button.MinHeightProperty, height));
            style.Setters.Add(new Setter(Button.PaddingProperty, new Thickness(Skin.S4, 0, Skin.S4, 0)));
            Styles.Add(style);
        }

        // ------------------------------------------------------------------

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.ShutdownMode = ShutdownMode.OnMainWindowClose;
                desktop.MainWindow = new LoginWindow();
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
