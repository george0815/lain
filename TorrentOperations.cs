using MonoTorrent;
using MonoTorrent.Client;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace lain
{
    internal class TorrentOperations
    {

         static ClientEngine engine = new ClientEngine(Settings.EngineSettings.ToSettings());






        static internal List<TorrentManager>? managers { get; set; } = [];


        internal TorrentOperations()
        {


            engine = new ClientEngine(Settings.EngineSettings!.ToSettings());

        }


        internal void DeleteTorrent(short index)
        {

            //pause torrent 

            //stop seeding 

            //remove from list 




        }

        internal void CreateTorrent(string? trackerUrl, List<FileInfo> files, string? magnetlink, short port)
        {

            //create torrent 


            //start seeding 



        }




        internal void PauseTorrent(short index)
        {

            //stop leeching       

            //write progress to disk


        }

        internal void ResumeTorrent(short index)
        {

            //load progress from disk 

            //start seeding 

        }



        internal static async Task AddTorrent(string downPath, string torPath)
        {



            Torrent torrent = await Torrent.LoadAsync(torPath);


            var manager = await engine.AddAsync(torrent, downPath);


            manager.TorrentStateChanged += (o, e) =>
            {
                Log.Write($"State changed: {e.OldState} -> {e.NewState}");
            };

            manager.PieceHashed += (o, e) =>
            {
                //Log.Write($"Piece hashed: {e.PieceIndex} - {e.HashPassed}");
            };

            Log.Write("Downloading... Press any key to exit.");


            managers!.Add(manager);

            await manager.StartAsync();

            await Task.Run(async () =>
            {
                while (true)
                {
                    foreach (var m in managers)
                    {
                        Log.Write(
                            $"[{m.Torrent!.Name}] {m.Progress:0.00}% " +
                            $"DL: {m.Monitor.DownloadRate / 1024:0.0}kB/s " +
                            $"UL: {m.Monitor.UploadRate / 1024:0.0}kB/s"
                        );
                    }

                    await Task.Delay(1000);
                }
            });

            /*
            while (manager.State != TorrentState.Stopped)
            {
                Console.WriteLine(

                    $"Progress: {manager.Progress:0.00}% - " +
                    $"Download Speed: {manager.Monitor.DownloadRate / 1024:0.00} kB/s - " +
                    $"Upload Speed: {manager.Monitor.UploadRate / 1024:0.00} kB/s - "

                    );

                Log.Write(

                    $"Progress: {manager.Progress:0.00}% - " +
                    $"Download Speed: {manager.Monitor.DownloadRate / 1024:0.00} kB/s - " +
                    $"Upload Speed: {manager.Monitor.UploadRate / 1024:0.00} kB/s - "

                    );

                await Task.Delay(1000);

            }
            */


        }

    }
}