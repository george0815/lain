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
        public static void Main(string[] args)
        {

            //load settings


            //welcome message


            //load tui







            Console.WriteLine("Hello, World!");

            if (args.Length >= 3)
            {
                TestMonoTorrent(args[0], args[1], int.Parse(args[2])).GetAwaiter().GetResult();

            }

        }


        static async Task TestMonoTorrent(string downPath, string torPath, int port)
        {


            var engineSettings = new EngineSettingsBuilder
            {
                AllowPortForwarding = true,
                ListenEndPoints = new Dictionary<string, IPEndPoint> { { "main", new IPEndPoint(IPAddress.Any, port)} },
                DhtEndPoint = new IPEndPoint(IPAddress.Any, 55124),
            };

            var engine = new ClientEngine(engineSettings.ToSettings());

            Torrent torrent = await Torrent.LoadAsync(torPath);


            var manager = await engine.AddAsync(torrent, downPath);


            manager.TorrentStateChanged += (o, e) =>
            {
                Console.WriteLine($"State changed: {e.OldState} -> {e.NewState}");
            };

            manager.PieceHashed += (o, e) =>
            {
                Console.WriteLine($"Piece hashed: {e.PieceIndex} - {e.HashPassed}");
            };

            Console.WriteLine("Downloading... Press any key to exit.");

            manager.StartAsync().GetAwaiter().GetResult();

            while (manager.State != TorrentState.Stopped)
            {
                Console.WriteLine(

                    $"Progress: {manager.Progress:0.00}% - " +
                    $"Download Speed: {manager.Monitor.DownloadRate / 1024:0.00} kB/s - " +
                    $"Upload Speed: {manager.Monitor.UploadRate / 1024:0.00} kB/s - "

                    );

                await Task.Delay(1000);

            }


        }

    }

}