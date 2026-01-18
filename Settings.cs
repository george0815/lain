using lain.helpers;
using MonoTorrent.Client;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Terminal.Gui;

namespace lain;


internal class SettingsData
{

    //Variables for UI
    public static ushort HeaderHeight { get; set; } = 15;
    public static ushort LogoWidth { get; set; } = 55;

    //Ports
    public ushort Port { get; set; } = 55123;
    public ushort DhtPort { get; set; } = 55124;

    // Max connections / rates
    public ushort MaxConnections { get; set; } = 100;
    public int MaxDownloadSpeed { get; set; } = 100 /* to KB*/ * 1024 /* to MB*/ * 1024;
    public int MaxUploadSpeed { get; set; } = 100 /* to KB*/ * 1024 /* to MB*/ * 1024;
    public int RefreshInterval { get; set; } = 1000;

    // Client settings
    public bool DetailedLogging { get; set; } = true;
    public bool StopSeedingWhenFinished { get; set; } = true;
    public bool EnablePortForwarding { get; set; } = true;
    public bool HidetextCursor { get; set; } = true;    
    public bool DisableColoredHotkeyInfo { get; set; } = false;
    public bool DisableASCII { get; set; } = false;
    public string? DefaultDownloadPath { get; set; } = "./";
    public string? LogPath { get; set; } = "./log.txt";
    public string SettingsPath { get; set; } = "cfg.json";

    // Colors
    public Terminal.Gui.Color BackgroundColor { get; set; } = Terminal.Gui.Color.Black;
    public Terminal.Gui.Color TextColor { get; set; } = Terminal.Gui.Color.White;
    public Terminal.Gui.Color FocusBackgroundColor { get; set; } = Terminal.Gui.Color.White;
    public Terminal.Gui.Color FocusTextColor { get; set; } = Terminal.Gui.Color.Black;
    public Terminal.Gui.Color HotTextColor { get; set; } = Terminal.Gui.Color.BrightYellow;
    public Terminal.Gui.Color LogoColor { get; set; } = Terminal.Gui.Color.White;

    //Controls
    public TorrentHotkeys Controls { get; set; } = new TorrentHotkeys
    {
        StartDownload = Key.F3,
        StopDownload = Key.F4,
        StartSeeding = Key.F5,
        StopSeeding = Key.F6,
        RemoveTorrent = Key.F7,
        GenMagLink = Key.F8
    };

    //Icons
    public List<string> icons { get; set; } = ASCII.icons;


    //Search settings

    public int SearchResultsLimit { get; set; } = 20;

    public int Timeout { get; set; } = 20 * 1_000;


    public int SearchResultsLimitPerSource { get; set; } = 20;



    public String[] DefaultSources { get; set; } = [
                    "kickasstorrents",
                        "thepiratebay",
                        "limetorrents",
                        "yts",
                        "x1337",
                        "torrentgalaxy"];


    public String[] SearchSources { get; set; } = ["thepiratebay", "limetorrents", "kickasstorrents" ];
    public String[] Categories { get; set; } = [Resources.Movies, Resources.Games, 
                                                Resources.TVshows, Resources.Applications, Resources.Other ];
    public string SortBy { get; set; } = Resources.Source;

    public bool UseQbittorrentPlugins { get; set; } = true;


}



internal static class Settings
{
    internal static SettingsData Current { get; private set; } = new();

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true, };

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

    internal static void Load()
    {
        try
        {
            if (!File.Exists(Current.SettingsPath)) { Settings.Save(); return; }

            string json = File.ReadAllText(Current.SettingsPath);
            var loaded = JsonSerializer.Deserialize<SettingsData>(json, JsonOptions);

            if (loaded != null) { Current = loaded; if (Current.DisableASCII) SettingsData.HeaderHeight = 5; }
            else { Settings.Save(); }

        }
        catch (Exception ex)
        {
            Console.WriteLine($"{Resources.ErrorloadingsettingsexMessage} {ex.Message}");
            Settings.Save();
        }
    }

    #endregion

}