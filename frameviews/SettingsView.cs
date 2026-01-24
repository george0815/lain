using System;
using System.Drawing;
using Terminal.Gui;
using lain.helpers;

namespace lain.frameviews
{
    /// <summary>
    /// SettingsView renders and manages the full application settings UI.
    ///
    /// This view is intentionally self-contained and explicit:
    ///  - UI construction
    ///  - input validation
    ///  - application of settings
    ///  - persistence to disk
    ///
    /// Abstraction is deliberately kept minimal to favor debuggability
    /// and predictable behavior in a terminal UI environment.
    /// </summary>
    internal class SettingsView : FrameView
    {
        /// <summary>
        /// Human-readable color names mapped to Terminal.Gui colors.
        ///
        /// Keys come from localized resource strings and are used
        /// as button labels in the color picker UI.
        /// </summary>
        internal static Dictionary<string, Terminal.Gui.Color> colors =
            new Dictionary<string, Terminal.Gui.Color>
            {
                { Resources.Black,          Terminal.Gui.Color.Black },
                { Resources.Blue,           Terminal.Gui.Color.Blue },
                { Resources.Green,          Terminal.Gui.Color.Green },
                { Resources.Cyan,           Terminal.Gui.Color.Cyan },
                { Resources.Red,            Terminal.Gui.Color.Red },
                { Resources.Magenta,        Terminal.Gui.Color.Magenta },
                { Resources.Brown,          Terminal.Gui.Color.Brown },
                { Resources.Gray,           Terminal.Gui.Color.Gray },
                { Resources.DarkGray,       Terminal.Gui.Color.DarkGray },
                { Resources.BrightBlue,     Terminal.Gui.Color.BrightBlue },
                { Resources.BrightGreen,    Terminal.Gui.Color.BrightGreen },
                { Resources.BrightCyan,     Terminal.Gui.Color.BrightCyan },
                { Resources.BrightRed,      Terminal.Gui.Color.BrightRed },
                { Resources.BrightMagenta,  Terminal.Gui.Color.BrightMagenta },
                { Resources.BrightYellow,   Terminal.Gui.Color.BrightYellow },
                { Resources.White,          Terminal.Gui.Color.White }
            };

        /// <summary>
        /// Initializes the settings UI and wires all controls
        /// directly to <see cref="Settings.Current"/>.
        ///
        /// Layout is built procedurally to keep ordering explicit
        /// and easy to modify without hidden layout logic.
        /// </summary>
        public SettingsView()
            : base(Resources.Settings)
        {
            X = 20;
            Y = SettingsData.HeaderHeight;
            Width = Dim.Fill();
            Height = Dim.Fill();

            // ScrollView allows all settings to remain accessible
            // even on small or resized terminal windows.
            var scroll = new ScrollView()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                ShowVerticalScrollIndicator = true,
                ShowHorizontalScrollIndicator = false,
            };

            Add(scroll);

            // Tracks vertical layout position inside the scroll view.
            // This avoids implicit layout rules and keeps spacing obvious.
            int y = 1;

            #region PORTS AND LIMITS

            // Primary listening port
            scroll.Add(new Label(Resources.Port) { X = 1, Y = y });
            var portField = new TextField(Settings.Current.Port.ToString())
            {
                X = (Thread.CurrentThread.CurrentUICulture.Name == "ja-JP" ? 32 : 35),
                Y = y,
                Width = 10
            };
            scroll.Add(portField);
            y += 2;

            // DHT port
            scroll.Add(new Label(Resources.DHTport) { X = 1, Y = y });
            var dhtPortField = new TextField(Settings.Current.DhtPort.ToString())
            {
                X = (Thread.CurrentThread.CurrentUICulture.Name == "ja-JP" ? 32 : 35),
                Y = y,
                Width = 10
            };
            scroll.Add(dhtPortField);
            y += 2;

            // Global connection limit
            scroll.Add(new Label(Resources.Maxtotalconnections) { X = 1, Y = y });
            var maxConnField = new TextField(Settings.Current.MaxConnections.ToString())
            {
                X = (Thread.CurrentThread.CurrentUICulture.Name == "ja-JP" ? 32 : 35),
                Y = y,
                Width = 10
            };
            scroll.Add(maxConnField);
            y += 2;

