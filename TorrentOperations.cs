using MonoTorrent;
using MonoTorrent.BEncoding;
using MonoTorrent.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace lain
{
    internal class TorrentOperations
    {

         static ClientEngine engine = new ClientEngine(Settings.EngineSettings.ToSettings());

        // Event fired whenever progress updates
        public static event Action? UpdateProgress;




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

        internal static async Task CreateTorrent(string folderPath, string outputPath, string? trackerUrl = null, string? magnetLink = null)
        {


            TorrentCreator creator = new TorrentCreator();
            creator.Announces.Add(new List<string> { trackerUrl! });
            ITorrentFileSource files = new TorrentFileSource("C:\\Users\\Hunter\\Documents\\GitHub\\lain\\bin\\Debug\\net10.0\\tmpout");

            int x = 5;

            BEncodedDictionary dict = await creator.CreateAsync(files);
            Torrent torrent = Torrent.Load(dict);


            //BitField bitfield = new BitField(torrent.Pieces.Count).Not();
           // FastResume fastResumeData = new FastResume(torrent.InfoHash, bitfield);

            //manager.LoadFastResume(fastResumeData);

            var manager = await engine.AddAsync(torrent, outputPath);


            manager.TorrentStateChanged += (o, e) =>
            {
                Log.Write($"State changed: {e.OldState} -> {e.NewState}");
            };

            manager.PieceHashed += (o, e) =>
            {
                //Log.Write($"Piece hashed: {e.PieceIndex} - {e.HashPassed}");
            };

            Log.Write("Creating... Press any key to exit.");


            managers!.Add(manager);

            await manager.StartAsync();

            //MagnetLink magnet = new MagnetLink(manager.Torrent.InfoHash, manager.Torrent.Name, new[] { "http://192.168.5.151:8000/announce", "udp://192.168.5.151:8000" });

            /*

            var creator = new TorrentCreator();
            TorrentFileSource src = new TorrentFileSource(folderPath);
      

            if (!string.IsNullOrWhiteSpace(trackerUrl))
            {
                creator.Announces.Add(new List<string> { trackerUrl });
            }



            // Create the .torrent
            await creator.CreateAsync(src, outputPath);

            

            Log.Write("Torrent created at: " + outputPath);

            // Automatically seed via your AddTorrent() method
            await AddTorrent(Settings.DefaultDownloadPath!, outputPath);

            */
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
                    // Fire event to notify UI
                    UpdateProgress?.Invoke();

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