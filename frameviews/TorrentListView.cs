using lain;
using lain.helpers;
using MonoTorrent;
using MonoTorrent.Client;
using System;
using System.Data;
using System.Xml.Linq;
using Terminal.Gui;
using TextCopy;

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

        TorrentOperations.LoadAllTorrents();

        // Subscribe to log updates
        TorrentOperations.UpdateProgress += Refresh;


    }

    //process key events for start/stop/remove/etc
    public override bool ProcessKey(KeyEvent keyEvent)
    {
        if (keyEvent.Key == Settings.Current.Controls.StartDownload)
        {

            
            Task.Run(async () =>
            {
                await TorrentOperations.ResumeTorrentAsync(_table.SelectedRow);
            });

            Log.Write(_table.SelectedRow.ToString());
            return true; // handled
        }
        else if (keyEvent.Key == Settings.Current.Controls.StopDownload)
        {
            Task.Run(async () =>
            {
                await TorrentOperations.PauseTorrentAsync(_table.SelectedRow);
            });
            Log.Write(_table.SelectedRow.ToString());
            return true; // handled
        }
        else if (keyEvent.Key == Settings.Current.Controls.StartSeeding)
        {
           
            Task.Run(async () =>
            {
                await TorrentOperations.StartSeedingAsync(_table.SelectedRow);
            });
            Log.Write(_table.SelectedRow.ToString());
            return true; // handled
        }
        else if (keyEvent.Key == Settings.Current.Controls.StopSeeding)
        {
            Task.Run(async () =>
            {
                await TorrentOperations.StopSeedingAsync(_table.SelectedRow);
            });
            Log.Write(_table.SelectedRow.ToString());
            return true; // handled
        }
        else if (keyEvent.Key == Settings.Current.Controls.GenMagLink)
        {
            var magnet = new MagnetLink(_managers[_table.SelectedRow].Torrent!.InfoHashes, _managers[_table.SelectedRow].Torrent!.Name,
                    _managers[_table.SelectedRow].Torrent!.AnnounceUrls.SelectMany(t => t.ToArray()).ToList());

            Log.Write($"Magnet link generated: {magnet.ToV1String()}");

            if (MessageBox.Query($"", "Magnet link copied to clipboard.",
                        "OK") == 0)
            {
                ClipboardService.SetText(magnet.ToV1String());
            }
  

            return true; // handled
        }
        else if (keyEvent.Key == Settings.Current.Controls.RemoveTorrent)
        {
            bool deleteFiles = false;

            if (MessageBox.Query("Delete downloaded files?",
                            "",
                            "Yes", "No") == 0)
            {
              
                deleteFiles = true;
            }

            Task.Run(async () =>
            {
                await TorrentOperations.DeleteTorrentAsync(_table.SelectedRow, deleteFiles);
            });
            Log.Write(_table.SelectedRow.ToString());
            return true; // handled
        }

        return base.ProcessKey(keyEvent);
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
                string name = m.Name ?? "Unknown";
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
