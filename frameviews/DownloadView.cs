using System;
using System.Threading.Tasks;
using Terminal.Gui;

namespace lain.frameviews
{
    internal class DownloadView : FrameView
    {
        public DownloadView()
            : base("Download")
        {
            X = 20;
            Y = SettingsData.HeaderHeight;
            Width = Dim.Fill();
            Height = Dim.Fill();

            // Create scroll view
            var scroll = new ScrollView()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(),

                ShowVerticalScrollIndicator = true,
                ShowHorizontalScrollIndicator = false
            };

            Add(scroll);

            int y = 1;


            #region URLS AND PATHS

            // Magnet URL
            scroll.Add(new Label("Magnet URL:") { X = 1, Y = y });
            var magnetInput = new TextField("") { X = 20, Y = y, Width = 40 };
            scroll.Add(magnetInput);
            y += 2;

            var magnetCheckbox = new CheckBox("Use magnet link")
            {
                X = 1,
                Y = y,
                Checked = false
            };
            scroll.Add(magnetCheckbox);
            y += 2;

            // Torrent file path
            scroll.Add(new Label("Torrent file path:") { X = 1, Y = y });
            var fileInput = new TextField("") { X = 20, Y = y, Width = 40 };
            scroll.Add(fileInput);
            y += 2;

            // Download path
            scroll.Add(new Label("Download path:") { X = 1, Y = y });
            var downloadPathInput = new TextField(Settings.Current.DefaultDownloadPath) { X = 20, Y = y, Width = 40 };
            scroll.Add(downloadPathInput);
            y += 2;

            #endregion

            #region LIMITS

            // Max Connections
            scroll.Add(new Label("Max connections:") { X = 1, Y = y });
            var maxConnField = new TextField(Settings.Current.MaxConnections.ToString())
            {
                X = 30,
                Y = y,
                Width = 10
            };
            scroll.Add(maxConnField);
            y += 2;

            // Max Download Speed
            scroll.Add(new Label("Max download speed (kB/s):") { X = 1, Y = y });
            var maxDlField = new TextField(Settings.Current.MaxDownloadSpeed.ToString())
            {
                X = 30,
                Y = y,
                Width = 10
            };
            scroll.Add(maxDlField);
            y += 2;

            // Max Upload Speed
            scroll.Add(new Label("Max upload speed (kB/s):") { X = 1, Y = y });
            var maxUpField = new TextField(Settings.Current.MaxUploadSpeed.ToString())
            {
                X = 30,
                Y = y,
                Width = 10
            };
            scroll.Add(maxUpField);
            y += 2;

            #endregion


            // DHT checkbox
            var dhtCheckbox = new CheckBox("Enable DHT")
            {
                X = 1,
                Y = y,
                Checked = false
            };
            scroll.Add(dhtCheckbox);
            y += 2;

            // Download button
            var downloadBtn = new Button("Download") { X = 1, Y = y };
            scroll.Add(downloadBtn);
            y += 2;

            // Set scroll content size AFTER layout
            scroll.ContentSize = new Terminal.Gui.Size(200, y + 5);

            // Download button action
            downloadBtn.Clicked += async () =>
            {
                TorrentData settings = new TorrentData
                {
                    UseMagnetLink = magnetCheckbox.Checked,
                    MagnetUrl = magnetInput.Text.ToString()!,
                    TorPath = fileInput.Text.ToString()!,
                    DownPath = downloadPathInput.Text.ToString()!,
                    MaxConnections = int.TryParse(maxConnField.Text.ToString(), out int mc)  ? mc : Settings.Current.MaxConnections,
                    MaxDownloadRate = int.TryParse(maxDlField.Text.ToString(), out int mds) ? mds * 1024 : Settings.Current.MaxDownloadSpeed,
                    MaxUploadRate = int.TryParse(maxUpField.Text.ToString(), out int mus) ? mus * 1024 : Settings.Current.MaxUploadSpeed,
                    UseDht = dhtCheckbox.Checked
                };

                _ = Task.Run(async () =>
                {
                    await TorrentOperations.AddTorrent(settings);
                });

                MessageBox.Query("Download", "Torrent download started.", "OK");
            };
        }
    }
}
