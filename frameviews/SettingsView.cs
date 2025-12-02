using System;
using System.Drawing;
using Terminal.Gui;
using lain.helpers;

namespace lain.frameviews
{
    internal class SettingsView : FrameView
    {

        //Color options
        Dictionary<string, Terminal.Gui.Color> colors = new Dictionary<string, Terminal.Gui.Color>
        {
            { "Black", Terminal.Gui.Color.Black },
            { "Blue", Terminal.Gui.Color.Blue  },
            { "Green", Terminal.Gui.Color.Green },
            { "Cyan", Terminal.Gui.Color.Cyan },
            { "Red", Terminal.Gui.Color.Red },
            { "Magenta", Terminal.Gui.Color.Magenta },
            { "Brown", Terminal.Gui.Color.Brown   },
            { "Gray", Terminal.Gui.Color.Gray },
            { "DarkGray", Terminal.Gui.Color.DarkGray },
            { "BrightBlue", Terminal.Gui.Color.BrightBlue },
            { "BrightGreen", Terminal.Gui.Color.BrightGreen },
            { "BrightCyan", Terminal.Gui.Color.BrightCyan },
            { "BrightRed", Terminal.Gui.Color.BrightRed },
            { "BrightMagenta", Terminal.Gui.Color.BrightMagenta },
            { "BrightYellow", Terminal.Gui.Color.BrightYellow},
            { "White", Terminal.Gui.Color.White }
        };


