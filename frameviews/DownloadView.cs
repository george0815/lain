using System;
using System.IO;
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

            var scroll = new ScrollView()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                ShowVerticalScrollIndicator = true
            };

            Add(scroll);

            int y = 1;

            #region URL AND PATH INPUTS


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


            //Torrent file path
            scroll.Add(new Label("Torrent file path:") { X = 1, Y = y });
            var fileInput = new TextField("") { X = 20, Y = y, Width = 40 };
            scroll.Add(fileInput);
            y += 2;

            // Download path
            scroll.Add(new Label("Download path:") { X = 1, Y = y });
            var downloadPathInput = new TextField(Settings.Current.DefaultDownloadPath)
            {
                X = 20,
                Y = y,
                Width = 40
            };
            scroll.Add(downloadPathInput);
            y += 2;

            #endregion

            #region LIMITS

            // Max connections
            scroll.Add(new Label("Max connections:") { X = 1, Y = y });
            var maxConnField = new TextField(Settings.Current.MaxConnections.ToString())
            {
                X = 30,
                Y = y,
                Width = 10
            };
            scroll.Add(maxConnField);
            y += 2;

            // Max download speed
            scroll.Add(new Label("Max download (kB/s):") { X = 1, Y = y });
            var maxDlField = new TextField(Settings.Current.MaxDownloadSpeed.ToString())
            {
                X = 30,
                Y = y,
                Width = 10
            };
            scroll.Add(maxDlField);
            y += 2;

            // Max upload speed
            scroll.Add(new Label("Max upload (kB/s):") { X = 1, Y = y });
            var maxUpField = new TextField(Settings.Current.MaxUploadSpeed.ToString())
            {
                X = 30,
                Y = y,
                Width = 10
            };
            scroll.Add(maxUpField);
            y += 2;

            #endregion

            var dhtCheckbox = new CheckBox("Enable DHT")
            {
                X = 1,
                Y = y,
                Checked = true
            };
            scroll.Add(dhtCheckbox);
            y += 2;

            var downloadBtn = new Button("Download") { X = 1, Y = y };
            scroll.Add(downloadBtn);
            y += 2;

            scroll.ContentSize = new Terminal.Gui.Size(200, y + 5);

            downloadBtn.Clicked += async () =>
            {
                #region VALIDATION

                bool useMagnet = magnetCheckbox.Checked;
                string magnetText = magnetInput.Text.ToString()!.Trim();
                string fileText = fileInput.Text.ToString()!.Trim();
                string downloadDir = downloadPathInput.Text.ToString()!.Trim();

                // Must use either magnet or torrent file
                if (!useMagnet && string.IsNullOrWhiteSpace(fileText))
                {
                    MessageBox.ErrorQuery("Error", "You must select either a magnet link or a torrent file.", "OK");
                    return;
                }

                // Magnet validation
                if (useMagnet)
                {
                    if (string.IsNullOrWhiteSpace(magnetText))
                    {
                        MessageBox.ErrorQuery("Error", "Magnet link is empty.", "OK");
                        return;
                    }
                    if (!magnetText.StartsWith("magnet:?"))
                    {
                        MessageBox.ErrorQuery("Error", "This does not appear to be a valid magnet link.", "OK");
                        return;
                    }
                }

                // Torrent file validation
                if (!useMagnet)
                {
                    if (!File.Exists(fileText))
                    {
                        MessageBox.ErrorQuery("Error", "Torrent file does not exist.", "OK");
                        return;
                    }
                }

                // Download path validation
                if (string.IsNullOrWhiteSpace(downloadDir) || !Directory.Exists(downloadDir))
                {

                    if (!Directory.Exists(downloadDir) && !string.IsNullOrWhiteSpace(downloadDir))
                    {
                        if (MessageBox.Query("Missing Directory",
                            "Download path does not exist. Create it?",
                            "Yes", "No") == 0)
                        {
                            Directory.CreateDirectory(downloadDir);

                        }
                        else return;
                    }

                }

                // Parse numeric fields safely
                if (!int.TryParse(maxConnField.Text.ToString(), out int maxConn) || maxConn <= 0)
                {
                    MessageBox.ErrorQuery("Error", "Invalid maximum connections value.", "OK");
                    return;
                }

                if (!int.TryParse(maxDlField.Text.ToString(), out int maxDl) || maxDl < 0)
                {
                    MessageBox.ErrorQuery("Error", "Invalid download speed limit.", "OK");
                    return;
                }

                if (!int.TryParse(maxUpField.Text.ToString(), out int maxUp) || maxUp < 0)
                {
                    MessageBox.ErrorQuery("Error", "Invalid upload speed limit.", "OK");
                    return;
                }

                #endregion

                #region SETUP SETTINGS OBJECT AND START DOWNLOAD

                TorrentData settings = new TorrentData
                {
                    UseMagnetLink = useMagnet,
                    MagnetUrl = magnetText,
                    TorPath = fileText,
                    DownPath = downloadDir,
                    MaxConnections = maxConn,
                    MaxDownloadRate = maxDl * 1024,
                    MaxUploadRate = maxUp * 1024,
                    UseDht = dhtCheckbox.Checked
                };

                //Add torrent asynchronously
                await Task.Run(async () =>
                {
                    try
                    {
                        await TorrentOperations.AddTorrent(settings);
                    }
                    catch (Exception ex)
                    {
                        Application.MainLoop.Invoke(() =>
                        {
                            MessageBox.ErrorQuery("Error", $"Torrent startup failed:\n{ex.Message}", "OK");
                        });
                    }
                });

                MessageBox.Query("Download", "Torrent download started.", "OK");

                #endregion

            };
        }
    }
}