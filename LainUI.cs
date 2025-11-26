using lain.frameviews;
using lain.helpers;
using MonoTorrent.Client;
using System;
using System.Collections.Generic;
using System.Text;
using Terminal.Gui;

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


        public bool ShouldExit { get; private set; }




        public LainUI()
        {

            #region HEADER

            //Header
            var header = new FrameView()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Settings.Current.DisableASCII ? 5 : SettingsData.HeaderHeight,
                CanFocus = false,
                Border = new Border() { BorderStyle = BorderStyle.None }
            };

            // Wrap ASCII art in a FrameView to give it a border
            var logoFrame = new FrameView()
            {
                X = 0,
                Y = 0,
                Width = Settings.Current.DisableASCII ? 0 : SettingsData.LogoWidth, // fixed width for logo
                Height = SettingsData.HeaderHeight,
                Border = new Border() { BorderStyle = BorderStyle.Single }
            };

            var logo = new Label()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                Text = Settings.Current.icons[0],

                ColorScheme = new ColorScheme()
                {
                    Normal = Application.Driver.MakeAttribute(Settings.Current.LogoColor, Settings.Current.BackgroundColor), // text color, background color
                    Focus = Application.Driver.MakeAttribute(Settings.Current.LogoColor, Settings.Current.BackgroundColor)
                }
            };

            if (!Settings.Current.DisableASCII)
            {
                logoFrame.Add(logo);
                header.Add(logoFrame);
            }


            #region EXTRA HEADER INFO

            // Date
            var date = new Label()
            {
                X = (Settings.Current.DisableASCII ? 0 : SettingsData.LogoWidth) + 2,
                Y = 1,
                Text = DateTime.Now.ToString("yyyy-MM-dd")
            };

            // Active torrents count
            var torrentCount = new Label()
            {
                X = (Settings.Current.DisableASCII ? 0 : SettingsData.LogoWidth) + 2,
                Y = 3,
                Text = $"Active Torrents: {TorrentOperations.Managers!.Count}"
            };

            // Active torrents preview
            StringBuilder sb = new StringBuilder("", 30);
            int count = 0;

            if (TorrentOperations.Managers.Count() != 0)
            {
                for (int i = 0; i <= TorrentOperations.Managers.Count(); i++)
                {

                    if (TorrentOperations.Managers[i].State == TorrentState.Seeding || TorrentOperations.Managers[i].State == TorrentState.Downloading) { sb.Append(TorrentOperations.Managers[i].Torrent?.Name); }
                    if (count == 3) { sb.Append("..."); break; }
                    sb.Append("\n");

                }
            }
            else { sb.Append("No active torrents"); }

            var torrentPreview = new Label()
                {
                    X = (Settings.Current.DisableASCII ? 0 : SettingsData.LogoWidth) + 2,
                    Y = 5,
                    Text = sb.ToString()
                };

            // Port
            var portDisplay = new Label()
            {
                X = (Settings.Current.DisableASCII ? 0 : SettingsData.LogoWidth) + 2,
                Y = 7,
                Text = $"Operating on port: {Settings.Current.Port}"
            };


            #region HOTKEY INFO


            header.Add(new Label($"Start: {Settings.Current.Controls.StartDownload}") { X = (Settings.Current.DisableASCII ? 0 : SettingsData.LogoWidth) + 30, Y = 1 });
            header.Add(new Label($"Stop: {Settings.Current.Controls.StartDownload}") { X = (Settings.Current.DisableASCII ? 0 : SettingsData.LogoWidth) + 30, Y = 3 });
            header.Add(new Label($"Start seeding: {Settings.Current.Controls.StartDownload}") { X = (Settings.Current.DisableASCII ? 0 : SettingsData.LogoWidth) + 30, Y = 5 });
            header.Add(new Label($"Stop seeding: {Settings.Current.Controls.StartDownload}") { X = (Settings.Current.DisableASCII ? 0 : SettingsData.LogoWidth) + 30, Y = 7 });
            header.Add(new Label($"Delete: {Settings.Current.Controls.StartDownload}") { X = (Settings.Current.DisableASCII ? 0 : SettingsData.LogoWidth) + 30, Y = 9 });




            #endregion


            header.Add(date, torrentCount, torrentPreview, portDisplay);
            Add(header);




            #endregion





            #endregion

            #region SIDEBAR AND MENU

            //Sidebar
            var sidebar = new FrameView()
            {
                X = 0,
                Y = SettingsData.HeaderHeight,
                Width = 20,
                Height = Dim.Fill(),
                Border = new Border() { BorderStyle = BorderStyle.Rounded }
            };

            //Menu
            var menu = new ListView(new string[]
            {
                "Torrents",
                "Download",
                "Create",
                "Settings",
                "Search",
                "Log"
            })
            {
                X = 1,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill() - 3
            };

            menu.SelectedItemChanged += (args) =>
            {
                SwitchPanel(args.Item, ref logo);
            };

            sidebar.Add(menu);

            #endregion



            //Exit
            var exitButton = new Button("Exit")
            {
                X = 1,
                Y = Pos.Bottom(menu),
                Width = 16,
            };

            exitButton.Clicked += () => ShowExitDialog();

            sidebar.Add(exitButton);

            // Add sidebar to window
            Add(sidebar);


            //Content
            torrentListView = new TorrentListView(TorrentOperations.Managers);
            downloadView = new DownloadView();
            createView = new CreateView();
            settingsView = new SettingsView();
            searchView = new SearchView();
            logView = new LogView(Log.log);

            // Default panel
            Add(torrentListView);
        }


        #region HELPER METHODS



        // Shows exit confirmation dialog
        private void ShowExitDialog()
        {
            var dialog = new Dialog("Exit?", 50, 10);
            dialog.Height = 3;
            var ok = new Button("OK");
            var cancel = new Button("Cancel");

            bool exitRequested = false;

            // Wire handlers BEFORE showing the dialog
            ok.Clicked += () =>
            {
                exitRequested = true;               // remember choice
                Application.RequestStop(dialog);    // close the modal dialog
            };

            cancel.Clicked += () =>
            {
                Application.RequestStop(dialog);    // just close the modal dialog
            };

            dialog.AddButton(ok);
            dialog.AddButton(cancel);

            // Run the dialog modally. This call returns when the dialog is closed.
            Application.Run(dialog);

            // After the dialog is closed, react to the choice
            if (exitRequested)
            {
                // Optional: clear UI
                Application.Top.RemoveAll();

                // Stop main UI loop (if running)
                Application.RequestStop();


            }
        }


        // Switches visible panel
        private void SwitchPanel(int index, ref Label logo)
        {
            Remove(torrentListView);
            Remove(downloadView);
            Remove(createView);
            Remove(settingsView);
            Remove(searchView);
            Remove(logView);

            logo.Text = ASCII.icons[index];

            switch (index)
            {
                case 0: Add(torrentListView); ((TorrentListView)torrentListView).Refresh(); break;
                case 1: Add(downloadView); break;
                case 2: Add(createView); break;
                case 3: Add(settingsView); break;
                case 4: Add(searchView); break;
                case 5: Add(logView); break;
            }

            SetNeedsDisplay();
        }

        #endregion






    }


}