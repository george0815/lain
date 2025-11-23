using System;
using System.Drawing;
using Terminal.Gui;

namespace lain.frameviews
{
    internal class SettingsView : FrameView
    {

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
            { "Bright Blue", Terminal.Gui.Color.BrightBlue },
            { "Bright Green", Terminal.Gui.Color.BrightGreen },
            { "Bright Cyan", Terminal.Gui.Color.BrightCyan },
            { "Bright Red", Terminal.Gui.Color.BrightRed },
            { "Bright Magenta", Terminal.Gui.Color.BrightMagenta },
            { "Bright Yellow", Terminal.Gui.Color.BrightYellow},
            { "White", Terminal.Gui.Color.White }
        };


        public SettingsView()
            : base("Settings")
        {
            X = 20;
            Y = Settings.HeaderHeight;
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

            // Port
            scroll.Add(new Label("Port:") { X = 1, Y = y });
            var portField = new TextField(Settings.DhtPort.ToString()) { X = 30, Y = y, Width = 10 };
            scroll.Add(portField);
            y += 2;

            // Max Connections
            scroll.Add(new Label("Max Connections:") { X = 1, Y = y });
            var maxConnField = new TextField(Settings.MaxConnections.ToString()) { X = 30, Y = y, Width = 10 };
            scroll.Add(maxConnField);
            y += 2;

            // Max Seeders per Torrent
            scroll.Add(new Label("Max Seeders/Torrent:") { X = 1, Y = y });
            var maxSeedField = new TextField(Settings.MaxSeedersPerTorrent.ToString()) { X = 30, Y = y, Width = 10 };
            scroll.Add(maxSeedField);
            y += 2;

            // Max Leechers per Torrent
            scroll.Add(new Label("Max Leechers/Torrent:") { X = 1, Y = y });
            var maxLeechField = new TextField(Settings.MaxLeechersPerTorrent.ToString()) { X = 30, Y = y, Width = 10 };
            scroll.Add(maxLeechField);
            y += 2;

            // Max Download Speed
            scroll.Add(new Label("Max Download Speed (kB/s):") { X = 1, Y = y });
            var maxDlField = new TextField(Settings.MaxDownloadSpeed.ToString()) { X = 30, Y = y, Width = 10 };
            scroll.Add(maxDlField);
            y += 2;

            // Max Upload Speed
            scroll.Add(new Label("Max Upload Speed (kB/s):") { X = 1, Y = y });
            var maxUpField = new TextField(Settings.MaxUploadSpeed.ToString()) { X = 30, Y = y, Width = 10 };
            scroll.Add(maxUpField);
            y += 2;

            // Checkboxes
            var dhtCheckbox = new CheckBox("Enable DHT for public torrents") { X = 1, Y = y, Checked = Settings.EnableDht };
            scroll.Add(dhtCheckbox);
            y += 2;

            var stopSeedCheckbox = new CheckBox("Stop Seeding When Finished") { X = 1, Y = y, Checked = Settings.StopSeedingWhenFinished };
            scroll.Add(stopSeedCheckbox);
            y += 2;

            var detailedLogging = new CheckBox("Enable detailed logging") { X = 1, Y = y, Checked = Settings.DetailedLogging };
            scroll.Add(detailedLogging);
            y += 2;

            var portFwdCheckbox = new CheckBox("Enable Port Forwarding") { X = 1, Y = y, Checked = Settings.EnablePortForwarding };
            scroll.Add(portFwdCheckbox);
            y += 2;

            // Paths
            scroll.Add(new Label("Default Download Path:") { X = 1, Y = y });
            var downloadPathField = new TextField(Settings.DefaultDownloadPath ?? "") { X = 30, Y = y, Width = 40 };
            scroll.Add(downloadPathField);
            y += 2;

            scroll.Add(new Label("Log Path:") { X = 1, Y = y });
            var logPathField = new TextField(Settings.LogPath ?? "") { X = 30, Y = y, Width = 40 };
            scroll.Add(logPathField);
            y += 2;

            scroll.Add(new Label("Settings Path:") { X = 1, Y = y });
            var settingsPathField = new TextField(Settings.SettingsPath ?? "") { X = 30, Y = y, Width = 40 };
            scroll.Add(settingsPathField);
            y += 3;

            

            // Background Color
            scroll.Add(new Label("Background color:") { X = 1, Y = y });

            var bgColorCombo = new ComboBox()
            {
                X = 30,
                Y = y,
                Width = 15,
                ReadOnly = true,
                Height = 8
            };
            bgColorCombo.SetSource(new List<string>(colors.Keys));
            bgColorCombo.SelectedItem = (int)Settings.BackgroundColor!; // default
            scroll.Add(bgColorCombo);
            y += 2;

            // Text Color
            scroll.Add(new Label("Text color:") { X = 1, Y = y });

            var textColorCombo = new ComboBox()
            {
                X = 30,
                Y = y,
                Width = 15,
                ReadOnly = true,
                Height = 8
            };
            textColorCombo.SetSource(new List<string>(colors.Keys));
            textColorCombo.SelectedItem = (int)Settings.TextColor!; // default
            scroll.Add(textColorCombo);
            y += 2;

            

            //Focus background color
            scroll.Add(new Label("Focus background color:") { X = 1, Y = y });

            var backgroundFocusColorCombo = new ComboBox()
            {
                X = 30,
                Y = y,
                Width = 15,
                ReadOnly = true,
                Height = 8
            };
            backgroundFocusColorCombo.SetSource(new List<string>(colors.Keys));
            backgroundFocusColorCombo.SelectedItem = (int)Settings.FocusBackgroundColor!; // default
            scroll.Add(backgroundFocusColorCombo);
            y += 2;

            //Focus text color
            scroll.Add(new Label("Focus text color:") { X = 1, Y = y });

            var textFocusColorCombo = new ComboBox()
            {
                X = 30,
                Y = y,
                Width = 15,
                ReadOnly = true,
                Height = 8
            };
            textFocusColorCombo.SetSource(new List<string>(colors.Keys));
            textFocusColorCombo.SelectedItem = (int)Settings.FocusTextColor!; // default
            scroll.Add(textFocusColorCombo);
            y += 2;


            //Hot text color
            scroll.Add(new Label("Hotkey text color:") { X = 1, Y = y });

            var hotTextColorCombo = new ComboBox()
            {
                X = 30,
                Y = y,
                Width = 15,
                ReadOnly = true,
                Height = 8
            };
            hotTextColorCombo.SetSource(new List<string>(colors.Keys));
            hotTextColorCombo.SelectedItem = (int)Settings.HotTextColor!; // default
            scroll.Add(hotTextColorCombo);
            y += 2;


            // Save button
            var saveBtn = new Button("Save") { X = 1, Y = y };
            scroll.Add(saveBtn);
            y += 2;

            // Set the content size so scroll bars work
            scroll.ContentSize = new Terminal.Gui.Size(Application.Top.Frame.Width - 2, y);

            saveBtn.Clicked += () =>
            {
                // Numeric fields
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

                //Colors
                Settings.BackgroundColor = colors[bgColorCombo.Text.ToString()!];
                Settings.TextColor = colors[textColorCombo.Text.ToString()!];
                Settings.FocusBackgroundColor = colors[backgroundFocusColorCombo.Text.ToString()!];
                Settings.FocusTextColor = colors[textFocusColorCombo.Text.ToString()!];
                Settings.HotTextColor = colors[hotTextColorCombo.Text.ToString()!];

                Settings.SaveSettings();

                MessageBox.Query("Settings", "Settings saved.", "OK");
            };
        }
    }
}
