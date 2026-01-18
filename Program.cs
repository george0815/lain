using lain.helpers;
using MonoTorrent;
using MonoTorrent.Client;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Terminal.Gui;






namespace lain
{

    class Program
    {


        public static async Task Main(string[] args)
        {

            // Change to Japanese (Japan) (for debugging)
            CultureInfo ci = new CultureInfo("ja-JP");

            //Thread.CurrentThread.CurrentUICulture = ci;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            Console.OutputEncoding = Encoding.GetEncoding(932);



            //load settings
            Settings.Load();
            Ghidorah.LoadQbittorrentPlugins();
            if (Ghidorah.QbSources == null || Ghidorah.QbSources.Length == 0)
            {
                Settings.Current.UseQbittorrentPlugins = false;
            }


            //load tui
            Application.Init();

            // Create a top-level container
            var top = Application.Top;

            // Add your window to the top-level
            var mainWin = new LainUI();

            ColorScheme myScheme = new ColorScheme()
            {
                Normal = Application.Driver.MakeAttribute(Settings.Current.TextColor, Settings.Current.BackgroundColor), // text, background
                Focus = Application.Driver.MakeAttribute(Settings.Current.FocusTextColor, Settings.Current.FocusBackgroundColor), // focused element
                HotNormal = Application.Driver.MakeAttribute(Settings.Current.HotTextColor, Settings.Current.BackgroundColor), //hotkey text, background
                HotFocus = Application.Driver.MakeAttribute(Settings.Current.FocusTextColor, Settings.Current.FocusBackgroundColor), // focused hotkey text, background
            };

            mainWin.ColorScheme = myScheme;
            top.Add(mainWin);

            // Run the app
            try
            {
                Application.Run();
            }
            finally
            {
                // Cleanup code here
                Log.Save();
            }



        }


    }

}