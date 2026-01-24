using lain.frameviews;
using lain.helpers;
using MonoTorrent.Client;
using System;
using System.Collections.Generic;
using System.Text;
using Terminal.Gui;

namespace lain
{
    /// <summary>
    /// Main UI window for Lain application using Terminal.Gui.
    /// Contains header, sidebar menu, and various content panels.
    /// </summary>
    public class LainUI : Window
    {
        // ------------------------------
        // Content views
        // ------------------------------
        readonly FrameView torrentListView;
        readonly FrameView downloadView;
        readonly FrameView createView;
        readonly FrameView settingsView;
        readonly FrameView searchView;
        readonly FrameView pluginsView;
        internal FrameView logView;

        // Header components
        readonly private FrameView header;
        readonly private Label torrentCount;

        public LainUI()
        {
            #region HEADER

            // Header container
            header = new FrameView()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Settings.Current.DisableASCII ? 5 : SettingsData.HeaderHeight,
                Border = new Border() { BorderStyle = BorderStyle.None }
            };

            // Scrollable area inside header
            var headerScroll = new ScrollView()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                ContentSize = new Size(120, SettingsData.HeaderHeight),
                ShowHorizontalScrollIndicator = false,
                ShowVerticalScrollIndicator = false,
                Border = new Border() { BorderStyle = BorderStyle.None }
            };

            header.Add(headerScroll);

            // ASCII logo frame
            var logoFrame = new FrameView()
            {
                X = 0,
                Y = 0,
                Width = Settings.Current.DisableASCII ? 0 : SettingsData.LogoWidth,
                Height = SettingsData.HeaderHeight,
                Border = new Border() { BorderStyle = BorderStyle.Single }
            };

