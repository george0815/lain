using System;
using System.Collections.Generic;
using Terminal.Gui;
using lain.frameviews;

namespace lain
{
    public class LainUI : Window
    {
        

        // Content views
        FrameView torrentListView;
        FrameView downloadView;
        FrameView createView;
        FrameView settingsView;
        FrameView searchView;
        internal FrameView logView;

        public LainUI()
        {
            //Title = "Torrent Client - Terminal.Gui";
            List<string> ActiveTorrents = new List<string>()
             {
                "Ubuntu ISO",
                "Arch Linux ISO",
                "Fedora ISO"
            };

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
            "Torrents",
            "Download",
            "Create",
            "Settings",
            "Search",
            "Log",
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
            torrentListView = new TorrentListView();
            downloadView = new DownloadView();
            createView = new CreateView();
            settingsView = new SettingsView();
            searchView = new SearchView();
            logView = new LogView(Log.log);

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
            Remove(logView);

            switch (index)
            {
                case 0: Add(torrentListView); break;
                case 1: Add(downloadView); break;
                case 2: Add(createView); break;
                case 3: Add(settingsView); break;
                case 4: Add(searchView); break;
                case 5:
                  
                    Add(logView); break;
                case 6: Application.RequestStop(); break;
            }

            SetNeedsDisplay();
        }


        

       
       
    }


}