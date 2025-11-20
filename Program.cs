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
            top.Add(mainWin);

            // Run the app
            Application.Run();

            /*
            Console.WriteLine("Hello, World!");

            if (args.Length >= 3)
            {

                TorrentOperations torrentOps = new();



                Settings settings = new(port: ushort.Parse(args[2]));

            


                await torrentOps.AddTorrent(settings, args[0], args[1]);

            }
            */

        }


       

    }

}