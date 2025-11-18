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
            


            //welcome message


            //load tui

            Console.WriteLine("Hello, World!");

            if (args.Length >= 3)
            {

                TorrentOperations torrentOps = new();



                Settings settings = new(port: ushort.Parse(args[2]));

            


                await torrentOps.TestMonoTorrent(settings, args[0], args[1]);

            }

        }


       

    }

}