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
                HotNormal = Application.Driver.MakeAttribute(Settings.HotTextColor, Settings.BackgroundColor), //hotkey text, background
                HotFocus = Application.Driver.MakeAttribute(Settings.FocusTextColor, Settings.FocusBackgroundColor), // focused hotkey text, background
            };

            mainWin.ColorScheme = myScheme;
            top.Add(mainWin);

            // Run the app
            Application.Run();

              



        }





    }

}