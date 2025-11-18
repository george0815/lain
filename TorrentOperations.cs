using MonoTorrent;
using MonoTorrent.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace lain
{
    internal class TorrentOperations
    {

        internal TorrentOperations()
        {
        }

        internal async Task TestMonoTorrent(Settings settings, string downPath, string torPath)
        {

            

            var engine = new ClientEngine(settings.EngineSettings!.ToSettings());

           

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
