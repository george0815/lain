using System;
using System.Collections.Generic;
using Terminal.Gui;

namespace lain
{
    public class LainUI : Window
    {
        List<string> ActiveTorrents = new List<string>()
    {
        "Ubuntu ISO",
        "Arch Linux ISO",
        "Fedora ISO"
    };

        // Content views
        FrameView torrentListView;
        FrameView downloadView;
        FrameView createView;
        FrameView settingsView;
        FrameView searchView;

        public LainUI()
        {
            //Title = "Torrent Client - Terminal.Gui";


            // --- HEADER -------------------------------------------------------
            var header = new FrameView()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = 3,
                CanFocus = false,
                Border = new Border() { BorderStyle = BorderStyle.None }
            };

            // Image placeholder
            var logo = new Label()
            {
                X = 1,
                Y = 1,
                Text = "[IMG]"
            };

            // Date
            var date = new Label()
            {
                X = 10,
                Y = 1,
                Text = DateTime.Now.ToString("yyyy-MM-dd")
            };

            // Active torrents
            var torrentCount = new Label()
            {
                X = Pos.Right(date) + 5,
                Y = 1,
                Text = $"Active Torrents: {ActiveTorrents.Count}"
            };

            header.Add(logo, date, torrentCount);
            Add(header);

            // --- SIDEBAR MENU ------------------------------------------------
            var menu = new ListView(new string[]
            {
            "Torrent List",
            "Download",
            "Create",
            "Settings",
            "Search",
            "Exit"
            })
            {
                X = 0,
                Y = 3,
                Width = 20,
                Height = Dim.Fill()
            };

            menu.SelectedItemChanged += (args) =>
            {
                SwitchPanel(args.Item);
            };

            Add(menu);

            // --- CONTENT PANELS ----------------------------------------------
            torrentListView = BuildTorrentList();
            downloadView = BuildDownloadView();
            createView = BuildCreateView();
            settingsView = BuildSettingsView();
            searchView = BuildSearchView();

            // Default panel
            Add(torrentListView);
        }

        // Switches visible panel
        private void SwitchPanel(int index)
        {
            Remove(torrentListView);
            Remove(downloadView);
            Remove(createView);
            Remove(settingsView);
            Remove(searchView);

            switch (index)
            {
                case 0: Add(torrentListView); break;
                case 1: Add(downloadView); break;
                case 2: Add(createView); break;
                case 3: Add(settingsView); break;
                case 4: Add(searchView); break;
                case 5: Application.RequestStop(); break;
            }

            SetNeedsDisplay();
        }

        // --------------------- TORRENT LIST PANEL ----------------------------
        private FrameView BuildTorrentList()
        {
            var panel = new FrameView("Torrent List")
            {
                X = 20,
                Y = 3,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };

            var list = new ListView(ActiveTorrents)
            {
                X = 0,
                Y = 0
            };

            panel.Add(list);

            // Pause/resume with key P
            list.KeyPress += (e) =>
            {
                if (e.KeyEvent.Key == Key.P)
                {
                    MessageBox.Query("Torrent", "Pause/Resume requested", "OK");
                    e.Handled = true;
                }
            };

            return panel;
        }

        // --------------------- DOWNLOAD PANEL --------------------------------
        private FrameView BuildDownloadView()
        {
            var panel = new FrameView("Download Torrent")
            {
                X = 20,
                Y = 3,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };

            var magnetLabel = new Label("Magnet URL:")
            {
                X = 1,
                Y = 1
            };

            var magnetInput = new TextField("")
            {
                X = 15,
                Y = 1,
                Width = 40
            };

            var fileLabel = new Label("Torrent File Path:")
            {
                X = 1,
                Y = 3
            };

            var fileInput = new TextField("")
            {
                X = 15,
                Y = 3,
                Width = 40
            };

            var downloadBtn = new Button("Download")
            {
                X = 1,
                Y = 5
            };

            panel.Add(magnetLabel, magnetInput, fileLabel, fileInput, downloadBtn);
            return panel;
        }

        // --------------------- CREATE TORRENT PANEL ---------------------------
        private FrameView BuildCreateView()
        {
            var panel = new FrameView("Create Torrent")
            {
                X = 20,
                Y = 3,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };

            panel.Add(new Label("File/Folder:") { X = 1, Y = 1 });
            panel.Add(new TextField("") { X = 15, Y = 1, Width = 40 });

            panel.Add(new Label("Trackers:") { X = 1, Y = 3 });
            panel.Add(new TextView() { X = 15, Y = 3, Width = 40, Height = 5 });

            panel.Add(new Label("Piece Size:") { X = 1, Y = 9 });
            panel.Add(new TextField("16384") { X = 15, Y = 9, Width = 10 });

            panel.Add(new Button("Create Torrent") { X = 1, Y = 11 });

            return panel;
        }