            // Download rate limit (displayed to user as MB/s)
            scroll.Add(new Label(Resources.Maxtotaldownloadspeed_MB_s_) { X = 1, Y = y });
            var maxDlField = new TextField(
                (Settings.Current.MaxDownloadSpeed / (1024 * 1024)).ToString())
            {
                X = (Thread.CurrentThread.CurrentUICulture.Name == "ja-JP" ? 32 : 35),
                Y = y,
                Width = 10
            };
            scroll.Add(maxDlField);
            y += 2;

            // Upload rate limit (displayed to user as MB/s)
            scroll.Add(new Label(Resources.Maxtotaluploadspeed_MB_s_) { X = 1, Y = y });
            var maxUpField = new TextField(
                (Settings.Current.MaxUploadSpeed / (1024 * 1024)).ToString())
            {
                X = (Thread.CurrentThread.CurrentUICulture.Name == "ja-JP" ? 32 : 35),
                Y = y,
                Width = 10
            };
            scroll.Add(maxUpField);
            y += 2;

            // UI refresh interval for progress updates
            scroll.Add(new Label(Resources.Progressrefreshrate_ms_) { X = 1, Y = y });
            var refreshRateField = new TextField(Settings.Current.RefreshInterval.ToString())
            {
                X = (Thread.CurrentThread.CurrentUICulture.Name == "ja-JP" ? 32 : 35),
                Y = y,
                Width = 10
            };
            scroll.Add(refreshRateField);
            y += 2;

            #endregion

            #region BOOLEAN OPTIONS

            // Stop seeding automatically once a torrent finishes
            var stopSeedCheckbox = new CheckBox(Resources.StopSeedingWhenFinished)
            {
                X = 1,
                Y = y,
                Checked = Settings.Current.StopSeedingWhenFinished
            };
            scroll.Add(stopSeedCheckbox);
            y += 2;

            // Disable colored hotkey hints in the UI
            var disableHotKeyColors = new CheckBox(Resources.Disablecoloredhotkeyinformation)
            {
                X = 1,
                Y = y,
                Checked = Settings.Current.DisableColoredHotkeyInfo
            };
            scroll.Add(disableHotKeyColors);
            y += 2;

            // Enable verbose logging for debugging and diagnostics
            var detailedLogging = new CheckBox(Resources.Enabledetailedlogging)
            {
                X = 1,
                Y = y,
                Checked = Settings.Current.DetailedLogging
            };
            scroll.Add(detailedLogging);
            y += 2;

            // Attempt automatic port forwarding via NAT traversal
            var portFwdCheckbox = new CheckBox(Resources.EnablePortForwarding)
            {
                X = 1,
                Y = y,
                Checked = Settings.Current.EnablePortForwarding
            };
            scroll.Add(portFwdCheckbox);
            y += 2;

            // Hide the terminal text cursor
            var hideTextCursor = new CheckBox(Resources.Hidetextcursor)
            {
                X = 1,
                Y = y,
                Checked = Settings.Current.HidetextCursor
            };
            scroll.Add(hideTextCursor);
            y += 2;

            #endregion


            #region PATHS

            // Default download directory
            scroll.Add(new Label(Resources.DefaultDownloadPath) { X = 1, Y = y });
            var downloadPathField =
                new TextField(Settings.Current.DefaultDownloadPath ?? "")
                {
                    X = 30,
                    Y = y,
                    Width = 40
                };
            scroll.Add(downloadPathField);
            var downloadFolderDialogBtn = new Button("...") { X = 71, Y = y };
            scroll.Add(downloadFolderDialogBtn);
            y += 2;

            // Log file location
            scroll.Add(new Label(Resources.LogPath) { X = 1, Y = y });
            var logPathField =
                new TextField(Settings.Current.LogPath ?? "")
                {
                    X = 30,
                    Y = y,
                    Width = 40
                };
            scroll.Add(logPathField);
            var logFileDialogBtn = new Button("...") { X = 71, Y = y };
            scroll.Add(logFileDialogBtn);
            y += 2;

