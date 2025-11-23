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
    internal struct TorrentData
    {
        public bool UseMagnetLink { get; set; } //
        public bool UseDht { get; set; }
        public string MagnetUrl { get; set; } //
        public string TorPath { get; set; }
        public string DownPath { get; set; }
        public int MaxConnections { get; set; }
        public int MaxDownloadRate { get; set; }
        public int MaxUploadRate { get; set; }
        public int MaxSeeders { get; set; } //
        public int MaxLeechers { get; set; } //
    }

    internal class TorrentOperations
    {
        

        static ClientEngine engine = new ClientEngine(Settings.EngineSettings!.ToSettings());

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
            ITorrentFileSource files = new TorrentFileSource(folderPath);

           

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
                if (Settings.DetailedLogging) { Log.Write($"Piece hashed: {e.PieceIndex} - {e.HashPassed}"); }
            };

            Log.Write("Creating...");


            managers!.Add(manager);

            await manager.StartAsync();

            //MagnetLink magnet = new MagnetLink(manager.Torrent.InfoHash, manager.Torrent.Name, new[] { "http://192.168.5.151:8000/announce", "udp://192.168.5.151:8000" });

            
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



        internal static async Task AddTorrent(TorrentData settings)
        {


            var tSettings = new TorrentSettingsBuilder
            {
                MaximumConnections = settings.MaxConnections,
                MaximumDownloadRate = settings.MaxDownloadRate,
                MaximumUploadRate = settings.MaxUploadRate,
                AllowDht = settings.UseDht,
            }.ToSettings();

            Torrent torrent = await Torrent.LoadAsync(settings.TorPath);


            var manager = await engine.AddAsync(torrent, settings.DownPath, tSettings);

            

            manager.TorrentStateChanged += (o, e) =>
            {
                Log.Write($"State changed: {e.OldState} -> {e.NewState}");

                if (e.NewState == TorrentState.Seeding && Settings.StopSeedingWhenFinished)
                {
                    manager.StopAsync();
                }

            };

            manager.PieceHashed += (o, e) =>
            {
                if (Settings.DetailedLogging) { Log.Write($"Piece hashed: {e.PieceIndex} - {e.HashPassed}"); }
            };

           

            Log.Write("Downloading...");


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

                    await Task.Delay(5000);
                }
            });

   


        }




    }
}