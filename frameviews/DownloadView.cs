using MonoTorrent;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Terminal.Gui;

namespace lain.frameviews
{
    internal class DownloadView : FrameView
    {
        public DownloadView()
            : base(Resources.Download)
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
            scroll.Add(new Label(Resources.Magnetlink) { X = 1, Y = y });
            var magnetInput = new TextField("") { X = (Thread.CurrentThread.CurrentUICulture.Name == "ja-JP" ? 25 : 20), Y = y, Width = 40 };
            scroll.Add(magnetInput);
            y += 2;

            var magnetCheckbox = new CheckBox(Resources.Usemagnetlink)
            {
                X = 1,
                Y = y,
                Checked = false
            };
            scroll.Add(magnetCheckbox);
            y += 2;


            //Torrent file path
            scroll.Add(new Label(Resources.Torrentfilepath) { X = 1, Y = y });
            var fileInput = new TextField("") { X = (Thread.CurrentThread.CurrentUICulture.Name == "ja-JP" ? 25 : 20), Y = y, Width = 40 };
            scroll.Add(fileInput);
            

            var torFileDialogBtn = new Button("...") { X = (Thread.CurrentThread.CurrentUICulture.Name == "ja-JP" ? 66 : 61), Y = y };
            scroll.Add(torFileDialogBtn);

            y += 2;


            // Download path
            scroll.Add(new Label(Resources.Downloadpath) { X = 1, Y = y });
            var downloadPathInput = new TextField(Settings.Current.DefaultDownloadPath)
            {
                X = (Thread.CurrentThread.CurrentUICulture.Name == "ja-JP" ? 25 : 20),
                Y = y,
                Width = 40
            };
            scroll.Add(downloadPathInput);

            var downloadFolderDialogBtn = new Button("...") { X = (Thread.CurrentThread.CurrentUICulture.Name == "ja-JP" ? 66 : 61), Y = y };
            scroll.Add(downloadFolderDialogBtn);

            y += 2;

            #endregion

            #region LIMITS

            // Max connections
            scroll.Add(new Label(Resources.Maxconnections) { X = 1, Y = y });
            var maxConnField = new TextField(Settings.Current.MaxConnections.ToString())
            {
                X = (Thread.CurrentThread.CurrentUICulture.Name == "ja-JP" ? 30 : 31),
                Y = y,
                Width = 10
            };
            scroll.Add(maxConnField);
            y += 2;

            // Max download speed
            scroll.Add(new Label(Resources.Maxdownload_MB_s_) { X = 1, Y = y });
            var maxDlField = new TextField((Settings.Current.MaxDownloadSpeed / (1024 * 1024)).ToString())
            {
                X = (Thread.CurrentThread.CurrentUICulture.Name == "ja-JP" ? 30 : 31),
                Y = y,
                Width = 10
            };
            scroll.Add(maxDlField);
            y += 2;

            // Max upload speed
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

            var dhtCheckbox = new CheckBox(Resources.EnableDHT)
            {
                X = 1,
                Y = y,
                Checked = true
            };
            scroll.Add(dhtCheckbox);
            y += 2;

            var downloadBtn = new Button(Resources.Download) { X = 1, Y = y };
            scroll.Add(downloadBtn);


            scroll.ContentSize = new Terminal.Gui.Size(200, y + 2);

            #region BUTTON EVENTS

            torFileDialogBtn.Clicked += () =>
            {
                string? path = DialogHelpers.ShowFileDialog(Resources.Selecttorrentfile, Resources.Selectatorrentfile, [".torrent"], false);
                if (!string.IsNullOrWhiteSpace(path))
                {
                    fileInput.Text = path;
                }
            };


            downloadFolderDialogBtn.Clicked += () =>
            {
                string? path = DialogHelpers.ShowSaveFileDialog(Resources.Selectdownloadfolder, Resources.Selectdownloadfolder, [""], Resources.Selectfolder);
                if (!string.IsNullOrWhiteSpace(path))
                {
                    // Get directory from full path
                    string dir = Path.GetDirectoryName(path) ?? path;
                    downloadPathInput.Text = path;
                }
            };


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
                    MessageBox.ErrorQuery(Resources.Error, Resources.Youmustselecteitheramagnetlinkoratorrentfile, Resources.OK);
                    return;
                }

                // Magnet validation
                if (useMagnet)
                {
                    if (string.IsNullOrWhiteSpace(magnetText))
                    {
                        MessageBox.ErrorQuery(Resources.Error, Resources.Magnetlinkisempty, Resources.OK);
                        return;
                    }
                    if (!magnetText.StartsWith("magnet:?"))
                    {
                        MessageBox.ErrorQuery(Resources.Error, Resources.Thisdoesnotappeartobeavalidmagnetlink, Resources.OK);
                        return;
                    }
                }

                // Torrent file validation
                if (!useMagnet)
                {
                    if (!File.Exists(fileText))
                    {
                        MessageBox.ErrorQuery(Resources.Error, Resources.Torrentfiledoesnotexist, Resources.OK);
                        return;
                    }
                }

                // Download path validation
                if (string.IsNullOrWhiteSpace(downloadDir) || !Directory.Exists(downloadDir))
                {

                    if (!Directory.Exists(downloadDir) && !string.IsNullOrWhiteSpace(downloadDir))
                    {
                        if (MessageBox.Query(Resources.MissingDirectory,
                            Resources.DownloadpathdoesnotexistCreateit_,
                            Resources.Yes, Resources.No) == 0)
                        {
                            Directory.CreateDirectory(downloadDir);

                        }
                        else return;
                    }

                }

                // Parse numeric fields safely
                if (!int.TryParse(maxConnField.Text.ToString(), out int maxConn) || maxConn <= 0 || maxConn > 50000 || maxConn < 0)
                {
                    MessageBox.ErrorQuery(Resources.Error, Resources.Invalidmaxconnections, Resources.OK);
                    return;
                }

                if (!int.TryParse(maxDlField.Text.ToString(), out int maxDl) || maxDl < 0 || (maxDl > (Int32.MaxValue / 1048576)))
                {
                    MessageBox.ErrorQuery(Resources.Error, Resources.Invaliddownloadspeedlimit, Resources.OK);
                    return;
                }

                if (!int.TryParse(maxUpField.Text.ToString(), out int maxUp) || maxUp < 0 || (maxUp > (Int32.MaxValue / 1048576)))
                {
                    MessageBox.ErrorQuery(Resources.Error, Resources.Invaliduploadspeedlimit, Resources.OK);
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
                    MaxDownloadRate = maxDl /* to KB*/ * 1024 /* to MB*/ * 1024,
                    MaxUploadRate = maxUp /* to KB*/ * 1024 /* to MB*/ * 1024,
                    UseDht = dhtCheckbox.Checked
                };

                MessageBox.Query(Resources.Download, Resources.Torrentdownloadstarted, Resources.OK);


                //Add torrent asynchronously
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
                            MessageBox.ErrorQuery(Resources.Error, $"{Resources.Torrentdownloadfailed}\n{ex.Message}", Resources.OK); 
                        });
                    }
                });

               

                #endregion

            };

            #endregion



        }


       
    }
}