        // --------------------- SETTINGS PANEL ---------------------------------
        private FrameView BuildSettingsView()
        {

            #region VIEW CREATION

            var panel = new FrameView("Settings")
            {
                X = 20,
                Y = 3,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };

            int y = 1; // starting Y position

            // Port
            panel.Add(new Label("Port:") { X = 1, Y = y });
            var portField = new TextField(Settings.DhtPort.ToString()) { X = 20, Y = y, Width = 10 };
            panel.Add(portField);
            y += 2;

            // Max Connections
            panel.Add(new Label("Max Connections:") { X = 1, Y = y });
            var maxConnField = new TextField(Settings.MaxConnections.ToString()) { X = 20, Y = y, Width = 10 };
            panel.Add(maxConnField);
            y += 2;

            // Max Seeders per Torrent
            panel.Add(new Label("Max Seeders/Torrent:") { X = 1, Y = y });
            var maxSeedField = new TextField(Settings.MaxSeedersPerTorrent.ToString()) { X = 20, Y = y, Width = 10 };
            panel.Add(maxSeedField);
            y += 2;

            // Max Leechers per Torrent
            panel.Add(new Label("Max Leechers/Torrent:") { X = 1, Y = y });
            var maxLeechField = new TextField(Settings.MaxLeechersPerTorrent.ToString()) { X = 20, Y = y, Width = 10 };
            panel.Add(maxLeechField);
            y += 2;

            // Max Download Speed
            panel.Add(new Label("Max Download Speed (kB/s):") { X = 1, Y = y });
            var maxDlField = new TextField(Settings.MaxDownloadSpeed.ToString()) { X = 25, Y = y, Width = 10 };
            panel.Add(maxDlField);
            y += 2;

            // Max Upload Speed
            panel.Add(new Label("Max Upload Speed (kB/s):") { X = 1, Y = y });
            var maxUpField = new TextField(Settings.MaxUploadSpeed.ToString()) { X = 25, Y = y, Width = 10 };
            panel.Add(maxUpField);
            y += 2;

            // Enable DHT
            var dhtCheckbox = new CheckBox("Enable DHT") { X = 1, Y = y, Checked = Settings.EnableDht };
            panel.Add(dhtCheckbox);
            y += 2;

            // Stop Seeding When Finished
            var stopSeedCheckbox = new CheckBox("Stop Seeding When Finished") { X = 1, Y = y, Checked = Settings.StopSeedingWhenFinished };
            panel.Add(stopSeedCheckbox);
            y += 2;

            // Enable Port Forwarding
            var portFwdCheckbox = new CheckBox("Enable Port Forwarding") { X = 1, Y = y, Checked = Settings.EnablePortForwarding };
            panel.Add(portFwdCheckbox);
            y += 2;

            // Default Download Path
            panel.Add(new Label("Default Download Path:") { X = 1, Y = y });
            var downloadPathField = new TextField(Settings.DefaultDownloadPath ?? "") { X = 25, Y = y, Width = 40 };
            panel.Add(downloadPathField);
            y += 2;

            // Log Path
            panel.Add(new Label("Log Path:") { X = 1, Y = y });
            var logPathField = new TextField(Settings.LogPath ?? "") { X = 25, Y = y, Width = 40 };
            panel.Add(logPathField);
            y += 2;

            // Settings Path
            panel.Add(new Label("Settings Path:") { X = 1, Y = y });
            var settingsPathField = new TextField(Settings.SettingsPath ?? "") { X = 25, Y = y, Width = 40 };
            panel.Add(settingsPathField);
            y += 2;


            // Save Button
            var saveBtn = new Button("Save")
            {
                X = 1,
                Y = y
            };


            panel.Add(saveBtn);




            #endregion



            #region EVENT HANDLERS




            saveBtn.Clicked += () =>
            {
                // Parse numeric fields safely
                if (ushort.TryParse(portField.Text.ToString(), out var port)) Settings.DhtPort = port;
                if (ushort.TryParse(maxConnField.Text.ToString(), out var maxConn)) Settings.MaxConnections = maxConn;
                if (ushort.TryParse(maxSeedField.Text.ToString(), out var maxSeed)) Settings.MaxSeedersPerTorrent = maxSeed;
                if (ushort.TryParse(maxLeechField.Text.ToString(), out var maxLeech)) Settings.MaxLeechersPerTorrent = maxLeech;
                if (int.TryParse(maxDlField.Text.ToString(), out var maxDl)) Settings.MaxDownloadSpeed = maxDl;
                if (int.TryParse(maxUpField.Text.ToString(), out var maxUp)) Settings.MaxUploadSpeed = maxUp;

                // Boolean fields
                Settings.EnableDht = dhtCheckbox.Checked;
                Settings.StopSeedingWhenFinished = stopSeedCheckbox.Checked;
                Settings.EnablePortForwarding = portFwdCheckbox.Checked;

                // String paths
                Settings.DefaultDownloadPath = downloadPathField.Text.ToString();
                Settings.LogPath = logPathField.Text.ToString();
                Settings.SettingsPath = settingsPathField.Text.ToString();

                MessageBox.Query("Settings", "Settings saved.", "OK");

                Settings.SaveSettings();

            };


            #endregion

            return panel;
        }

        // --------------------- SEARCH PANEL -----------------------------------
        private FrameView BuildSearchView()
        {
            var panel = new FrameView("Search")
            {
                X = 20,
                Y = 3,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };

            var searchBar = new TextField("")
            {
                X = 1,
                Y = 1,
                Width = 40
            };

            var searchBtn = new Button("Search")
            {
                X = Pos.Right(searchBar) + 2,
                Y = 1
            };

            var results = new ListView(new List<string>())
            {
                X = 1,
                Y = 3,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };

            searchBtn.Clicked += () =>
            {
                // Example placeholder
                results.SetSource(new List<string>()
                {
                $"Result: {searchBar.Text}",
                "Result B",
                "Result C"
                });
            };

            panel.Add(searchBar, searchBtn, results);
            return panel;
        }
    }


}