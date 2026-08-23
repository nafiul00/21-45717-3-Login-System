using System;
using Avalonia;

namespace ShiftDesk.Mac
{
    internal static class Program
    {
        /// <summary>
        /// The application opens on the sign-in window, the same as the Windows
        /// Forms build. Avalonia treats that window as the main one, so closing
        /// it ends the program and closing the dashboard does not.
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

        public static AppBuilder BuildAvaloniaApp()
        {
            return AppBuilder.Configure<App>()
                             .UsePlatformDetect()
                             .LogToTrace();
        }
    }
}
