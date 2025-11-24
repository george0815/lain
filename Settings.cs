using MonoTorrent.Client;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace lain;


internal class SettingsData
{

    //Variables for UI
    public static ushort HeaderHeight { get; set; } = 15;
    public static ushort LogoWidth { get; set; } = 55;

    // Ports
    public ushort Port { get; set; } = 55123;
    public ushort DhtPort { get; set; } = 55124;

    // Max connections / rates
    public ushort MaxConnections { get; set; } = 100;
    public int MaxDownloadSpeed { get; set; } = 1000;
    public int MaxUploadSpeed { get; set; } = 1000;

    // Client settings
    public bool DetailedLogging { get; set; } = true;
    public bool StopSeedingWhenFinished { get; set; } = true;
    public bool EnablePortForwarding { get; set; } = true;
    public string? DefaultDownloadPath { get; set; } = "";
    public string? LogPath { get; set; } = "";
    public string SettingsPath { get; set; } = "cfg.json";

    // Colors
    public Terminal.Gui.Color BackgroundColor { get; set; } = Terminal.Gui.Color.Black;
    public Terminal.Gui.Color TextColor { get; set; } = Terminal.Gui.Color.White;
    public Terminal.Gui.Color FocusBackgroundColor { get; set; } = Terminal.Gui.Color.White;
    public Terminal.Gui.Color FocusTextColor { get; set; } = Terminal.Gui.Color.Black;
    public Terminal.Gui.Color HotTextColor { get; set; } = Terminal.Gui.Color.BrightYellow;
}



internal static class Settings
{
    internal static SettingsData Current { get; private set; } = new();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        //Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

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

    internal static void Save()
    {
        try
        {
            var json = JsonSerializer.Serialize(Current, JsonOptions);
            File.WriteAllText(Current.SettingsPath ?? "cfg.json", json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving settings: {ex.Message}");
        }
    }

    internal static void Load()
    {
        try
        {
            if (!File.Exists(Current.SettingsPath))
                return;

            string json = File.ReadAllText(Current.SettingsPath);
            var loaded = JsonSerializer.Deserialize<SettingsData>(json, JsonOptions);

            if (loaded != null)
                Current = loaded;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading settings: {ex.Message}");
        }
    }
}