            // Settings file location
            scroll.Add(new Label(Resources.SettingsPath) { X = 1, Y = y });
            var settingsPathField =
                new TextField(Settings.Current.SettingsPath ?? "")
                {
                    X = 30,
                    Y = y,
                    Width = 40
                };
            scroll.Add(settingsPathField);
            var settingsFileDialogBtn = new Button("...") { X = 71, Y = y };
            scroll.Add(settingsFileDialogBtn);
            y += 3;

            #endregion

            #region COLOR SETTINGS

            // Background color
            scroll.Add(new Label(Resources.Backgroundcolor) { X = 1, Y = y });
            var myKey =
                colors.FirstOrDefault(x => x.Value == Settings.Current.BackgroundColor).Key;
            var bgColorCombo = new Button()
            {
                X = 30,
                Y = y,
                Width = 3,
                Height = 1,
                Text = myKey
            };

            bgColorCombo.Clicked += () =>
            {
                // Pick color from grid dialog
                string tmp = DialogHelpers.PickColorGrid();
                Settings.Current.BackgroundColor = colors[tmp];

                // Update button label to reflect selection
                bgColorCombo.Text = tmp;

                // Force redraw
                bgColorCombo.SetNeedsDisplay();
                scroll.SetNeedsDisplay();
            };

            scroll.Add(bgColorCombo);
            y += 2;

            // Text color
            scroll.Add(new Label(Resources.Textcolor) { X = 1, Y = y });
            myKey = colors.FirstOrDefault(x => x.Value == Settings.Current.TextColor).Key;
            var textColorCombo = new Button()
            {
                X = 30,
                Y = y,
                Width = 3,
                Height = 1,
                Text = myKey
            };

            textColorCombo.Clicked += () =>
            {
                string tmp = DialogHelpers.PickColorGrid();
                Settings.Current.TextColor = colors[tmp];
                textColorCombo.Text = tmp;
                textColorCombo.SetNeedsDisplay();
                scroll.SetNeedsDisplay();
            };

            scroll.Add(textColorCombo);
            y += 2;

            // Focus background color
            scroll.Add(new Label(Resources.Focusbackgroundcolor) { X = 1, Y = y });
            myKey =
                colors.FirstOrDefault(
                    x => x.Value == Settings.Current.FocusBackgroundColor).Key;
            var backgroundFocusColorCombo = new Button()
            {
                X = 30,
                Y = y,
                Width = 3,
                Height = 1,
                Text = myKey
            };

            backgroundFocusColorCombo.Clicked += () =>
            {
                string tmp = DialogHelpers.PickColorGrid();
                Settings.Current.FocusBackgroundColor = colors[tmp];
                backgroundFocusColorCombo.Text = tmp;
                backgroundFocusColorCombo.SetNeedsDisplay();
                scroll.SetNeedsDisplay();
            };

            scroll.Add(backgroundFocusColorCombo);
            y += 2;

            // Focus text color
            scroll.Add(new Label(Resources.Focustextcolor) { X = 1, Y = y });
            myKey =
                colors.FirstOrDefault(
                    x => x.Value == Settings.Current.FocusTextColor).Key;
            var textFocusColorCombo = new Button()
            {
                X = 30,
                Y = y,
                Width = 3,
                Height = 1,
                Text = myKey
            };

            textFocusColorCombo.Clicked += () =>
            {
                string tmp = DialogHelpers.PickColorGrid();
                Settings.Current.FocusTextColor = colors[tmp];
                textFocusColorCombo.Text = tmp;
                textFocusColorCombo.SetNeedsDisplay();
                scroll.SetNeedsDisplay();
            };

            scroll.Add(textFocusColorCombo);
            y += 2;

            // Hotkey / accent color
            scroll.Add(new Label(Resources.Hotkeytextcolor) { X = 1, Y = y });
            myKey =
                colors.FirstOrDefault(
                    x => x.Value == Settings.Current.HotTextColor).Key;
            var hotTextColorCombo = new Button()
            {
                X = 30,
                Y = y,
                Width = 3,
                Height = 1,
                Text = myKey
            };

