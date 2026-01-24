using lain.helpers;
using MonoTorrent.Client;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Terminal.Gui;

namespace lain
{

    /// <summary>
    /// Holds all configurable settings and UI variables for the application.
    /// </summary>
    internal class SettingsData
    {
        // ------------------------------
        // UI Variables
        // ------------------------------
        public static ushort HeaderHeight { get; set; } = 15;  // Default header height
        public static ushort LogoWidth { get; set; } = 55;     // Width of ASCII logo display

        // ------------------------------
        // Network Ports
        // ------------------------------
        public ushort Port { get; set; } = 55123;             // Main listening port
        public ushort DhtPort { get; set; } = 55124;          // DHT listening port

        // ------------------------------
        // Maximum connections / transfer rates
        // ------------------------------
        public ushort MaxConnections { get; set; } = 100;
        public int MaxDownloadSpeed { get; set; } = 100 /* to KB*/ * 1024 /* to MB*/ * 1024;
        public int MaxUploadSpeed { get; set; } = 100 /* to KB*/ * 1024 /* to MB*/ * 1024;
        public int RefreshInterval { get; set; } = 1000;      // UI refresh interval (ms)

        // ------------------------------
        // Client behavior flags
        // ------------------------------
        public bool DetailedLogging { get; set; } = true;
        public bool StopSeedingWhenFinished { get; set; } = true;
        public bool EnablePortForwarding { get; set; } = true;
        public bool HidetextCursor { get; set; } = true;
        public bool DisableColoredHotkeyInfo { get; set; } = false;
        public bool DisableASCII { get; set; } = false;

        // ------------------------------
        // Paths
        // ------------------------------
        public string? DefaultDownloadPath { get; set; } = "./";  // Default download folder
        public string? LogPath { get; set; } = "./log.txt";       // Log file path
        public string SettingsPath { get; set; } = "cfg.json";    // Settings file

        // ------------------------------
        // UI Colors
        // ------------------------------
        public Terminal.Gui.Color BackgroundColor { get; set; } = Terminal.Gui.Color.Black;
        public Terminal.Gui.Color TextColor { get; set; } = Terminal.Gui.Color.White;
        public Terminal.Gui.Color FocusBackgroundColor { get; set; } = Terminal.Gui.Color.White;
        public Terminal.Gui.Color FocusTextColor { get; set; } = Terminal.Gui.Color.Black;
        public Terminal.Gui.Color HotTextColor { get; set; } = Terminal.Gui.Color.BrightYellow;
        public Terminal.Gui.Color LogoColor { get; set; } = Terminal.Gui.Color.White;

        // ------------------------------
        // Hotkey controls
        // ------------------------------
        public TorrentHotkeys Controls { get; set; } = new TorrentHotkeys
        {
            StartDownload = Key.F3,
            StopDownload = Key.F4,
            StartSeeding = Key.F5,
            StopSeeding = Key.F6,
            RemoveTorrent = Key.F7,
            GenMagLink = Key.F8
        };

        // ------------------------------
        // ASCII / Icons
        // ------------------------------
        public List<string> Icons { get; set; } = ASCII.icons;

        // ------------------------------
        // Search settings
        // ------------------------------
        public int SearchResultsLimit { get; set; } = 20;              // Max results overall
        public int Timeout { get; set; } = 20 * 1_000;                 // Timeout in milliseconds
        public int SearchResultsLimitPerSource { get; set; } = 20;    // Max results per source

        // ------------------------------
        // Default Sources / Categories
        // ------------------------------
        public String[] DefaultSources { get; set; } = [
            "kickasstorrents",
            "thepiratebay",
            "limetorrents",
            "yts",
            "x1337",
            "torrentgalaxy"
        ];

        public String[] SearchSources { get; set; } = [
            "thepiratebay",
            "limetorrents",
            "kickasstorrents"
        ];

        public String[] Categories { get; set; } = [
            Resources.Movies,
            Resources.Games,
            Resources.TVshows,
            Resources.Applications,
            Resources.Other
        ];

        public string SortBy { get; set; } = Resources.Source;

        public bool UseQbittorrentPlugins { get; set; } = true;
    }


    /// <summary>
    /// Provides global access to the current settings instance, 
    /// and handles saving/loading from JSON.
    /// </summary>
    internal static class Settings
    {
        /// <summary>
        /// The currently loaded settings.
        /// </summary>
        internal static SettingsData Current { get; private set; } = new();

        /// <summary>
        /// Options for JSON serialization.
        /// </summary>
        private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

        /// <summary>
        /// Converts current settings into MonoTorrent EngineSettingsBuilder.
        /// </summary>
        internal static EngineSettingsBuilder BuildEngineSettings()
        {
            var s = Current;

            return new EngineSettingsBuilder
            {
                AllowPortForwarding = s.EnablePortForwarding,

                ListenEndPoints = new()
                {
                    { "main", new IPEndPoint(IPAddress.Any, s.Port) }
                },

                DhtEndPoint = new IPEndPoint(IPAddress.Any, s.DhtPort),

                MaximumConnections = s.MaxConnections,
                MaximumDownloadRate = s.MaxDownloadSpeed * 1024,
                MaximumUploadRate = s.MaxUploadSpeed * 1024,
            };
        }

        #region SAVE/LOAD

        /// <summary>
        /// Saves current settings to disk in JSON format.
        /// </summary>
        internal static void Save()
        {
            try
            {
                var json = JsonSerializer.Serialize(Current, JsonOptions);
                File.WriteAllText(Current.SettingsPath ?? "cfg.json", json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{Resources.ErrorsavingsettingsexMessage} {ex.Message}");
            }
        }

        /// <summary>
        /// Loads settings from disk or creates default if missing or corrupted.
        /// </summary>
        internal static void Load()
        {
            try
            {
                if (!File.Exists(Current.SettingsPath))
                {
                    Settings.Save();
                    return;
                }

                string json = File.ReadAllText(Current.SettingsPath);
                var loaded = JsonSerializer.Deserialize<SettingsData>(json, JsonOptions);

                if (loaded != null)
                {
                    Current = loaded;

                    // Reduce header height if ASCII art disabled
                    if (Current.DisableASCII)
                        SettingsData.HeaderHeight = 5;
                }
                else
                {
                    Settings.Save();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{Resources.ErrorloadingsettingsexMessage} {ex.Message}");
                Settings.Save();
            }
        }

        #endregion
    }
}
