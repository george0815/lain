using lain.helpers;
using MonoTorrent;
using System;
using System.ComponentModel.Design;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Xml.Linq;
using Terminal.Gui;
using TextCopy;
using static NStack.Unicode;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace lain.frameviews
{

    #region DATA STRUCTS

    //holds data received from python subprocess
    internal class GhidorahResponse
    {
        public List<GhidorahItem>? data { get; set; }

        public List<string>? errors { get; set; }

    }


    //holds individual torrent item data before sanitization
    internal class GhidorahItem
    {
        public string? name { get; set; }
        public string? size { get; set; }
        public string? seeders { get; set; }
        public string? leechers { get; set; }
        public string? category { get; set; }
        public string? source { get; set; }
        public string? url { get; set; }
        public string? date { get; set; }
        public string? magnet { get; set; }
        public string? hash { get; set; }
    }


    //holds sanitized torrent data
    internal struct TorrentResult
    {
        internal string Name;
        internal int Size;
        internal int Seeders;
        internal int Leechers;
        internal string Category;
        internal string Source;
        internal string Url;
        internal string Date;
        internal string Magnet;
        internal string Hash;

    };

    #endregion

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

           

            // Define the table's schema
            _tableData = new DataTable();



            // Define columns
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


            // Create the TableView
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

                    //initialize ghidorah search args
                    torrents = new List<TorrentResult>();
                    SearchArgs args = new SearchArgs
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

                        // Start counter
                        int seconds = 0;

                        // CancellationTokenSource to stop the timer
                        var cts = new CancellationTokenSource();

                        // Update button text every second
                        var timer = new Timer(_ =>
                        {
                            seconds++;
                            Application.MainLoop.Invoke(() =>
                            {
                                searchBtn.Text = $"Searching... {seconds}s";
                                searchBtn.SetNeedsDisplay();
                            });
                        }, null, 0, 1000); // first 0ms delay, then every 1000ms

                        // Run the search in the background
                        Task.Run(() =>
                        {
                            try
                            {
                                var res = Ghidorah.Search(args); // your long-running search

                                // Update UI with results
                                Application.MainLoop.Invoke(() =>
                                {
                                    DisplayResults(res);
                                    searchBtn.Text = "Search"; // reset button text
                                    searchBtn.SetNeedsDisplay();
                                });
                            }
                            finally
                            {
                                // Ensure timer stops even if search throws
                                cts.Cancel();
                                timer.Dispose();
                            }
                        });
                    });




                };

            Add(searchBar, searchBtn, _table);

        }

        #region HELPER METHODS

        //parse size string into int in bytes
        private static int ParseSize(string? size)
        {
            if (string.IsNullOrWhiteSpace(size) || size == "N/A")
                return 0;

            var parts = size.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2)
                return 0;

            if (!double.TryParse(parts[0], out var number))
                return 0;

            var unit = parts[1].ToUpperInvariant();

            var multipliers = new Dictionary<string, int>
            {
                ["KIB"] = 1,
                ["MIB"] = 1024,
                ["GIB"] = 1024 * 1024,
                ["TIB"] = 1024 * 1024 * 1024,
                ["KiB"] = 1,
                ["MiB"] = 1024,
                ["GiB"] = 1024 * 1024,
                ["TiB"] = 1024 * 1024 * 1024,
                ["kb"] = 1,
                ["mb"] = 1024,
                ["gb"] = 1024 * 1024,
                ["tb"] = 1024 * 1024 * 1024,
                ["KB"] = 1,
                ["MB"] = 1024,
                ["GB"] = 1024 * 1024,
                ["TB"] = 1024 * 1024 * 1024
            };

            return multipliers.TryGetValue(unit, out var mul)
                ? (int)(number * mul)
                : 0;
        }


        //sanitize raw json response into structured data
        private List<TorrentResult> Sanitize(string res)
        {

            if (string.IsNullOrWhiteSpace(res))
                return torrents;

            GhidorahResponse? parsed;

            try
            {
                parsed = System.Text.Json.JsonSerializer.Deserialize<GhidorahResponse>(res);
            }
            catch (Exception e)
            {
                Log.Write($"{Resources.Error}: {e.Message}");
                MessageBox.ErrorQuery(Resources.Error, e.Message, Resources.OK);

                return torrents;
            }

            if ((parsed?.data == null || parsed?.data.Count == 0) && parsed?.errors?.Count > 0)
            {
                Log.Write($"{Resources.Error}: {parsed?.errors[0]}");
                MessageBox.ErrorQuery(Resources.Error, parsed?.errors[0], Resources.OK);
                return torrents;
            }

            foreach (var item in parsed.data)
            {
                var torrent = new TorrentResult
                {
                    Name = item.name ?? "N/A",
                    Size = ParseSize(item.size),
                    Seeders = int.TryParse(item.seeders, out var s) ? s : 0,
                    Leechers = int.TryParse(item.leechers, out var l) ? l : 0,
                    Category = item.category ?? "N/A",
                    Source = item.source ?? "N/A",
                    Url = item.url ?? "N/A",
                    Date = item.date ?? "N/A",
                    Magnet = item.magnet ?? "N/A",
                    Hash = item.hash ?? "N/A"
                };

                torrents.Add(torrent);
            }

            return torrents;
        }


        //display search results in table
        private void DisplayResults(string rawString)
        {

            var res = Sanitize(rawString);

            _tableData.Clear();

            for (int i = 0; (i < res.Count && i < Settings.Current.SearchResultsLimit); i++)
            {

                string name = res[i].Name ?? "N/A";
                int size = res[i].Size;
                int seeders = res[i].Seeders;
                int leechers = res[i].Leechers;
                string category = res[i].Category ?? "N/A";
                string source = res[i].Source ?? "N/A";
                string url = res[i].Url ?? "N/A";
                string date = res[i].Date ?? "N/A";
                string magnet = res[i].Magnet ?? "N/A";
                string hash = res[i].Hash ?? "N/A";

                _tableData.Rows.Add(name.Truncate(15), seeders, leechers, size, magnet.Truncate(15), date, category, source, url.Truncate(15), hash.Truncate(15));
            }
            _table.Table = _tableData;
            _table.Update();
            _table.SetNeedsDisplay();
            SetNeedsDisplay();

        }


        //if there's a magnet link selected, start download on donwload key press
        public override bool ProcessKey(KeyEvent keyEvent)

        {
            var torrent = torrents?.ElementAtOrDefault(_table.SelectedRow);

            if (keyEvent.Key == Settings.Current.Controls.StartDownload && 
                torrent != null && torrents![_table.SelectedRow].Magnet != "N/A" && 
                torrents![_table.SelectedRow].Magnet.Contains("magnet:?"))
            {

                TorrentData settings = new TorrentData
                {
                    UseMagnetLink = true,
                    MagnetUrl = torrents?[_table.SelectedRow].Magnet!,
                    TorPath = torrents?[_table.SelectedRow].Name!,
                    DownPath = Settings.Current.DefaultDownloadPath!,
                    MaxConnections = Settings.Current.MaxConnections,
                    MaxDownloadRate = Settings.Current.MaxDownloadSpeed /* to KB*/ * 1024 /* to MB*/ * 1024,
                    MaxUploadRate = Settings.Current.MaxUploadSpeed /* to KB*/ * 1024 /* to MB*/ * 1024,
                    UseDht = true
                };

                MessageBox.Query(Resources.Download, Resources.Torrentdownloadstarted, Resources.OK);


                //Add torrent asynchronously
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
                            MessageBox.ErrorQuery(Resources.Error, $"{Resources.Torrentdownloadfailed}\n{ex.Message}", Resources.OK);
                        });
                    }
                });


                
            }

            else if (keyEvent.Key == Settings.Current.Controls.StartDownload && torrent != null && torrents![_table.SelectedRow].Magnet == "N/A")
            {

                MessageBox.ErrorQuery(Resources.Error, $"{Resources.Nomagnetlink}", Resources.OK);

            }


            return base.ProcessKey(keyEvent);
        }
        

        #endregion


    }
}
