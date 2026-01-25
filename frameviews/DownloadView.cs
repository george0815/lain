using MonoTorrent;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using lain.helpers;
using Terminal.Gui;

namespace lain.frameviews
{
    /// <summary>
    /// UI view responsible for starting new torrent downloads.
    ///
    /// Supports both:
    /// - Magnet links
    /// - Local .torrent files
    ///
    /// This view handles user input, validation, and basic settings
    /// (paths, speed limits, DHT, connection limits), then delegates
    /// the actual download process to TorrentOperations.
    /// </summary>
    internal class DownloadView : FrameView
    {
        public DownloadView()
            : base(Resources.Download)
        {
            // Position the frame within the main application layout.
            X = 20;
            Y = SettingsData.HeaderHeight;
            Width = Dim.Fill();
            Height = Dim.Fill();

            // ScrollView prevents layout overflow as controls grow vertically.
            var scroll = new ScrollView()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                ShowVerticalScrollIndicator = true
            };

            Add(scroll);

            // Vertical layout cursor.
            int y = 1;

            #region URL AND PATH INPUTS
            // -------------------
            // Magnet / torrent inputs
            // -------------------

            // Magnet URL input.
            scroll.Add(new Label(Resources.Magnetlink) { X = 1, Y = y });

            // X position is culture-sensitive to account for wider Japanese labels.
            var magnetInput = new TextField("")
            {
                X = (Thread.CurrentThread.CurrentUICulture.Name == "ja-JP" ? 25 : 20),
                Y = y,
                Width = 40
            };
            scroll.Add(magnetInput);
            y += 2;

            // Toggle between magnet-based and file-based downloads.
            var magnetCheckbox = new CheckBox(Resources.Usemagnetlink)
            {
                X = 1,
                Y = y,
                Checked = false
            };
            scroll.Add(magnetCheckbox);
            y += 2;

            // Local .torrent file path input.
            scroll.Add(new Label(Resources.Torrentfilepath) { X = 1, Y = y });
            var fileInput = new TextField("")
            {
                X = (Thread.CurrentThread.CurrentUICulture.Name == "ja-JP" ? 25 : 20),
                Y = y,
                Width = 40
            };
            scroll.Add(fileInput);

            // Opens file picker dialog for .torrent files.
            var torFileDialogBtn = new Button("...")
            {
                X = (Thread.CurrentThread.CurrentUICulture.Name == "ja-JP" ? 66 : 61),
                Y = y
            };
            scroll.Add(torFileDialogBtn);
            y += 2;

            // Download destination directory.
            scroll.Add(new Label(Resources.Downloadpath) { X = 1, Y = y });
            var downloadPathInput = new TextField(Settings.Current.DefaultDownloadPath)
            {
                X = (Thread.CurrentThread.CurrentUICulture.Name == "ja-JP" ? 25 : 20),
                Y = y,
                Width = 40
            };
            scroll.Add(downloadPathInput);

            // Folder picker dialog for download destination.
            var downloadFolderDialogBtn = new Button("...")
            {
                X = (Thread.CurrentThread.CurrentUICulture.Name == "ja-JP" ? 66 : 61),
                Y = y
            };
            scroll.Add(downloadFolderDialogBtn);
            y += 2;

            #endregion

            #region LIMITS
            // -------------------
            // Connection and speed limits
            // -------------------

            // Maximum peer connections.
            scroll.Add(new Label(Resources.Maxconnections) { X = 1, Y = y });
            var maxConnField = new TextField(Settings.Current.MaxConnections.ToString())
            {
                X = (Thread.CurrentThread.CurrentUICulture.Name == "ja-JP" ? 30 : 31),
                Y = y,
                Width = 10
            };
            scroll.Add(maxConnField);
            y += 2;

            // Maximum download speed (MB/s, converted later).
            scroll.Add(new Label(Resources.Maxdownload_MB_s_) { X = 1, Y = y });
            var maxDlField = new TextField((Settings.Current.MaxDownloadSpeed / (1024 * 1024)).ToString())
            {
                X = (Thread.CurrentThread.CurrentUICulture.Name == "ja-JP" ? 30 : 31),
                Y = y,
                Width = 10
            };
            scroll.Add(maxDlField);
            y += 2;

            // Maximum upload speed (MB/s, converted later).
            scroll.Add(new Label(Resources.Maxupload_MB_s_) { X = 1, Y = y });
            var maxUpField = new TextField((Settings.Current.MaxUploadSpeed / (1024 * 1024)).ToString())
            {
                X = (Thread.CurrentThread.CurrentUICulture.Name == "ja-JP" ? 30 : 31),
                Y = y,
                Width = 10
            };
            scroll.Add(maxUpField);
            y += 2;

            #endregion

            // Toggle for DHT participation.
            var dhtCheckbox = new CheckBox(Resources.EnableDHT)
            {
                X = 1,
                Y = y,
                Checked = true
            };
            scroll.Add(dhtCheckbox);
            y += 2;

            // Primary action button to start the download.
            var downloadBtn = new Button(Resources.Download) { X = 1, Y = y };
            scroll.Add(downloadBtn);

            // Inform ScrollView of the total virtual content height.
            scroll.ContentSize = new Terminal.Gui.Size(200, y + 2);

            #region BUTTON EVENTS
            // -------------------
            // Dialog handlers and download action
            // -------------------

