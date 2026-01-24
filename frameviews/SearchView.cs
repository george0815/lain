using lain.helpers;
using MonoTorrent;
using System;
using System.ComponentModel.Design;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using Terminal.Gui;
using TextCopy;
using static NStack.Unicode;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace lain.frameviews
{
    #region DATA STRUCTS

    /// <summary>
    /// Holds the raw response returned by the Ghidorah Python subprocess.
    /// This is deserialized directly from JSON before any validation or cleanup.
    /// </summary>
    internal class GhidorahResponse
    {
        public List<GhidorahItem>? Data { get; set; }
        public List<string>? Errors { get; set; }
    }

    /// <summary>
    /// Represents a single torrent entry as returned by Ghidorah,
    /// prior to sanitization and default-value enforcement.
    /// </summary>
    internal class GhidorahItem
    {
        public string? Name { get; set; }
        public long? Size { get; set; }

        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int? Seeders { get; set; }

        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int? Leechers { get; set; }

        public string? Category { get; set; }
        public string? Source { get; set; }
        public string? Url { get; set; }
        public string? Date { get; set; }
        public string? Magnet { get; set; }
        public string? Hash { get; set; }
    }

    /// <summary>
    /// Sanitized, non-nullable torrent data used internally by the UI.
    /// All fields are guaranteed to have usable defaults.
    /// </summary>
    internal struct TorrentResult
    {
        internal string Name;
        internal long Size;
        internal int Seeders;
        internal int Leechers;
        internal string Category;
        internal string Source;
        internal string Url;
        internal string Date;
        internal string Magnet;
        internal string Hash;
    }

    #endregion

    /// <summary>
    /// Search view responsible for querying Ghidorah, displaying results,
    /// and allowing quick-start downloads via keyboard shortcuts.
    /// </summary>
    internal class SearchView : FrameView
    {
        private List<TorrentResult>? torrents;

        private readonly TableView _table;
        private readonly DataTable _tableData;

        public SearchView()
            : base(Resources.Search)
        {
            X = 20;
            Y = SettingsData.HeaderHeight;
            Width = Dim.Fill();
            Height = Dim.Fill();

            #region UI ELEMENTS

            var searchBar = new TextField("")
            {
                X = 1,
                Y = 1,
                Width = 40
            };

            var searchBtn = new Button(Resources.Search)
            {
                X = Pos.Right(searchBar) + 2,
                Y = 1
            };

            // Backing DataTable used by the TableView
            _tableData = new DataTable();

            // Define visible columns (order matters)
            _tableData.Columns.Add(Resources.Name, typeof(string));
            _tableData.Columns.Add(Resources.Seeders, typeof(string));
            _tableData.Columns.Add(Resources.Leechers, typeof(string));
            _tableData.Columns.Add(Resources.Size, typeof(string));
            _tableData.Columns.Add(Resources.MagnetLinkSearch, typeof(string));
            _tableData.Columns.Add(Resources.Date, typeof(string));
            _tableData.Columns.Add(Resources.Category, typeof(string));
            _tableData.Columns.Add(Resources.Source, typeof(string));
            _tableData.Columns.Add(Resources.Url, typeof(string));
            _tableData.Columns.Add(Resources.Hash, typeof(string));

            // TableView rendering the search results
            _table = new TableView()
            {
                X = 0,
                Y = 3,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                Table = _tableData
            };

            #endregion

            searchBtn.Clicked += () =>
            {
                // Initialize storage for incoming results
                torrents = [];

                // Build Ghidorah search arguments from current settings
                SearchArgs args = new()
                {
                    Query = searchBar.Text.ToString() ?? "",
                    Limit = Settings.Current.SearchResultsLimitPerSource,
                    TotalLimit = Settings.Current.SearchResultsLimit,
                    Sources = Settings.Current.SearchSources,
                    Categories = Settings.Current.Categories,
                    SortBy = Settings.Current.SortBy
                };

                Task.Run(() =>
                {
                    _tableData.Clear();

                    int seconds = 0;

                    // Used only to stop the timer cleanly
                    var cts = new CancellationTokenSource();

                    // Update the search button with elapsed time
                    var timer = new Timer(_ =>
                    {
                        seconds++;
                        Application.MainLoop.Invoke(() =>
                        {
                            searchBtn.Text = $"Searching... {seconds}s";
                            searchBtn.SetNeedsDisplay();
                        });
                    }, null, 0, 1000);

                    // Perform the search in the background
                    Task.Run(() =>
                    {
                        try
                        {
                            var res = Ghidorah.Search(args);

                            Application.MainLoop.Invoke(() =>
                            {
                                DisplayResults(res);
                                searchBtn.Text = "Search";
                                searchBtn.SetNeedsDisplay();
                            });
                        }
                        finally
                        {
                            cts.Cancel();
                            timer.Dispose();
                        }
                    });
                });
            };

            Add(searchBar, searchBtn, _table);
        }

        #region HELPER METHODS

       
        /// <summary>
        /// Converts the raw JSON response into sanitized TorrentResult entries.
        /// Handles deserialization errors and reported Ghidorah failures.
        /// </summary>
        private List<TorrentResult> Sanitize(string res)
        {
            if (string.IsNullOrWhiteSpace(res))
                return torrents!;

            GhidorahResponse? parsed;

            try
            {
                parsed = System.Text.Json.JsonSerializer.Deserialize<GhidorahResponse>(res);
            }
            catch (Exception e)
            {
                Log.Write($"{Resources.Error}: {e.Message}");
                MessageBox.ErrorQuery(Resources.Error, e.Message, Resources.OK);
                return torrents!;
            }

            if ((parsed?.Data == null || parsed?.Data.Count == 0) &&
                parsed?.Errors?.Count > 0)
            {
                Log.Write($"{Resources.Error}: {parsed.Errors[0]}");
                MessageBox.ErrorQuery(Resources.Error, parsed.Errors[0], Resources.OK);
                return torrents!;
            }

            foreach (var item in parsed!.Data!)
            {
                var torrent = new TorrentResult
                {
                    Name = item.Name ?? "N/A",
                    Size = item.Size ?? 0,
                    Seeders = item.Seeders ?? 0,
                    Leechers = item.Leechers ?? 0,
                    Category = item.Category ?? "N/A",
                    Source = item.Source ?? "N/A",
                    Url = item.Url ?? "N/A",
                    Date = item.Date ?? "N/A",
                    Magnet = item.Magnet ?? "N/A",
                    Hash = item.Hash ?? "N/A"
                };

                torrents!.Add(torrent);
            }

            return torrents!;
        }

        /// <summary>
        /// Renders sanitized search results into the TableView.
        /// </summary>
        private void DisplayResults(string rawString)
        {
            var res = Sanitize(rawString);

            _tableData.Clear();

            for (int i = 0;
                 i < res.Count && i < Settings.Current.SearchResultsLimit;
                 i++)
            {
                string name = res[i].Name ?? "N/A";
                long size = res[i].Size;
                int seeders = res[i].Seeders;
                int leechers = res[i].Leechers;
                string category = res[i].Category ?? "N/A";
                string source = res[i].Source ?? "N/A";
                string url = res[i].Url ?? "N/A";
                string date = res[i].Date ?? "N/A";
                string magnet = res[i].Magnet ?? "N/A";
                string hash = res[i].Hash ?? "N/A";

                _tableData.Rows.Add(
                    name.Truncate(20),
                    seeders,
                    leechers,
                    FormatSizeBytes(size),
                    magnet.Truncate(15),
                    date,
                    category,
                    source,
                    url.Truncate(15),
                    hash.Truncate(15)
                );
            }

            _table.Table = _tableData;
            _table.Update();
            _table.SetNeedsDisplay();
            SetNeedsDisplay();
        }

        /// <summary>
        /// Formats a byte count into a human-readable string.
        /// </summary>
        public static string FormatSizeBytes(long numBytes, int precision = 1)
        {
            if (numBytes <= 0)
                return "0 B";

            double size = numBytes;
            string[] units = ["B", "KB", "MB", "GB", "TB", "PB"];
            const double step = 1024.0;

            foreach (var unit in units)
            {
                if (size < step)
                {
                    if (unit == "B")
                        return $"{(long)size} {unit}";

                    return $"{Math.Round(size, precision).ToString($"F{precision}", CultureInfo.InvariantCulture)} {unit}";
                }

                size /= step;
            }

            return $"{Math.Round(size, precision).ToString($"F{precision}", CultureInfo.InvariantCulture)} PB";
        }

        /// <summary>
        /// Handles keyboard input, including starting a download when the
        /// configured download key is pressed on a valid magnet entry.
        /// </summary>
        public override bool ProcessKey(KeyEvent keyEvent)
        {
            var torrent = torrents?.ElementAtOrDefault(_table.SelectedRow);

            if (keyEvent.Key == Settings.Current.Controls.StartDownload &&
                torrent != null &&
                torrents![_table.SelectedRow].Magnet != "N/A" &&
                torrents![_table.SelectedRow].Magnet.Contains("magnet:?"))
            {
                TorrentData settings = new()
                {
                    UseMagnetLink = true,
                    MagnetUrl = torrents?[_table.SelectedRow].Magnet!,
                    TorPath = torrents?[_table.SelectedRow].Name!,
                    DownPath = Settings.Current.DefaultDownloadPath!,
                    MaxConnections = Settings.Current.MaxConnections,
                    MaxDownloadRate = Settings.Current.MaxDownloadSpeed * 1024 * 1024,
                    MaxUploadRate = Settings.Current.MaxUploadSpeed * 1024 * 1024,
                    UseDht = true
                };

                MessageBox.Query(Resources.Download, Resources.Torrentdownloadstarted, Resources.OK);

                Task.Run(async () =>
                {
                    try
                    {
                        await TorrentOperations.AddTorrent(settings, false, false);
                    }
                    catch (Exception ex)
                    {
                        Application.MainLoop.Invoke(() =>
                        {
                            MessageBox.ErrorQuery(
                                Resources.Error,
                                $"{Resources.Torrentdownloadfailed}\n{ex.Message}",
                                Resources.OK
                            );
                        });
                    }
                });
            }
            else if (keyEvent.Key == Settings.Current.Controls.StartDownload &&
                     torrent != null &&
                     torrents![_table.SelectedRow].Magnet == "N/A")
            {
                MessageBox.ErrorQuery(Resources.Error, Resources.Nomagnetlink, Resources.OK);
            }

            return base.ProcessKey(keyEvent);
        }

        #endregion
    }
}