            hotTextColorCombo.Clicked += () =>
            {
                string tmp = DialogHelpers.PickColorGrid();
                Settings.Current.HotTextColor = colors[tmp];
                hotTextColorCombo.Text = tmp;
                hotTextColorCombo.SetNeedsDisplay();
                scroll.SetNeedsDisplay();
            };

            scroll.Add(hotTextColorCombo);
            y += 2;

            // ASCII logo color
            scroll.Add(new Label(Resources.ASCIIcolor) { X = 1, Y = y });
            myKey =
                colors.FirstOrDefault(
                    x => x.Value == Settings.Current.LogoColor).Key;
            var logoColorCombo = new Button()
            {
                X = 30,
                Y = y,
                Width = 3,
                Height = 1,
                Text = myKey
            };

            logoColorCombo.Clicked += () =>
            {
                string tmp = DialogHelpers.PickColorGrid();
                Settings.Current.LogoColor = colors[tmp];
                logoColorCombo.Text = tmp;
                logoColorCombo.SetNeedsDisplay();
                scroll.SetNeedsDisplay();
            };

            scroll.Add(logoColorCombo);
            y += 2;

            // Toggle ASCII logo entirely
            var disableASCII =
                new CheckBox(Resources.DisableASCII)
                {
                    X = 1,
                    Y = y,
                    Checked = Settings.Current.DisableASCII
                };
            scroll.Add(disableASCII);
            y += 2;

            #endregion

            // Persist settings to disk
            var saveBtn = new Button(Resources.Save) { X = 1, Y = y };
            scroll.Add(saveBtn);

            // Required so scroll bars calculate correctly
            scroll.ContentSize =
                new Terminal.Gui.Size(Application.Top.Frame.Width - 2, y + 2);

            #region EVENT HANDLERS

            logFileDialogBtn.Clicked += () =>
            {
                string? path =
                    DialogHelpers.ShowSaveFileDialog(
                        Resources.Selectlogfilepath,
                        Resources.Selectfilenameforthelogfile,
                        [".txt"]);

                if (!string.IsNullOrWhiteSpace(path))
                {
                    logPathField.Text = path;
                }
            };

            settingsFileDialogBtn.Clicked += () =>
            {
                string? path =
                    DialogHelpers.ShowSaveFileDialog(
                        Resources.Selectconfigfilepath,
                        Resources.Selectfilenamefortheconfigfile,
                        [".json"]);

                if (!string.IsNullOrWhiteSpace(path))
                {
                    settingsPathField.Text = path;
                }
            };

            downloadFolderDialogBtn.Clicked += () =>
            {
                string? path =
                    DialogHelpers.ShowSaveFileDialog(
                        Resources.Selectdefaultdownloadfolder,
                        Resources.Selectthedefaultdownloadfolderpath,
                        [""]);

                if (!string.IsNullOrWhiteSpace(path))
                {
                    // Extract directory from returned path
                    string dir = Path.GetDirectoryName(path) ?? path;
                    downloadPathField.Text = path;
                }
            };

