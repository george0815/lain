using lain;
using MonoTorrent.Client;
using Terminal.Gui;

internal class LogView : FrameView
{
    public ListView ListView { get; set; }

    public LogView(List<string> log)
        : base("Log")
    {
        X = 20;
        Y = Settings.HeaderHeight;
        Width = Dim.Fill();
        Height = Dim.Fill();

        ListView = new ListView(log)
        {
            X = 1,
            Y = 1,
            Width = Dim.Fill() - 2,
            Height = Dim.Fill() - 2
        };

        Add(ListView);

        // Subscribe to log updates
        Log.OnLogAdded += RefreshLog;
    }

    internal void RefreshLog()
    {
        Application.MainLoop.Invoke(() =>
        {
            ListView.SetSource(Log.log);
            ListView.SetNeedsDisplay();
            SetNeedsDisplay();
        });
    }
}
