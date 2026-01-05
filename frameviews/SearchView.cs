using lain.helpers;
using MonoTorrent;
using System.Data;
using System.Diagnostics;
using Terminal.Gui;
using TextCopy;

namespace lain.frameviews
{

    internal class GhidorahResponse
    {
        public List<GhidorahItem>? data { get; set; }
    }

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



    internal struct TorrentResult
    {
        internal String Name;
        internal int Size;
        internal int Seeders;
        internal int Leechers;
        internal String Category;
        internal String Source;
        internal String Url;
        internal String Date;
        internal String Magnet;
        internal String Hash;

    };



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


            searchBtn.Clicked += () =>
                {

                    //initialize ghidorah search args
                    torrents = new List<TorrentResult>();
                    SearchArgs args = new SearchArgs
                    {
                        Query = searchBar.Text.ToString() ?? "",
                        Limit = Settings.Current.SearchResultsLimit,
                        TotalLimit = Settings.Current.SearchResultsLimit,
                        Sources = Settings.Current.SearchSources,
                        Categories = Settings.Current.Categories,
                        SortBy = Resources.Source
                    };

                    Task.Run(() =>
                    {
                        var res = Ghidorah.Search(args);

                        Application.MainLoop.Invoke(() =>
                        {
                            DisplayResults(res);
                        });
                    });




                };

            Add(searchBar, searchBtn, _table);

        }

        //HELPER FUNCTIONS

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
                Debug.WriteLine($"{Resources.Error}: {e.Message}");

                return torrents;
            }

            if (parsed?.data == null)
                return torrents;

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

        

        private void DisplayResults(string rawString)
        {

            var res = Sanitize(rawString);

            _tableData.Clear();

            foreach (var m in res)
            {

                string name = m.Name ?? "N/A";
                int size = m.Size;
                int seeders = m.Seeders;
                int leechers = m.Leechers;
                string category = m.Category ?? "N/A";
                string source = m.Source ?? "N/A";
                string url = m.Url ?? "N/A";
                string date = m.Date ?? "N/A";
                string magnet = m.Magnet ?? "N/A";
                string hash = m.Hash ?? "N/A";

                _tableData.Rows.Add(name.Truncate(15), seeders, leechers, size, magnet.Truncate(15), date, category, source, url.Truncate(15), hash.Truncate(15));
            }
            _table.Table = _tableData;
            _table.Update();
            _table.SetNeedsDisplay();
            SetNeedsDisplay();
            Application.Refresh();

        }


        //if there's a magnet link selected, start download on donwload key press
        public override bool ProcessKey(KeyEvent keyEvent)
        {
            if (keyEvent.Key == Settings.Current.Controls.StartDownload)
            {


                #region SETUP SETTINGS OBJECT AND START DOWNLOAD

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



                #endregion
            }


            return base.ProcessKey(keyEvent);
        }


    }
}