            saveBtn.Clicked += () =>
            {
                try
                {
                    // --- numeric validation ---

                    if (!ushort.TryParse(portField.Text.ToString(), out var port))
                    {
                        MessageBox.ErrorQuery(Resources.Error, Resources.Invalidportnumber, Resources.OK);
                        return;
                    }

                    if (!ushort.TryParse(dhtPortField.Text.ToString(), out var dhtPort))
                    {
                        MessageBox.ErrorQuery(Resources.Error, Resources.InvalidDHTportnumber, Resources.OK);
                        return;
                    }

                    if (!ushort.TryParse(maxConnField.Text.ToString(), out var maxConn)
                        || maxConn > 50000 || maxConn < 0)
                    {
                        MessageBox.ErrorQuery(Resources.Error, Resources.Invalidmaximumconnectionsvalue, Resources.OK);
                        return;
                    }

                    if (!int.TryParse(maxDlField.Text.ToString(), out var maxDl)
                        || maxDl > (Int32.MaxValue / 1048576))
                    {
                        MessageBox.ErrorQuery(Resources.Error, Resources.Invalidmaxdownloadspeed, Resources.OK);
                        return;
                    }

                    if (!int.TryParse(maxUpField.Text.ToString(), out var maxUp)
                        || maxUp > (Int32.MaxValue / 1048576))
                    {
                        MessageBox.ErrorQuery(Resources.Error, Resources.Invalidmaxuploadspeed, Resources.OK);
                        return;
                    }

                    if (!int.TryParse(refreshRateField.Text.ToString(), out var refRate)
                        || refRate > 50000 || refRate < 0)
                    {
                        MessageBox.ErrorQuery(Resources.Error, Resources.Invalidprogressrefreshrate, Resources.OK);
                        return;
                    }

                    // --- path validation ---

                    string downloadPath = downloadPathField.Text.ToString()!.Trim();
                    string logPath = logPathField.Text.ToString()!.Trim();
                    string settingsPath = settingsPathField.Text.ToString()!.Trim();

                    if (string.IsNullOrWhiteSpace(downloadPath))
                    {
                        MessageBox.ErrorQuery(Resources.Error, Resources.Downloadpathcannotbeempty, Resources.OK);
                        return;
                    }

                    if (!Directory.Exists(downloadPath))
                    {
                        if (MessageBox.Query(
                            Resources.MissingDirectory,
                            Resources.DownloadpathdoesnotexistCreateit_,
                            Resources.Yes, Resources.No) == 0)
                        {
                            Directory.CreateDirectory(downloadPath);
                        }
                        else return;
                    }

                    if (string.IsNullOrWhiteSpace(logPath))
                    {
                        MessageBox.ErrorQuery(Resources.Error, Resources.Logpathcannotbeempty, Resources.OK);
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(settingsPath))
                    {
                        MessageBox.ErrorQuery(Resources.Error, Resources.Settingspathcannotbeempty, Resources.OK);
                        return;
                    }

                    // --- apply settings ---

                    Settings.Current.Port = port;
                    Settings.Current.DhtPort = dhtPort;
                    Settings.Current.MaxConnections = maxConn;
                    Settings.Current.MaxDownloadSpeed = maxDl * 1024 * 1024;
                    Settings.Current.MaxUploadSpeed = maxUp * 1024 * 1024;
                    Settings.Current.RefreshInterval = refRate;

                    Settings.Current.StopSeedingWhenFinished = stopSeedCheckbox.Checked;
                    Settings.Current.EnablePortForwarding = portFwdCheckbox.Checked;
                    Settings.Current.DetailedLogging = detailedLogging.Checked;
                    Settings.Current.DisableASCII = disableASCII.Checked;
                    Settings.Current.DisableColoredHotkeyInfo = disableHotKeyColors.Checked;
                    Settings.Current.HidetextCursor = hideTextCursor.Checked;

                    Settings.Current.DefaultDownloadPath = downloadPath;
                    Settings.Current.LogPath = logPath;
                    Settings.Current.SettingsPath = settingsPath;

                    Settings.Current.BackgroundColor = colors[bgColorCombo.Text.ToString()!];
                    Settings.Current.TextColor = colors[textColorCombo.Text.ToString()!];
                    Settings.Current.FocusBackgroundColor = colors[backgroundFocusColorCombo.Text.ToString()!];
                    Settings.Current.FocusTextColor = colors[textFocusColorCombo.Text.ToString()!];
                    Settings.Current.HotTextColor = colors[hotTextColorCombo.Text.ToString()!];
                    Settings.Current.LogoColor = colors[logoColorCombo.Text.ToString()!];

                    Settings.Save();

                    MessageBox.Query(Resources.Settings, Resources.Settingssavedsuccessfully, Resources.OK);
                    Log.Write(Resources.Settingssaved);
                }
                catch (Exception ex)
                {
                    MessageBox.ErrorQuery(Resources.FatalError, ex.Message, Resources.OK);
                }
            };

            #endregion
        }
    }
}
