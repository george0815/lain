using lain;
using lain.helpers;
using MonoTorrent;
using MonoTorrent.Client;
using System;
using System.Data;
using System.Xml.Linq;
using Terminal.Gui;
using TextCopy;

namespace lain.frameviews;

/// <summary>
/// Displays a list of active torrents with progress, peers, rates, and controls
/// for starting, stopping, seeding, and removing torrents.
/// 
/// This view is backed by a TableView and synchronizes with TorrentOperations events.
/// </summary>
public class TorrentListView : FrameView
{
    /// <summary>
    /// List of torrent managers backing this view.
    /// </summary>
    private readonly List<TorrentManager> _managers;

    /// <summary>
    /// Terminal.Gui table showing torrent info.
    /// </summary>
    private readonly TableView _table;

    /// <summary>
    /// DataTable holding the torrent rows displayed in _table.
    /// </summary>
    private readonly DataTable _tableData;

    /// <summary>
    /// Initialize the torrent list view with the provided managers.
    /// Sets up table columns and subscribes to progress updates.
    /// </summary>
    /// <param name="managers">List of TorrentManager objects</param>
    public TorrentListView(List<TorrentManager> managers)
        : base(Resources.Torrents)
    {
        _managers = managers;

        X = 20;
        Y = SettingsData.HeaderHeight;
        Width = Dim.Fill();
        Height = Dim.Fill();

        // --- Table Schema ---
        _tableData = new DataTable();

        // Define visible columns in a consistent order
        _tableData.Columns.Add(Resources.Name, typeof(string));
        _tableData.Columns.Add(Resources.State, typeof(string));
        _tableData.Columns.Add(Resources.Progress, typeof(string));
        _tableData.Columns.Add(Resources.Peers, typeof(string));
        _tableData.Columns.Add(Resources.Leechers, typeof(string));
        _tableData.Columns.Add(Resources.Seeders, typeof(string));
        _tableData.Columns.Add(Resources.Downloadrate, typeof(string));
        _tableData.Columns.Add(Resources.Uploadrate, typeof(string));

        // --- TableView Setup ---
        _table = new TableView()
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            Table = _tableData
        };

        Add(_table);

        // Load existing torrents into the manager
        TorrentOperations.LoadAllTorrents();

        // Subscribe to background progress updates
        TorrentOperations.UpdateProgress += Refresh;
    }

    /// <summary>
    /// Handles keyboard input for torrent control.
    /// Supports start/stop download, start/stop seeding, remove, and magnet link generation.
    /// </summary>
    /// <param name="keyEvent">The key event pressed by the user</param>
    /// <returns>True if the key was handled, otherwise false</returns>
    public override bool ProcessKey(KeyEvent keyEvent)
    {
        // Safety: ignore if no row is selected
        if (_table.SelectedRow < 0 || _table.SelectedRow >= _managers.Count)
            return base.ProcessKey(keyEvent);

        // --- Start download ---
        if (keyEvent.Key == Settings.Current.Controls.StartDownload)
        {
            Task.Run(async () =>
            {
                await TorrentOperations.ResumeTorrentAsync(_table.SelectedRow);
            });

            Log.Write(_table.SelectedRow.ToString());
            return true;
        }

        // --- Stop download ---
        else if (keyEvent.Key == Settings.Current.Controls.StopDownload)
        {
            Task.Run(async () =>
            {
                await TorrentOperations.PauseTorrentAsync(_table.SelectedRow);
            });

            Log.Write(_table.SelectedRow.ToString());
            return true;
        }

        // --- Start seeding ---
        else if (keyEvent.Key == Settings.Current.Controls.StartSeeding)
        {
            Task.Run(async () =>
            {
                await TorrentOperations.StartSeedingAsync(_table.SelectedRow);
            });

            Log.Write(_table.SelectedRow.ToString());
            return true;
        }

        // --- Stop seeding ---
        else if (keyEvent.Key == Settings.Current.Controls.StopSeeding)
        {
            Task.Run(async () =>
            {
                await TorrentOperations.StopSeedingAsync(_table.SelectedRow);
            });

            Log.Write(_table.SelectedRow.ToString());
            return true;
        }

        // --- Generate magnet link ---
        else if (keyEvent.Key == Settings.Current.Controls.GenMagLink)
        {
            var manager = _managers[_table.SelectedRow];
            var torrent = manager.Torrent!;

            var magnet = new MagnetLink(
                torrent.InfoHashes,
                torrent.Name,
                [.. torrent.AnnounceUrls.SelectMany(t => t.ToArray())]
            );

            Log.Write($"{Resources.MagnetlinkgeneratedmagnetToV1String__}{magnet.ToV1String()}");

            // Copy magnet link to clipboard and notify user
            if (MessageBox.Query($"", Resources.Magnetlinkcopiedtoclipboard, Resources.OK) == 0)
            {
                ClipboardService.SetText(magnet.ToV1String());
            }

            return true;
        }

        // --- Remove torrent ---
        else if (keyEvent.Key == Settings.Current.Controls.RemoveTorrent)
        {
            bool deleteFiles = false;

            if (MessageBox.Query(Resources.Deletedownloadedfiles_, "", Resources.Yes, Resources.No) == 0)
            {
                deleteFiles = true;
            }

            Task.Run(async () =>
            {
                await TorrentOperations.DeleteTorrentAsync(_table.SelectedRow, deleteFiles);
            });

            Log.Write(_table.SelectedRow.ToString());
            return true;
        }

        return base.ProcessKey(keyEvent);
    }

    /// <summary>
    /// Refreshes the TableView to reflect the latest torrent data.
    /// Called from the TorrentOperations.UpdateProgress event.
    /// </summary>
    public void Refresh()
    {
        // Ensure updates run on the main GUI thread
        Application.MainLoop.Invoke(() =>
        {
            // Clear existing rows
            _tableData.Clear();

            // Add each torrent's current state to the table
            foreach (var m in _managers)
            {
                string name = m.Name ?? "Unknown";
                string state = m.State.ToString();

                // Format progress as a percentage with one decimal
                string progress = $"{m.Progress:0.0}%";

                // Open connections and peers
                string peers = m.OpenConnections.ToString();
                string leechers = m.Peers.Leechs.ToString();
                string seeders = m.Peers.Seeds.ToString();

                // Download/upload rates in kB/s
                string downloadRate = $"{m.Monitor.DownloadRate / 1024:0.0} kB/s";
                string uploadRate = $"{m.Monitor.UploadRate / 1024:0.0} kB/s";

                _tableData.Rows.Add(
                    name,
                    state,
                    progress,
                    peers,
                    leechers,
                    seeders,
                    downloadRate,
                    uploadRate
                );
            }

            // Trigger redraws
            _table.Update();
            _table.SetNeedsDisplay();
            SetNeedsDisplay();
        });
    }
}
