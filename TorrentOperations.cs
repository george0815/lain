using MonoTorrent;
using MonoTorrent.BEncoding;
using MonoTorrent.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace lain
{

    //used for passing in settings from UI forms
    internal struct TorrentData
    {
        public bool UseMagnetLink { get; set; }
        public bool UseDht { get; set; }
        public string MagnetUrl { get; set; }
        public string TorPath { get; set; }
        public string DownPath { get; set; }
        public int MaxConnections { get; set; }
        public int MaxDownloadRate { get; set; }
        public int MaxUploadRate { get; set; }
    }

    //CRUD operations
    internal class TorrentOperations
    {
        // Shared engine instance (rebuilt on startup using settings)
        private static readonly ClientEngine Engine =
            new ClientEngine(Settings.BuildEngineSettings().ToSettings());

        //updates torrent list data
        public static event Action? UpdateProgress;

        //list of all torrents
        internal static List<TorrentManager> Managers { get; } = new();


        internal TorrentOperations() { }

        #region ADD/CREATE

        // Create torrent
        internal static async Task CreateTorrent(
            string folderPath,
            string outputPath,
            string? trackerUrl = null,
            string? magnetLink = null)
        {
            var creator = new TorrentCreator();

            if (!string.IsNullOrWhiteSpace(trackerUrl))
                creator.Announces.Add(new List<string> { trackerUrl });

            ITorrentFileSource files = new TorrentFileSource(folderPath);

            BEncodedDictionary dict = await creator.CreateAsync(files);
            Torrent torrent = Torrent.Load(dict);

            var manager = await Engine.AddAsync(torrent, outputPath);

            WireUpManagerEvents(manager);
            Managers.Add(manager);

            Log.Write("Creating...");
            await manager.StartAsync();
        }


        // Add torrent
        internal static async Task AddTorrent(TorrentData data)
        {
            var tSettings = new TorrentSettingsBuilder
            {
                MaximumConnections = data.MaxConnections,
                MaximumDownloadRate = data.MaxDownloadRate,
                MaximumUploadRate = data.MaxUploadRate,
                AllowDht = data.UseDht
            }.ToSettings();

            Torrent torrent = await Torrent.LoadAsync(data.TorPath);

            var manager = await Engine.AddAsync(torrent, data.DownPath, tSettings);

            WireUpManagerEvents(manager);
            Managers.Add(manager);

            Log.Write("Downloading...");
            await manager.StartAsync();

            StartProgressLoop();
        }

        #endregion

        #region EVENTS

        // Events
        private static void WireUpManagerEvents(TorrentManager manager)
        {
            manager.TorrentStateChanged += async (o, e) =>
            {
                Log.Write($"State changed: {e.OldState} -> {e.NewState}");

                if (e.NewState == TorrentState.Seeding &&
                    Settings.Current.StopSeedingWhenFinished)
                {
                    await manager.StopAsync();
                }
            };

            manager.PieceHashed += (o, e) =>
            {
                if (Settings.Current.DetailedLogging)
                {
                    Log.Write($"Piece hashed: {e.PieceIndex} - {e.HashPassed}");
                }
            };
        }

        // Progress loop
        private static bool _progressLoopRunning = false;

        private static void StartProgressLoop()
        {
            if (_progressLoopRunning)
                return;

            _progressLoopRunning = true;

            Task.Run(async () =>
            {
                while (true)
                {
                    foreach (var m in Managers)
                    {
                        if (m?.Torrent == null) continue;

                        Log.Write(
                            $"[{m.Torrent.Name}] {m.Progress:0.00}% " +
                            $"DL: {m.Monitor.DownloadRate / 1024:0.0}kB/s " +
                            $"UL: {m.Monitor.UploadRate / 1024:0.0}kB/s"
                        );
                    }

                    UpdateProgress?.Invoke();
                    await Task.Delay(5000);
                }
            });
        }

        #endregion


        // TODO: Remove/Pause/Resume logic here
        internal void PauseTorrent(short index)
        {
            // TODO
        }

        internal void ResumeTorrent(short index)
        {
            // TODO
        }

        internal void DeleteTorrent(short index)
        {
            // TODO
        }
    }
}
