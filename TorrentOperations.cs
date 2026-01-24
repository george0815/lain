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
using System.Text.Json;
using System.Text.Json.Serialization;
using Terminal.Gui;
using TextCopy;

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
            new(Settings.BuildEngineSettings().ToSettings());

        //list of all torrents
        internal static List<TorrentManager> Managers { get; } = [];

        //list of all torrentdata for serialization
        public static List<TorrentData> TorrentDataDTOList { get; set; } = [];

        // Add this static readonly field to cache the JsonSerializerOptions instance
        private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true, };

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
                    creator.Announces.Add([t]); // Add as a tier
                }
            }


            


            //Implement other metadata and settings
            creator.Private = data.IsPrivate;
            creator.Comment = data.Comment;
            creator.Publisher = data.Publisher; 
            creator.PieceLength = data.PieceSize > 0 ? data.PieceSize : creator.PieceLength;
            
           



            ITorrentFileSource files = new TorrentFileSource(data.TorPath);

            BEncodedDictionary dict = await creator.CreateAsync(files);
            

            File.WriteAllBytes(data.DownPath, dict.Encode());


            data.MaxConnections = Settings.Current.MaxConnections;
            data.MaxDownloadRate = Settings.Current.MaxDownloadSpeed;
            data.MaxUploadRate = Settings.Current.MaxUploadSpeed;
            data.UseDht = true;

            data.TorPath = data.DownPath;
            data.DownPath = "./";
            


            await AddTorrent(data, false, true);



        }


        // Add torrent
        internal static async Task AddTorrent(TorrentData data, bool loading, bool create)
        {
            // Build torrent settings
            var tSettings = new TorrentSettingsBuilder
            {
                MaximumConnections = data.MaxConnections,
                MaximumDownloadRate = data.MaxDownloadRate,
                MaximumUploadRate = data.MaxUploadRate,
                AllowDht = data.UseDht,
                AllowInitialSeeding = data.StartSeedingAfterCreation,

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
                if (!loading){ TorrentDataDTOList.Add(data); };
                SaveTorrentData();

                Log.Write(Resources.Downloadingfrommagnetlink);

                if (manager.Progress != 100.0 || !Settings.Current.StopSeedingWhenFinished)
                {
                    await manager.StartAsync();
                }

                StartProgressLoop();      
                AutosaveLoop();
            }
            else
            {
                // Fallback: load from .torrent file

                if (!Path.Exists(data.TorPath)) { return; }

                Torrent torrent = await Torrent.LoadAsync(data.TorPath);

                var manager = await Engine.AddAsync(torrent, data.DownPath, tSettings);

                WireUpManagerEvents(manager);
                Managers.Add(manager);
                if (!loading) { TorrentDataDTOList.Add(data); }         
                SaveTorrentData();


                
                Log.Write(create ?  Resources.Creating : Resources.Downloadingfromtorrentfile);


                if (manager.Progress != 100.0 || !Settings.Current.StopSeedingWhenFinished)
                {
                    await manager.StartAsync();
                }
                
                StartProgressLoop();
                AutosaveLoop();
            }
        }

        #endregion

        #region EVENTS

        //updates torrent list data
        public static event Action? UpdateProgress;


        // Events
        private static void WireUpManagerEvents(TorrentManager manager)
        {
            manager.TorrentStateChanged += async (o, e) =>
            {
                Log.Write($"{Resources.StatechangedeOldState__eNewState} {e.OldState} -> {e.NewState}"); 

                if ((e.OldState== TorrentState.Downloading) && e.NewState == TorrentState.Seeding &&
                    Settings.Current.StopSeedingWhenFinished)
                {
                    await manager.StopAsync();
                  
                }
            };

            Engine.Dht.StateChanged += async (o, e) =>
            {
                if (Settings.Current.DetailedLogging)
                {
                    Log.Write($"DHT: {Engine.Dht.State}");
                }
            };

            manager.PieceHashed += (o, e) =>
            {
                if (Settings.Current.DetailedLogging)
                {
                    Log.Write($"{Resources.PiecehashedePieceIndex_eHashPassed} {e.PieceIndex} - {e.HashPassed}"); 
             
                }
            };
        }
        //Autosave loop
        private static bool _autoLoopRunning = false;
        private static void AutosaveLoop()
        {
            if (_autoLoopRunning)
                return;

            _autoLoopRunning = true;

            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(TimeSpan.FromMinutes(1));
                    try
                    {
                        Log.Write(Resources.Savedfastresumedata);
                        await Engine.SaveStateAsync();

                    }
                    catch (Exception ex)
                    {
                        Log.Write($"{Resources.Errorsavingfastresumedataex} {ex}"); 
                    }
                    
                }
            });
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

                    UpdateProgress?.Invoke();
                    foreach (var m in Managers)
                    {
                        if (m?.Torrent == null) continue;

                        Log.Write(
                            $"[{m.Torrent.Name}] {m.Progress:0.00}% " +
                            $"DL: {m.Monitor.DownloadRate / 1024:0.0}kB/s " +
                            $"UL: {m.Monitor.UploadRate / 1024:0.0}kB/s"
                        );
                    }

                   
                    await Task.Delay(Settings.Current.RefreshInterval);
                }
            });
        }

        #endregion

        #region MANAGE TORRENTS

        internal async static void LoadAllTorrents()
        {
            try
            {
                if (!File.Exists("torrents.json")) {  return; }

                string json = File.ReadAllText("torrents.json");
                var loaded = JsonSerializer.Deserialize<List<TorrentData>>(json, JsonOptions);

                if (loaded != null) { TorrentDataDTOList = loaded; }

                //for each torrent add it to manager
                for (int i = 0; i < TorrentDataDTOList.Count; i++)
                {
                    await AddTorrent(TorrentDataDTOList[i], true, false);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{Resources.Errorloadingtorrents} {ex.Message}"); 
                Settings.Save();
            }
        }


        internal static void SaveTorrentData()
        {
            try
            {
                var json = JsonSerializer.Serialize(TorrentDataDTOList, JsonOptions);
                File.WriteAllText("torrents.json", json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{Resources.ErrorsavingtorrentsexMessage} {ex.Message}"); 
            }
        }

      

        internal static async Task SaveTorrentAsync(int index)
        {

            var fastResumeData = (await Managers[index].SaveFastResumeAsync()).Encode();

            var fastResumePath = Engine.Settings.GetFastResumePath(Managers[index].InfoHashes);
            var parentDirectory = Path.GetDirectoryName(fastResumePath)!;
            Directory.CreateDirectory(parentDirectory);
            File.WriteAllBytes(fastResumePath, fastResumeData);

            if (Settings.Current.DetailedLogging == true)
            {
                Log.Write($"{Resources.FastresumedatasavedfortorrentManagers_index_Torrent_Name}{Managers[index].Torrent?.Name}");
            }
        }

        internal static async Task LoadTorrentAsync(int index)
        {
          
            var fastResumePath = Engine.Settings.GetFastResumePath(Managers[index].InfoHashes);

            if (File.Exists(fastResumePath) &&
                FastResume.TryLoad(fastResumePath, out FastResume? fastResume) &&
                Managers[index].InfoHashes.Contains(fastResume.InfoHashes.V1OrV2))
            {
                await Managers[index].LoadFastResumeAsync(fastResume);
            }

        }

        internal static async Task PauseTorrentAsync(int index)
        {
            if (index < 0 || index >= Managers.Count) return;
            var manager = Managers[index];
            if (manager != null && (manager.State == TorrentState.Downloading || manager.State == TorrentState.Seeding))
            {
                try
                {

                    await manager.PauseAsync();
                    Log.Write($"{Resources.PausedtorrentmanagerTorrent_Name} {manager.Torrent?.Name}"); 
                    await SaveTorrentAsync(index);
                   
                }
                catch (Exception ex)
                {
                    Log.Write($"{Resources.ErrorpausingtorrentexMessage} {ex.Message}"); 
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
                    Log.Write($"{Resources.ResumedtorrentmanagerTorrent_Name} {manager.Torrent?.Name}"); 

                }
                catch (Exception ex)
                {
                    Log.Write($"{Resources.ErrorresumingtorrentexMessage} {ex.Message}"); 
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
                    Log.Write($"{Resources.StartedseedingmanagerTorrent_Name} {manager.Torrent?.Name}"); 


                }
                catch (Exception ex)
                {
                    Log.Write($"{Resources.ErrorstartingseedingexMessage} {ex.Message}"); 
                }
            }
        }

        internal static async Task StopSeedingAsync(int index)
        {
            if (index < 0 || index >= Managers.Count) return;
            var manager = Managers[index];
            if (manager != null && manager.State == TorrentState.Seeding && manager.State != TorrentState.Stopping && manager.State != TorrentState.Stopped)
            {
                try
                {
                    await manager.StopAsync();
                    Log.Write($"{Resources.StoppedseedingmanagerTorrent_Name} {manager.Torrent?.Name}"); 

                }
                catch (Exception ex)
                {
                    Log.Write($"{Resources.ErrorstoppingseedingexMessage} {ex.Message}"); 
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
                    if (manager.State != TorrentState.Stopped && manager.State != TorrentState.Stopping)
                        await manager.StopAsync();

                    // Remove from engine
                    await Engine.RemoveAsync(manager);

                    // Remove from list
                    Managers.RemoveAt(index);
                    TorrentDataDTOList.RemoveAt(index);
                    SaveTorrentData();

                    Log.Write($"{Resources.DeletedtorrentmanagerTorrent_Name}{manager.Torrent?.Name}"); 

                    // Optionally delete downloaded files
                    if (deleteFiles && Directory.Exists(manager.SavePath))
                    {
                        Directory.Delete(manager.SavePath, true);
                        Log.Write($"{Resources.DeletedfilesatmanagerSavePath}{manager.SavePath}"); 
                    }
                }
                catch (Exception ex)
                {
                    Log.Write($"{Resources.ErrordeletingtorrentexMessage} {ex.Message}"); 
                }
            }
        }

        #endregion

    }
}