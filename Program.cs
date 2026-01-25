using lain.helpers;
using MonoTorrent;
using MonoTorrent.Client;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Terminal.Gui;

namespace lain
{
    /// <summary>
    /// Entry point for the Lain application.
    /// Initializes settings, plugins, and starts the Terminal.Gui interface.
    /// </summary>
    /// <author>George Hunter S.</author>
    /// <created>Jan, 2026</created>
    class Program
    {
        public static async Task Main(string[] args)
        {

            // ------------------------------
            // CLI Entry point (will implement later)
            // ------------------------------
            if (args.Length > 0)
            {
                return;
            }

            else {

                // ------------------------------
                // Optional debug: change culture to Japanese
                // ------------------------------
                CultureInfo ci = new("ja-JP");
                //Thread.CurrentThread.CurrentUICulture = ci;

                // Register encodings for legacy code pages (e.g., Shift-JIS)
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                // Enable UTF-8
                Application.UseSystemConsole = true;
                Console.InputEncoding = Encoding.UTF8;
                Console.OutputEncoding = Encoding.UTF8;


                // ------------------------------
                // Load settings and plugins
                // ------------------------------
                Settings.Load(); // load user settings from file
                Ghidorah.LoadQbittorrentPlugins(); // load qbittorrent plugin sources

                // Disable QB plugin usage if none loaded
                if (Ghidorah.QbSources == null || Ghidorah.QbSources.Length == 0)
                {
                    Settings.Current.UseQbittorrentPlugins = false;
                }

                // ------------------------------
                // Initialize Terminal.Gui
                // ------------------------------
                Application.Init();

                // Top-level container for windows
                var top = Application.Top;

                // Main window (contains header, sidebar, and content panels)
                var mainWin = new LainUI();

                // ------------------------------
                // Setup color scheme for the main window
                // ------------------------------
                ColorScheme myScheme = new()
                {
                    Normal = Application.Driver.MakeAttribute(Settings.Current.TextColor, Settings.Current.BackgroundColor), // normal text
                    Focus = Application.Driver.MakeAttribute(Settings.Current.FocusTextColor, Settings.Current.FocusBackgroundColor), // focused element
                    HotNormal = Application.Driver.MakeAttribute(Settings.Current.HotTextColor, Settings.Current.BackgroundColor), // hotkey text
                    HotFocus = Application.Driver.MakeAttribute(Settings.Current.FocusTextColor, Settings.Current.FocusBackgroundColor), // focused hotkey
                };

                mainWin.ColorScheme = myScheme;

                // Add main window to the top-level container
                top.Add(mainWin);

                // ------------------------------
                // Run the application
                // ------------------------------
                try
                {
                    Application.Run(); // blocks until user exits
                }
                finally
                {
                    // ------------------------------
                    // Cleanup / persist data
                    // ------------------------------
                    Log.Save(); // save logs to file
                }
            }
        }
    }
}
