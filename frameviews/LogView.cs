using lain;
using lain.helpers;
using MonoTorrent.Client;
using Terminal.Gui;

internal class LogView : FrameView
{
    public ListView ListView { get; set; } = new();

    public LogView(List<string> log)
        : base(Resources.Log)
    {
        X = 20;
        Y = SettingsData.HeaderHeight;
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

    // Method to refresh the log display
    internal void RefreshLog()
    {
        Application.MainLoop.Invoke(() =>
        {
            try
            {
                // Save scroll position + selected row
                int selected = ListView.SelectedItem;
                int top = ListView.TopItem;

                // Update data
                ListView.SetSource(Log.log);

                // Restore scroll position (safely)
                if (top < ListView.Source.Count)
                    ListView.TopItem = top;

                // Restore selected item (safely)
                if (selected < ListView.Source.Count)
                    ListView.SelectedItem = selected;
                else
                    ListView.SelectedItem = ListView.Source.Count - 1;

                ListView.SetNeedsDisplay();
                SetNeedsDisplay();
            }
            catch { }
        });
    }

}