            torFileDialogBtn.Clicked += () =>
            {
                string? path = DialogHelpers.ShowFileDialog(
                    Resources.Selecttorrentfile,
                    Resources.Selectatorrentfile,
                    [".torrent"],
                    false
                );

                if (!string.IsNullOrWhiteSpace(path))
                {
                    fileInput.Text = path;
                }
            };

            downloadFolderDialogBtn.Clicked += () =>
            {
                string? path = DialogHelpers.ShowSaveFileDialog(
                    Resources.Selectdownloadfolder,
                    Resources.Selectdownloadfolder,
                    [""],
                    Resources.Selectfolder
                );

                if (!string.IsNullOrWhiteSpace(path))
                {
                    // Path may be returned as a file; directory is derived if needed.
                    string dir = Path.GetDirectoryName(path) ?? path;
                    downloadPathInput.Text = path;
                }
            };

            // Main download workflow.
            downloadBtn.Clicked += async () =>
            {
                #region VALIDATION
                // -------------------
                // Input validation
                // -------------------

                bool useMagnet = magnetCheckbox.Checked;
                string magnetText = magnetInput.Text.ToString()!.Trim();
                string fileText = fileInput.Text.ToString()!.Trim();
                string downloadDir = downloadPathInput.Text.ToString()!.Trim();

                // User must choose either magnet link or torrent file.
                if (!useMagnet && string.IsNullOrWhiteSpace(fileText))
                {
                    MessageBox.ErrorQuery(
                        Resources.Error,
                        Resources.Youmustselecteitheramagnetlinkoratorrentfile,
                        Resources.OK
                    );
                    return;
                }

                // Magnet link validation.
                if (useMagnet)
                {
                    if (string.IsNullOrWhiteSpace(magnetText))
                    {
                        MessageBox.ErrorQuery(Resources.Error, Resources.Magnetlinkisempty, Resources.OK);
                        return;
                    }

                    if (!magnetText.StartsWith("magnet:?"))
                    {
                        MessageBox.ErrorQuery(
                            Resources.Error,
                            Resources.Thisdoesnotappeartobeavalidmagnetlink,
                            Resources.OK
                        );
                        return;
                    }
                }

                // Torrent file validation.
                if (!useMagnet)
                {
                    if (!File.Exists(fileText))
                    {
                        MessageBox.ErrorQuery(
                            Resources.Error,
                            Resources.Torrentfiledoesnotexist,
                            Resources.OK
                        );
                        return;
                    }
                }

                // Download directory validation and optional creation.
                if (string.IsNullOrWhiteSpace(downloadDir) || !Directory.Exists(downloadDir))
                {
                    if (!Directory.Exists(downloadDir) && !string.IsNullOrWhiteSpace(downloadDir))
                    {
                        if (MessageBox.Query(
                            Resources.MissingDirectory,
                            Resources.DownloadpathdoesnotexistCreateit_,
                            Resources.Yes,
                            Resources.No
                        ) == 0)
                        {
                            Directory.CreateDirectory(downloadDir);
                        }
                        else return;
                    }
                }

                // Parse and validate numeric limits.
                if (!int.TryParse(maxConnField.Text.ToString(), out int maxConn)
                    || maxConn <= 0
                    || maxConn > 50000)
                {
                    MessageBox.ErrorQuery(Resources.Error, Resources.Invalidmaxconnections, Resources.OK);
                    return;
                }

                if (!int.TryParse(maxDlField.Text.ToString(), out int maxDl)
                    || maxDl < 0
                    || (maxDl > (Int32.MaxValue / 1048576)))
                {
                    MessageBox.ErrorQuery(Resources.Error, Resources.Invaliddownloadspeedlimit, Resources.OK);
                    return;
                }

                if (!int.TryParse(maxUpField.Text.ToString(), out int maxUp)
                    || maxUp < 0
                    || (maxUp > (Int32.MaxValue / 1048576)))
                {
                    MessageBox.ErrorQuery(Resources.Error, Resources.Invaliduploadspeedlimit, Resources.OK);
                    return;
                }

                #endregion

                #region SETUP SETTINGS OBJECT AND START DOWNLOAD
                // -------------------
                // Torrent setup and async start
                // -------------------

                TorrentData settings = new()
                {
                    UseMagnetLink = useMagnet,
                    MagnetUrl = magnetText,
                    TorPath = fileText,
                    DownPath = downloadDir,
                    MaxConnections = maxConn,
                    MaxDownloadRate = maxDl /* MB */ * 1024 * 1024,
                    MaxUploadRate = maxUp /* MB */ * 1024 * 1024,
                    UseDht = dhtCheckbox.Checked
                };

                MessageBox.Query(Resources.Download, Resources.Torrentdownloadstarted, Resources.OK);

                // Add torrent off the UI thread to avoid blocking.
                await Task.Run(async () =>
                {
                    try
                    {
                        await TorrentOperations.AddTorrent(settings, false, false);
                    }
                    catch (Exception ex)
                    {
                        Application.MainLoop.Invoke(() =>
                        {
                            MessageBox.ErrorQuery(
                                Resources.Error,
                                $"{Resources.Torrentdownloadfailed}\n{ex.Message}",
                                Resources.OK
                            );
                        });
                    }
                });

                #endregion
            };

            #endregion
        }
    }
}
