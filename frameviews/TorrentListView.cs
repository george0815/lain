using lain;
using MonoTorrent.Client;
using System.Data;
using Terminal.Gui;

public class TorrentListView : FrameView
{
    private readonly List<TorrentManager> _managers;
    private readonly TableView _table;
    private readonly DataTable _tableData;

    public TorrentListView(List<TorrentManager> managers)
        : base("Torrents")
    {
        _managers = managers;

        X = 20;
        Y = 3;
        Width = Dim.Fill();
        Height = Dim.Fill();

        // Define the table's schema
        _tableData = new DataTable();
        _tableData.Columns.Add("Name", typeof(string));
        _tableData.Columns.Add("State", typeof(string));
        _tableData.Columns.Add("Progress", typeof(string));

        // Create the TableView
        _table = new TableView()
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            Table = _tableData
        };

        Add(_table);
      

        // Subscribe to log updates
        TorrentOperations.UpdateProgress += Refresh;
    }

    public void Refresh()
    {
        Application.MainLoop.Invoke(() =>
        {
            _tableData.Clear();

            foreach (var m in _managers)
            {
                string name = m.Torrent?.Name ?? "Unknown";
                string state = m.State.ToString();
                string progress = $"{m.Progress:0.0}%";

                _tableData.Rows.Add(name, state, progress);
            }

            _table.Update();
            _table.SetNeedsDisplay();
            SetNeedsDisplay();
        });
    }
}