        public SettingsView()
            : base("Settings")
        {
            X = 20;
            Y = SettingsData.HeaderHeight;
            Width = Dim.Fill();
            Height = Dim.Fill();

            // Create a scroll view
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

            int y = 1; // starting Y position inside scroll view

            #region PORTS AND LIMITS

            // Port
            scroll.Add(new Label("Port:") { X = 1, Y = y });
            var portField = new TextField(Settings.Current.Port.ToString()) { X = 35, Y = y, Width = 10 };
            scroll.Add(portField);
            y += 2;

            //DHT port
            scroll.Add(new Label("DHT port:") { X = 1, Y = y });
            var dhtPortField = new TextField(Settings.Current.DhtPort.ToString()) { X = 35, Y = y, Width = 10 };
            scroll.Add(dhtPortField);
            y += 2;

            // Max Connections
            scroll.Add(new Label("Max total connections:") { X = 1, Y = y });
            var maxConnField = new TextField(Settings.Current.MaxConnections.ToString()) { X = 35, Y = y, Width = 10 };
            scroll.Add(maxConnField);
            y += 2;

            // Max Download Speed
            scroll.Add(new Label("Max total download speed (kB/s):") { X = 1, Y = y });
            var maxDlField = new TextField((Settings.Current.MaxDownloadSpeed / 1024).ToString()) { X = 35, Y = y, Width = 10 };
            scroll.Add(maxDlField);
            y += 2;

            // Max Upload Speed
            scroll.Add(new Label("Max total upload speed (kB/s):") { X = 1, Y = y });
            var maxUpField = new TextField((Settings.Current.MaxUploadSpeed / 1024).ToString()) { X = 35, Y = y, Width = 10 };
            scroll.Add(maxUpField);
            y += 2;

            // Progress Refresh rate
            scroll.Add(new Label("Progress refresh rate (ms):") { X = 1, Y = y });
            var refreshRateField = new TextField(Settings.Current.MaxUploadSpeed.ToString()) { X = 35, Y = y, Width = 10 };
            scroll.Add(refreshRateField);
            y += 2;


            #endregion

            #region BOOLEAN OPTIONS

            var stopSeedCheckbox = new CheckBox("Stop Seeding When Finished") { X = 1, Y = y, Checked = Settings.Current.StopSeedingWhenFinished };
            scroll.Add(stopSeedCheckbox);
            y += 2;

            var disableHotKeyColors = new CheckBox("Disable colored hotkey information") { X = 1, Y = y, Checked = Settings.Current.DisableColoredHotkeyInfo };
            scroll.Add(disableHotKeyColors);
            y += 2;

            var detailedLogging = new CheckBox("Enable detailed logging") { X = 1, Y = y, Checked = Settings.Current.DetailedLogging };
            scroll.Add(detailedLogging);
            y += 2;

            var portFwdCheckbox = new CheckBox("Enable Port Forwarding") { X = 1, Y = y, Checked = Settings.Current.EnablePortForwarding };
            scroll.Add(portFwdCheckbox);
            y += 2;

            #endregion

            #region PATHS

            // Paths
            scroll.Add(new Label("Default Download Path:") { X = 1, Y = y });
            var downloadPathField = new TextField(Settings.Current.DefaultDownloadPath ?? "") { X = 30, Y = y, Width = 40 };
            scroll.Add(downloadPathField);
            var downloadFolderDialogBtn = new Button("...") { X = 71, Y = y };
            scroll.Add(downloadFolderDialogBtn);
            y += 2;

            scroll.Add(new Label("Log Path:") { X = 1, Y = y });
            var logPathField = new TextField(Settings.Current.LogPath ?? "") { X = 30, Y = y, Width = 40 };
            scroll.Add(logPathField);
            var logFileDialogBtn = new Button("...") { X = 71, Y = y };
            scroll.Add(logFileDialogBtn);
            y += 2;

            scroll.Add(new Label("Settings Path:") { X = 1, Y = y });
            var settingsPathField = new TextField(Settings.Current.SettingsPath ?? "") { X = 30, Y = y, Width = 40 };
            scroll.Add(settingsPathField);
            var settingsFileDialogBtn = new Button("...") { X = 71, Y = y };
            scroll.Add(settingsFileDialogBtn);
            y += 3;

            #endregion

            #region COLOR SETTINGS

            
            // Background Color
            scroll.Add(new Label("Background color:") { X = 1, Y = y });

            var bgColorCombo = new Button()
            {
                X = 30,
                Y = y,
                Width = 3,
        
                Height = 1,
                Text = Settings.Current.BackgroundColor.ToString()
            };
            

            bgColorCombo.Clicked += () =>
            {
                // Open your color picker
                Settings.Current.BackgroundColor = DialogHelpers.PickColorGrid();

                // Update *the button's* text, not `Text = ...`
                bgColorCombo.Text = Settings.Current.BackgroundColor.ToString();

                // Force redraw the button *and its parent view*
                bgColorCombo.SetNeedsDisplay();
                scroll.SetNeedsDisplay();

            };

            scroll.Add(bgColorCombo); 
            y += 2;

            // Text Color
            scroll.Add(new Label("Text color:") { X = 1, Y = y });

            var textColorCombo = new Button()
            {
                X = 30,
                Y = y,
                Width = 3,

                Height = 1,
                Text = Settings.Current.TextColor.ToString()
            };


            textColorCombo.Clicked += () =>
            {
                // Open your color picker
                Settings.Current.TextColor = DialogHelpers.PickColorGrid();

                // Update *the button's* text, not `Text = ...`
                textColorCombo.Text = Settings.Current.TextColor.ToString();

                // Force redraw the button *and its parent view*
                textColorCombo.SetNeedsDisplay();
                scroll.SetNeedsDisplay();

            };
            scroll.Add(textColorCombo);
            y += 2;



            //Focus background color
            scroll.Add(new Label("Focus background color:") { X = 1, Y = y });

            var backgroundFocusColorCombo = new Button()
            {
                X = 30,
                Y = y,
                Width = 3,

                Height = 1,
                Text = Settings.Current.FocusBackgroundColor.ToString()
            };


            backgroundFocusColorCombo.Clicked += () =>
            {
                // Open your color picker
                Settings.Current.FocusBackgroundColor = DialogHelpers.PickColorGrid();

                // Update *the button's* text, not `Text = ...`
                backgroundFocusColorCombo.Text = Settings.Current.FocusBackgroundColor.ToString();

                // Force redraw the button *and its parent view*
                backgroundFocusColorCombo.SetNeedsDisplay();
                scroll.SetNeedsDisplay();

            };
            scroll.Add(backgroundFocusColorCombo);
            y += 2;

            //Focus text color
            scroll.Add(new Label("Focus text color:") { X = 1, Y = y });

            var textFocusColorCombo = new Button()
            {
                X = 30,
                Y = y,
                Width = 3,

                Height = 1,
                Text = Settings.Current.FocusTextColor.ToString()
            };


            textFocusColorCombo.Clicked += () =>
            {
                // Open your color picker
                Settings.Current.FocusTextColor = DialogHelpers.PickColorGrid();

                // Update *the button's* text, not `Text = ...`
                textFocusColorCombo.Text = Settings.Current.FocusTextColor.ToString();

                // Force redraw the button *and its parent view*
                textFocusColorCombo.SetNeedsDisplay();
                scroll.SetNeedsDisplay();

            };
            scroll.Add(textFocusColorCombo);
            y += 2;


            //Hot text color
            scroll.Add(new Label("Hotkey text color:") { X = 1, Y = y });

            var hotTextColorCombo = new Button()
            {
                X = 30,
                Y = y,
                Width = 3,

                Height = 1,
                Text = Settings.Current.HotTextColor.ToString()
            };


            hotTextColorCombo.Clicked += () =>
            {
                // Open your color picker
                Settings.Current.HotTextColor = DialogHelpers.PickColorGrid();

                // Update *the button's* text, not `Text = ...`
                hotTextColorCombo.Text = Settings.Current.HotTextColor.ToString();

                // Force redraw the button *and its parent view*
                hotTextColorCombo.SetNeedsDisplay();
                scroll.SetNeedsDisplay();

            };
            scroll.Add(hotTextColorCombo);
            y += 2;

            //ASCII color
            scroll.Add(new Label("ASCII color:") { X = 1, Y = y });

            var logoColorCombo = new Button()
            {
                X = 30,
                Y = y,
                Width = 3,

                Height = 1,
                Text = Settings.Current.LogoColor.ToString()
            };


            logoColorCombo.Clicked += () =>
            {
                // Open your color picker
                Settings.Current.LogoColor = DialogHelpers.PickColorGrid();

                // Update *the button's* text, not `Text = ...`
                logoColorCombo.Text = Settings.Current.LogoColor.ToString();

                // Force redraw the button *and its parent view*
                logoColorCombo.SetNeedsDisplay();
                scroll.SetNeedsDisplay();

            };
            scroll.Add(logoColorCombo);
            y += 2;


            //Enable/disable ASCII
            var disableASCII = new CheckBox("Disable ASCII") { X = 1, Y = y, Checked = Settings.Current.DisableASCII };
            scroll.Add(disableASCII);
            y += 2;



            #endregion

            // Save button
            var saveBtn = new Button("Save") { X = 1, Y = y };
            scroll.Add(saveBtn);
            y += 2;

            // Set the content size so scroll bars work
            scroll.ContentSize = new Terminal.Gui.Size(Application.Top.Frame.Width - 2, y);

            #region EVENT HANDLERS

            logFileDialogBtn.Clicked += () =>
            {
                string? path = DialogHelpers.ShowSaveFileDialog("Select log file path", "Select filename for the log file.", [".txt"]);
                if (!string.IsNullOrWhiteSpace(path))
                {
                    // Get directory from full path
                    logPathField.Text = path;
                }
            };

            settingsFileDialogBtn.Clicked += () =>
            {
                string? path = DialogHelpers.ShowSaveFileDialog("Select config file path", "Select filename for the config file.", [".json"]);
                if (!string.IsNullOrWhiteSpace(path))
                {
                    // Get directory from full path
                    settingsPathField.Text = path;
                }
            };


            downloadFolderDialogBtn.Clicked += () =>
            {
                string? path = DialogHelpers.ShowSaveFileDialog("Select default download folder", "Select the default download folder path.", [""]);
                if (!string.IsNullOrWhiteSpace(path))
                {
                    // Get directory from full path
                    string dir = Path.GetDirectoryName(path) ?? path;
                    downloadPathField.Text = path;
                }
            };



            saveBtn.Clicked += () =>
            {
                try
                {
                    // Parse ports
                    if (!ushort.TryParse(portField.Text.ToString(), out var port))
                    {
                        MessageBox.ErrorQuery("Error", "Invalid port number.", "OK");
                        return;
                    }

                    if (!ushort.TryParse(dhtPortField.Text.ToString(), out var dhtPort))
                    {
                        MessageBox.ErrorQuery("Error", "Invalid DHT port number.", "OK");
                        return;
                    }

                    // Parse limits
                    if (!ushort.TryParse(maxConnField.Text.ToString(), out var maxConn))
                    {
                        MessageBox.ErrorQuery("Error", "Invalid max connections.", "OK");
                        return;
                    }

                    if (!int.TryParse(maxDlField.Text.ToString(), out var maxDl))
                    {
                        MessageBox.ErrorQuery("Error", "Invalid max download speed.", "OK");
                        return;
                    }

                    if (!int.TryParse(maxUpField.Text.ToString(), out var maxUp))
                    {
                        MessageBox.ErrorQuery("Error", "Invalid max upload speed.", "OK");
                        return;
                    }

                    if (!int.TryParse(refreshRateField.Text.ToString(), out var refRate))
                    {
                        MessageBox.ErrorQuery("Error", "Invalid progress refresh rate.", "OK");
                        return;
                    }

                    if (refRate > 1000)
                    {
                        MessageBox.ErrorQuery("Error", "Invalid progress refresh rate. Must be over 1000ms.", "OK");
                        return;
                    }


                    // Paths
                    string downloadPath = downloadPathField.Text.ToString()!.Trim();
                    string logPath = logPathField.Text.ToString()!.Trim()!;
                    string settingsPath = settingsPathField.Text.ToString()!.Trim();

                    if (string.IsNullOrWhiteSpace(downloadPath))
                    {
                        MessageBox.ErrorQuery("Error", "Download path cannot be empty.", "OK");
                        return;
                    }

                    if (!Directory.Exists(downloadPath))
                    {
                        if (MessageBox.Query("Missing Directory",
                            "Download path does not exist. Create it?",
                            "Yes", "No") == 0)
                        {
                            Directory.CreateDirectory(downloadPath);
                        }
                        else return;
                    }


                    if (string.IsNullOrWhiteSpace(logPath))
                    {
                        MessageBox.ErrorQuery("Error", "Log path cannot be empty.", "OK");
                        return;
                    }



                    if (string.IsNullOrWhiteSpace(settingsPath))
                    {
                        MessageBox.ErrorQuery("Error", "Settings path cannot be empty.", "OK");
                        return;
                    }





                    // Apply settings
                    Settings.Current.Port = port;
                    Settings.Current.DhtPort = dhtPort;
                    Settings.Current.MaxConnections = maxConn;
                    Settings.Current.MaxDownloadSpeed = maxDl * 1024;
                    Settings.Current.MaxUploadSpeed = maxUp * 1024;
                    Settings.Current.RefreshInterval = refRate;

                    Settings.Current.StopSeedingWhenFinished = stopSeedCheckbox.Checked;
                    Settings.Current.EnablePortForwarding = portFwdCheckbox.Checked;
                    Settings.Current.DetailedLogging = detailedLogging.Checked;
                    Settings.Current.DisableASCII = disableASCII.Checked;
                    Settings.Current.DisableColoredHotkeyInfo = disableHotKeyColors.Checked;


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

                    MessageBox.Query("Settings", "Settings saved successfully.\n Some changes will take effect after restarting.", "OK");
                    Log.Write("Settings saved.");
                }
                catch (Exception ex)
                {
                    MessageBox.ErrorQuery("Fatal Error", ex.Message, "OK");
                }
            };


            #endregion
        }
    }
}