            var logo = new Label()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                Text = Settings.Current.Icons[0],
                ColorScheme = new ColorScheme()
                {
                    Normal = Application.Driver.MakeAttribute(Settings.Current.LogoColor, Settings.Current.BackgroundColor),
                    Focus = Application.Driver.MakeAttribute(Settings.Current.LogoColor, Settings.Current.BackgroundColor)
                }
            };

            // Add logo only if ASCII art is enabled
            if (!Settings.Current.DisableASCII)
            {
                logoFrame.Add(logo);
                header.Add(logoFrame);
            }

            #region EXTRA HEADER INFO

            // Display current date
            var date = new Label()
            {
                X = (Settings.Current.DisableASCII ? 0 : SettingsData.LogoWidth) + 2,
                Y = 1,
                Text = DateTime.Now.ToString("yyyy-MM-dd")
            };

            // Display number of active torrents
            torrentCount = new Label()
            {
                X = (Settings.Current.DisableASCII ? 30 : SettingsData.LogoWidth) + 2,
                Y = (Settings.Current.DisableASCII ? 1 : 3),
                Text = $"{Resources.ActiveTorrents}{TorrentOperations.Managers!.Count}"
            };

            // Display port
            var portDisplay = new Label()
            {
                X = (Settings.Current.DisableASCII ? 0 : SettingsData.LogoWidth) + 2,
                Y = (Settings.Current.DisableASCII ? 3 : 5),
                Text = $"{Resources.Operatingonport}{Settings.Current.Port}"
            };

            #region HOTKEY INFO

            // Add hotkey instructions with colored text
            headerScroll.Add(new Label($"{Resources.Start}{Settings.Current.Controls.StartDownload}")
            {
                ColorScheme = (Settings.Current.DisableColoredHotkeyInfo ? this.SuperView?.ColorScheme : new ColorScheme()
                {
                    Normal = Application.Driver.MakeAttribute(Color.Green, Settings.Current.BackgroundColor)
                }),
                X = (Settings.Current.DisableASCII ? 30 : SettingsData.LogoWidth) + 30,
                Y = 1
            });

            headerScroll.Add(new Label($"{Resources.Stop}{Settings.Current.Controls.StopDownload}")
            {
                ColorScheme = (Settings.Current.DisableColoredHotkeyInfo ? this.SuperView?.ColorScheme : new ColorScheme()
                {
                    Normal = Application.Driver.MakeAttribute(Color.Red, Settings.Current.BackgroundColor)
                }),
                X = (Settings.Current.DisableASCII ? 30 : SettingsData.LogoWidth) + 30,
                Y = 3
            });

            headerScroll.Add(new Label($"{Resources.Startseeding}{Settings.Current.Controls.StartSeeding}")
            {
                ColorScheme = (Settings.Current.DisableColoredHotkeyInfo ? this.SuperView?.ColorScheme : new ColorScheme()
                {
                    Normal = Application.Driver.MakeAttribute(Color.BrightYellow, Settings.Current.BackgroundColor)
                }),
                X = (Settings.Current.DisableASCII ? 42 : SettingsData.LogoWidth) + 30,
                Y = (Settings.Current.DisableASCII ? 1 : 5)
            });

            headerScroll.Add(new Label($"{Resources.Stopseeding}{Settings.Current.Controls.StopSeeding}")
            {
                ColorScheme = (Settings.Current.DisableColoredHotkeyInfo ? this.SuperView?.ColorScheme : new ColorScheme()
                {
                    Normal = Application.Driver.MakeAttribute(Color.Blue, Settings.Current.BackgroundColor)
                }),
                X = (Settings.Current.DisableASCII ? 42 : SettingsData.LogoWidth) + 30,
                Y = (Settings.Current.DisableASCII ? 3 : 7)
            });

            headerScroll.Add(new Label($"{Resources.Delete}{Settings.Current.Controls.RemoveTorrent}")
            {
                ColorScheme = (Settings.Current.DisableColoredHotkeyInfo ? this.SuperView?.ColorScheme : new ColorScheme()
                {
                    Normal = Application.Driver.MakeAttribute(Color.Magenta, Settings.Current.BackgroundColor)
                }),
                X = (Settings.Current.DisableASCII ? 62 : SettingsData.LogoWidth) + 30,
                Y = (Settings.Current.DisableASCII ? 1 : 9)
            });

            headerScroll.Add(new Label($"{Resources.GeneratemagnetlinkSettingsCurrentControlsGenMagLink}{Settings.Current.Controls.GenMagLink}")
            {
                ColorScheme = (Settings.Current.DisableColoredHotkeyInfo ? this.SuperView?.ColorScheme : new ColorScheme()
                {
                    Normal = Application.Driver.MakeAttribute(Color.BrightCyan, Settings.Current.BackgroundColor)
                }),
                X = (Settings.Current.DisableASCII ? 62 : SettingsData.LogoWidth) + 30,
                Y = (Settings.Current.DisableASCII ? 3 : 11)
            });

            #endregion

            // Add info labels to header scroll
            headerScroll.Add(date, torrentCount, portDisplay);

            // Add header to main window
            Add(header);

            #endregion
            #endregion

            #region SIDEBAR AND MENU

            // Sidebar container
            var sidebar = new FrameView()
            {
                X = 0,
                Y = SettingsData.HeaderHeight,
                Width = 20,
                Height = Dim.Fill(),
                Border = new Border() { BorderStyle = BorderStyle.Rounded }
            };

            // Sidebar menu
            var menu = new ListView(new string[]
            {
                Resources.Torrents,
                Resources.Download,
                Resources.Create,
                Resources.Ghidorah,
                Resources.Settings,
                Resources.Search,
                Resources.Log
            })
            {
                X = 1,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill() - 3
            };

            // Switch content panel when menu selection changes
            menu.SelectedItemChanged += (args) =>
            {
                SwitchPanel(args.Item, ref logo);
            };

            sidebar.Add(menu);

            // Exit button
            var exitButton = new Button(Resources.Exit)
            {
                X = 1,
                Y = Pos.Bottom(menu),
                Width = 16
            };

            exitButton.Clicked += () => ShowExitDialog();

            sidebar.Add(exitButton);

            // Add sidebar to window
            Add(sidebar);

            #endregion

            // Subscribe to torrent updates
            TorrentOperations.UpdateProgress += RefreshActiveTorrents;

            #region CONTENT VIEWS

            torrentListView = new TorrentListView(TorrentOperations.Managers);
            downloadView = new DownloadView();
            createView = new CreateView();
            settingsView = new SettingsView();
            pluginsView = new PluginView();
            searchView = new SearchView();
            logView = new LogView(Log.log);

            // Show default panel
            Add(torrentListView);

            #endregion
        }

        #region HELPER METHODS

        /// <summary>
        /// Refreshes the active torrent count in header.
        /// </summary>
        void RefreshActiveTorrents()
        {
            torrentCount.Text = $"{Resources.ActiveTorrents}{TorrentOperations.Managers!.Count}";
            torrentCount.SetNeedsDisplay();
            SetNeedsDisplay();
        }

        /// <summary>
        /// Shows exit confirmation dialog and handles application stop.
        /// </summary>
        private static void ShowExitDialog()
        {
            var dialog = new Dialog(Resources.Exit_, 50, 10) { Height = 3 };
            var ok = new Button(Resources.OK);
            var cancel = new Button(Resources.Cancel);

            bool exitRequested = false;

            ok.Clicked += () =>
            {
                exitRequested = true;
                Application.RequestStop(dialog);
            };

            cancel.Clicked += () =>
            {
                Application.RequestStop(dialog);
            };

            dialog.AddButton(ok);
            dialog.AddButton(cancel);

            // Run dialog modally
            Application.Run(dialog);

            // If confirmed, close main UI
            if (exitRequested)
            {
                Application.Top.RemoveAll();
                Application.RequestStop();
            }
        }

        /// <summary>
        /// Switches visible content panel based on menu index.
        /// </summary>
        private void SwitchPanel(int index, ref Label logo)
        {
            // Remove all panels first
            Remove(torrentListView);
            Remove(downloadView);
            Remove(createView);
            Remove(settingsView);
            Remove(searchView);
            Remove(logView);

            // Update ASCII logo
            logo.Text = ASCII.icons[index];

            switch (index)
            {
                case 0: Add(torrentListView); ((TorrentListView)torrentListView).Refresh(); break;
                case 1: Add(downloadView); break;
                case 2: Add(createView); break;
                case 3: Add(pluginsView); break;
                case 4: Add(settingsView); break;
                case 5: Add(searchView); break;
                case 6: Add(logView); break;
            }

            SetNeedsDisplay();
        }

        /// <summary>
        /// Controls cursor visibility based on settings.
        /// </summary>
        public override void PositionCursor()
        {
            if (Settings.Current.HidetextCursor)
            {
                Application.Driver.SetCursorVisibility(CursorVisibility.Invisible);
                return;
            }

            base.PositionCursor();
        }

        #endregion
    }
}
