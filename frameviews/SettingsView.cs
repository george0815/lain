using System;
using System.Collections.Generic;
using System.Text;
using Terminal.Gui;

namespace lain.frameviews
{
    internal class SettingsView : FrameView
    {

        public SettingsView()
            : base("Settings")
        {


            X = 20;
            Y = 3;
            Width = Dim.Fill();
            Height = Dim.Fill();
           

            int y = 1; // starting Y position

            // Port
             Add(new Label("Port:") { X = 1, Y = y });
            var portField = new TextField(Settings.DhtPort.ToString()) { X = 20, Y = y, Width = 10 };
             Add(portField);
            y += 2;

            // Max Connections
             Add(new Label("Max Connections:") { X = 1, Y = y });
            var maxConnField = new TextField(Settings.MaxConnections.ToString()) { X = 20, Y = y, Width = 10 };
             Add(maxConnField);
            y += 2;

            // Max Seeders per Torrent
             Add(new Label("Max Seeders/Torrent:") { X = 1, Y = y });
            var maxSeedField = new TextField(Settings.MaxSeedersPerTorrent.ToString()) { X = 20, Y = y, Width = 10 };
             Add(maxSeedField);
            y += 2;

            // Max Leechers per Torrent
             Add(new Label("Max Leechers/Torrent:") { X = 1, Y = y });
            var maxLeechField = new TextField(Settings.MaxLeechersPerTorrent.ToString()) { X = 20, Y = y, Width = 10 };
             Add(maxLeechField);
            y += 2;

            // Max Download Speed
             Add(new Label("Max Download Speed (kB/s):") { X = 1, Y = y });
            var maxDlField = new TextField(Settings.MaxDownloadSpeed.ToString()) { X = 25, Y = y, Width = 10 };
             Add(maxDlField);
            y += 2;

            // Max Upload Speed
             Add(new Label("Max Upload Speed (kB/s):") { X = 1, Y = y });
            var maxUpField = new TextField(Settings.MaxUploadSpeed.ToString()) { X = 25, Y = y, Width = 10 };
             Add(maxUpField);
            y += 2;

            // Enable DHT
            var dhtCheckbox = new CheckBox("Enable DHT") { X = 1, Y = y, Checked = Settings.EnableDht };
             Add(dhtCheckbox);
            y += 2;

            // Stop Seeding When Finished
            var stopSeedCheckbox = new CheckBox("Stop Seeding When Finished") { X = 1, Y = y, Checked = Settings.StopSeedingWhenFinished };
             Add(stopSeedCheckbox);
            y += 2;

            // Detailed logging
            var detailedLogging = new CheckBox("Enable detailed logging") { X = 1, Y = y, Checked = Settings.DetailedLogging };
            Add(detailedLogging);
            y += 2;

            // Enable Port Forwarding
            var portFwdCheckbox = new CheckBox("Enable Port Forwarding") { X = 1, Y = y, Checked = Settings.EnablePortForwarding };
             Add(portFwdCheckbox);
            y += 2;

            // Default Download Path
             Add(new Label("Default Download Path:") { X = 1, Y = y });
            var downloadPathField = new TextField(Settings.DefaultDownloadPath ?? "") { X = 25, Y = y, Width = 40 };
             Add(downloadPathField);
            y += 2;

            // Log Path
             Add(new Label("Log Path:") { X = 1, Y = y });
            var logPathField = new TextField(Settings.LogPath ?? "") { X = 25, Y = y, Width = 40 };
             Add(logPathField);
            y += 2;

            // Settings Path
             Add(new Label("Settings Path:") { X = 1, Y = y });
            var settingsPathField = new TextField(Settings.SettingsPath ?? "") { X = 25, Y = y, Width = 40 };
             Add(settingsPathField);
            y += 2;


            // Save Button
            var saveBtn = new Button("Save")
            {
                X = 1,
                Y = y
            };


             Add(saveBtn);




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


        }
    }
}
