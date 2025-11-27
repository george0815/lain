using lain.helpers;
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
        public bool StartSeedingAfterCreation { get; set; }
        public bool IsPrivate { get; set; }

        public List<string> Trackers { get; set; }
        public string MagnetUrl { get; set; }
        public string TorPath { get; set; }
        public string DownPath { get; set; }
        public string Name { get; set; }
        public string Publisher { get; set; }
        public string Comment { get; set; }
        public int MaxConnections { get; set; }
        public int PieceSize { get; set; }
        public int MaxDownloadRate { get; set; }
        public int MaxUploadRate { get; set; }

    }

    //CRUD operations
    internal class TorrentOperations
    {
        #region DECLARATIONS

        // Shared engine instance (rebuilt on startup using settings)
        private static readonly ClientEngine Engine =
            new ClientEngine(Settings.BuildEngineSettings().ToSettings());

        //list of all torrents
        internal static List<TorrentManager> Managers { get; } = new();

        internal TorrentOperations() { }

        #endregion

        #region ADD/CREATE

        // Create torrent
        internal static async Task CreateTorrent(TorrentData data)
        {
            var creator = new TorrentCreator();

            foreach (string t in data.Trackers)
            {
                if (!string.IsNullOrWhiteSpace(t))
                {
                    creator.Announces.Add(new List<string> { t }); // Add as a tier
                }
            }


            // Build torrent settings
            var tSettings = new TorrentSettingsBuilder
            {
                AllowInitialSeeding = data.StartSeedingAfterCreation,
            }.ToSettings();


            //Implement other metadata and settings
            creator.Private = data.IsPrivate;
            creator.Comment = data.Comment;
            creator.Publisher = data.Publisher; 
            creator.PieceLength = data.PieceSize > 0 ? data.PieceSize : creator.PieceLength;
            creator.CreatedBy = "Lain Torrent Client";
           



            ITorrentFileSource files = new TorrentFileSource(data.TorPath);

            BEncodedDictionary dict = await creator.CreateAsync(files);
            Torrent torrent = Torrent.Load(dict);

            
            var manager = await Engine.AddAsync(torrent, data.DownPath, tSettings);

            

            WireUpManagerEvents(manager);
            Managers.Add(manager);

            Log.Write("Creating...");
            await manager.StartAsync();

            if (data.UseMagnetLink)
            {
                var magnet = new MagnetLink(torrent.InfoHashes, torrent.Name,
                    torrent.AnnounceUrls.SelectMany(t => t.ToArray()).ToList());

                Log.Write($"Magnet link generated: {magnet}");

                MagnetLinkGenerated?.Invoke(magnet.ToString()!);
            }




        }


        // Add torrent
        internal static async Task AddTorrent(TorrentData data)
        {
            // Build torrent settings
            var tSettings = new TorrentSettingsBuilder
            {
                MaximumConnections = data.MaxConnections,
                MaximumDownloadRate = data.MaxDownloadRate,
                MaximumUploadRate = data.MaxUploadRate,
                AllowDht = data.UseDht
            }.ToSettings();

            // Check if we have a magnet link
            if (!string.IsNullOrWhiteSpace(data.MagnetUrl))
            {
                // Create a MagnetLink object from the URL
                MagnetLink magnet = MagnetLink.Parse(data.MagnetUrl);

                // Add the torrent from magnet link
                var manager = await Engine.AddAsync(magnet, data.DownPath, tSettings);

                WireUpManagerEvents(manager);
                Managers.Add(manager);

                Log.Write("Downloading from magnet link...");
                await manager.StartAsync();

                StartProgressLoop();
            }
            else
            {
                // Fallback: load from .torrent file
                Torrent torrent = await Torrent.LoadAsync(data.TorPath);

                var manager = await Engine.AddAsync(torrent, data.DownPath, tSettings);

                WireUpManagerEvents(manager);
                Managers.Add(manager);

                Log.Write("Downloading from torrent file...");
                await manager.StartAsync();

                StartProgressLoop();
            }
        }

        #endregion

        #region EVENTS

        //updates torrent list data
        public static event Action? UpdateProgress;

        //updates
        public static event Action<string>? MagnetLinkGenerated;

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


        internal static async Task PauseTorrentAsync(int index)
        {
            if (index < 0 || index >= Managers.Count) return;
            var manager = Managers[index];
            if (manager != null && (manager.State == TorrentState.Downloading || manager.State == TorrentState.Seeding))
            {
                try
                {
                    await manager.PauseAsync();
                    Log.Write($"Paused torrent: {manager.Torrent?.Name}");
                }
                catch (Exception ex)
                {
                    Log.Write($"Error pausing torrent: {ex.Message}");
                }
            }
        }

        internal static async Task ResumeTorrentAsync(int index)
        {
            if (index < 0 || index >= Managers.Count) return;
            var manager = Managers[index];
            if (manager != null && manager.State == TorrentState.Paused)
            {
                try
                {
                    await manager.StartAsync();
                    Log.Write($"Resumed torrent: {manager.Torrent?.Name}");
                }
                catch (Exception ex)
                {
                    Log.Write($"Error resuming torrent: {ex.Message}");
                }
            }
        }

        internal static async Task StartSeedingAsync(int index)
        {
            if (index < 0 || index >= Managers.Count) return;
            var manager = Managers[index];
            if (manager != null && manager.State != TorrentState.Seeding)
            {
                try
                {
                    await manager.StartAsync();
                    Log.Write($"Started seeding: {manager.Torrent?.Name}");
                }
                catch (Exception ex)
                {
                    Log.Write($"Error starting seeding: {ex.Message}");
                }
            }
        }

        internal static async Task StopSeedingAsync(int index)
        {
            if (index < 0 || index >= Managers.Count) return;
            var manager = Managers[index];
            if (manager != null && manager.State == TorrentState.Seeding)
            {
                try
                {
                    await manager.StopAsync();
                    Log.Write($"Stopped seeding: {manager.Torrent?.Name}");
                   
                }
                catch (Exception ex)
                {
                    Log.Write($"Error stopping seeding: {ex.Message}");
                }
            }
        }

        internal static async Task DeleteTorrentAsync(int index, bool deleteFiles = true)
        {
            if (index < 0 || index >= Managers.Count) return;
            var manager = Managers[index];

            if (manager != null)
            {
                try
                {
                    // Stop the torrent if it’s active
                    if (manager.State != TorrentState.Stopped)
                        await manager.StopAsync();

                    // Remove from engine
                    await Engine.RemoveAsync(manager);

                    // Remove from list
                    Managers.RemoveAt(index);

                    Log.Write($"Deleted torrent: {manager.Torrent?.Name}");

                    // Optionally delete downloaded files
                    if (deleteFiles && Directory.Exists(manager.SavePath))
                    {
                        Directory.Delete(manager.SavePath, true);
                        Log.Write($"Deleted files at: {manager.SavePath}");
                    }
                }
                catch (Exception ex)
                {
                    Log.Write($"Error deleting torrent: {ex.Message}");
                }
            }
        }

    }
}