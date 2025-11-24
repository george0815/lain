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
        Y = SettingsData.HeaderHeight;
        Width = Dim.Fill();
        Height = Dim.Fill();

        // Define the table's schema
        _tableData = new DataTable();



        // Define columns
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


        // Subscribe to log updates
        TorrentOperations.UpdateProgress += Refresh;
    }


    // Method to refresh the torrent list display
    public void Refresh()
    {

        //clear existing rows and repopulate
        Application.MainLoop.Invoke(() =>
        {
            _tableData.Clear();

            foreach (var m in _managers)
            {
                string name = m.Torrent?.Name ?? "Unknown";
                string state = m.State.ToString();
                string progress = $"{m.Progress:0.0}%";
                string peers = m.OpenConnections.ToString() ?? "0";
                string leechers = m.Peers.Leechs.ToString() ?? "0";
                string seeders = m.Peers.Seeds.ToString() ?? "0";
                string downloadRate = $"{m.Monitor.DownloadRate / 1024:0.0}kB/s";
                string uploadRate = $"{m.Monitor.UploadRate / 1024:0.0}kB/s";
             


                _tableData.Rows.Add(name, state, progress, peers, leechers, seeders, downloadRate, uploadRate);
            }

            _table.Update();
            _table.SetNeedsDisplay();
            SetNeedsDisplay();
        });
    }
}
