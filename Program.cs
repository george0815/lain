using System;
using Terminal.Gui;
using System.Threading.Tasks;
using MonoTorrent.Client;
using MonoTorrent;
using System.Net.Sockets;
using System.Net;





namespace lain
{

    class Program
    {

        

        public static async Task Main(string[] args)
        {

            //load settings
            Settings.LoadSettings();


            //welcome message


            //load tui
            Application.Init();

            // Create a top-level container
            var top = Application.Top;

            // Add your window to the top-level
            var mainWin = new LainUI();

            ColorScheme myScheme = new ColorScheme()
            {
                Normal = Application.Driver.MakeAttribute(Settings.TextColor, Settings.BackgroundColor), // text, background
                Focus = Application.Driver.MakeAttribute(Settings.FocusTextColor, Settings.FocusBackgroundColor), // focused element
                HotNormal = Application.Driver.MakeAttribute(Settings.HotTextColor, Settings.BackgroundColor),
                HotFocus = Application.Driver.MakeAttribute(Settings.FocusTextColor, Settings.FocusBackgroundColor),
            };

            mainWin.ColorScheme = myScheme;
            top.Add(mainWin);

            // Run the app
            Application.Run();

            
           





            if (args.Length > 0) {
               // await TorrentOperations.AddTorrent(args[0], args[1]);

            }
            else
            {
                // await torrentOperations.AddTorrent("test.torrent", "tmpdir");

                //TorrentOperations.AddTorrent("tmpdir", "test.torrent").GetAwaiter().GetResult();


            }



        }





    }

}