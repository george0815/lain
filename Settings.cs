using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json;
using MonoTorrent.Client;

namespace lain
{
    internal class Settings
    {
        #region Properties

        internal static ushort Port { get; set; } = 55123;
        internal static ushort DhtPort { get; set; } = 55124;
        internal static ushort MaxSeedersPerTorrent { get; set; } = 100;
        internal static ushort MaxConnections { get; set; } = 100;
        internal static ushort MaxLeechersPerTorrent { get; set; } = 200;
        internal static int MaxDownloadSpeed { get; set; } = 1000;
        internal static int MaxUploadSpeed { get; set; } = 1000;
        internal static bool EnableDht { get; set; } = true;
        internal static bool StopSeedingWhenFinished { get; set; } = true;
        internal static bool EnablePortForwarding { get; set; } = true;
        internal static string? DefaultDownloadPath { get; set; } = "";
        internal static string? LogPath { get; set; } = "";
        internal static string? SettingsPath { get; set; } = "settings.json";

        internal static EngineSettingsBuilder? EngineSettings { get; set; } = new EngineSettingsBuilder
        {
            AllowPortForwarding = EnablePortForwarding,
            ListenEndPoints = new Dictionary<string, IPEndPoint> { { "main", new IPEndPoint(System.Net.IPAddress.Any, Port) } },
            DhtEndPoint = new IPEndPoint(System.Net.IPAddress.Any, DhtPort)
        };

        #endregion

        /// <summary>
        /// Save all static properties to JSON.
        /// </summary>
        internal static void SaveSettings()
        {
            try
            {
                var jsonOptions = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                // Serialize only the user-configurable properties
                var settingsData = new
                {
                    Port,
                    DhtPort,
                    MaxSeedersPerTorrent,
                    MaxConnections,
                    MaxLeechersPerTorrent,
                    MaxDownloadSpeed,
                    MaxUploadSpeed,
                    EnableDht,
                    StopSeedingWhenFinished,
                    EnablePortForwarding,
                    DefaultDownloadPath,
                    LogPath,
                    SettingsPath
                };

                string json = JsonSerializer.Serialize(settingsData, jsonOptions);
                File.WriteAllText(SettingsPath ?? "settings.json", json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving settings: {ex.Message}");
            }
        }

        /// <summary>
        /// Load all static properties from JSON.
        /// </summary>
        internal static void LoadSettings()
        {
            try
            {
                if (!File.Exists(SettingsPath ?? "cfg.json"))
                    return;

                string json = File.ReadAllText(SettingsPath ?? "settings.json");

                // Deserialize into a temporary object
                var settingsData = JsonSerializer.Deserialize<SettingsDTO>(json);
                if (settingsData == null) return;

                Port = settingsData.Port;
                DhtPort = settingsData.DhtPort;
                MaxSeedersPerTorrent = settingsData.MaxSeedersPerTorrent;
                MaxConnections = settingsData.MaxConnections;
                MaxLeechersPerTorrent = settingsData.MaxLeechersPerTorrent;
                MaxDownloadSpeed = settingsData.MaxDownloadSpeed;
                MaxUploadSpeed = settingsData.MaxUploadSpeed;
                EnableDht = settingsData.EnableDht;
                StopSeedingWhenFinished = settingsData.StopSeedingWhenFinished;
                EnablePortForwarding = settingsData.EnablePortForwarding;
                DefaultDownloadPath = settingsData.DefaultDownloadPath;
                LogPath = settingsData.LogPath;
                SettingsPath = settingsData.SettingsPath;

                // Rebuild EngineSettings
                EngineSettings = new EngineSettingsBuilder
                {
                    AllowPortForwarding = EnablePortForwarding,
                    ListenEndPoints = new Dictionary<string, IPEndPoint> { { "main", new IPEndPoint(IPAddress.Any, Port) } },
                    DhtEndPoint = new IPEndPoint(IPAddress.Any, DhtPort)
                };
            }
            catch (Exception ex)
            {
                SaveSettings();
                Console.WriteLine($"Error loading settings: {ex.Message}");
            }
        }

        // DTO class used only for JSON serialization
        private class SettingsDTO
        {
            public ushort Port { get; set; }
            public ushort DhtPort { get; set; }
            public ushort MaxSeedersPerTorrent { get; set; }
            public ushort MaxConnections { get; set; }
            public ushort MaxLeechersPerTorrent { get; set; }
            public int MaxDownloadSpeed { get; set; }
            public int MaxUploadSpeed { get; set; }
            public bool EnableDht { get; set; }
            public bool StopSeedingWhenFinished { get; set; }
            public bool EnablePortForwarding { get; set; }
            public string? DefaultDownloadPath { get; set; }
            public string? LogPath { get; set; }
            public string? SettingsPath { get; set; }
        }
    }
}
