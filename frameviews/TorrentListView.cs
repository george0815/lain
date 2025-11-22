using lain;
using MonoTorrent.Client;
using System;
using System.Data;
using System.Xml.Linq;
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
        Y = Settings.HeaderHeight;
        Width = Dim.Fill();
        Height = Dim.Fill();

        // Define the table's schema
        _tableData = new DataTable();

        


        _tableData.Columns.Add("Name", typeof(string));
        _tableData.Columns.Add("State", typeof(string));
        _tableData.Columns.Add("Progress", typeof(string));
        _tableData.Columns.Add("Peers", typeof(string));
        _tableData.Columns.Add("Leechers", typeof(string));
        _tableData.Columns.Add("Seeders", typeof(string));
        _tableData.Columns.Add("Download rate", typeof(string));
        _tableData.Columns.Add("Upload rate", typeof(string));


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

       
        
        for (int i = 0; i <= 10; i++)
        {
            _tableData.Rows.Add(432423, 4324, 432423);
            // Add a blank row to act as a separator
        }

        

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
