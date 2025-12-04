using lain;
using MonoTorrent.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Xml.Linq;
using Terminal.Gui;
using TextCopy;

namespace lain.frameviews
{
    internal class CreateView : FrameView
    {
        public CreateView()
            : base(Resources.Create)
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
                ShowVerticalScrollIndicator = true,
            };

            Add(scroll);

            int y = 1;

            #region PATHS

            scroll.Add(new Label(Resources.File_Folder) { X = 1, Y = y });
            var folderPath = new TextField("") { X = 20, Y = y, Width = 40 };
            scroll.Add(folderPath);
            var filesDialogBtn = new Button("...") { X = 61, Y = y };
            scroll.Add(filesDialogBtn);
            y += 2;

            

            scroll.Add(new Label(Resources.Outputpath) { X = 1, Y = y });
            var outputPath = new TextField("") { X = 20, Y = y, Width = 40 };
            scroll.Add(outputPath);
            var outputFolderDialogBtn = new Button("...") { X = 61, Y = y };
            scroll.Add(outputFolderDialogBtn);
            y += 2;

            #endregion

            #region TRACKERS

            scroll.Add(new Label(Resources.Trackers) { X = 1, Y = y });
            var trackerLink = new TextView()
            {
                X = 20,
                Y = y,
                Width = 40,
                Height = 5
            };
            scroll.Add(trackerLink);
            y += 6;

            #endregion

            #region PIECE SIZE

            scroll.Add(new Label(Resources.PieceSize) { X = 1, Y = y });

            var pieceSizes = new Dictionary<string, int>
            {
                { "16 KB", 16 * 1024 },
                { "32 KB", 32 * 1024 },
                { "64 KB", 64 * 1024 },
                { "128 KB", 128 * 1024 },
                { "256 KB", 256 * 1024 },
                { "512 KB", 512 * 1024 },
                { "1 MB", 1024 * 1024 },
                { "2 MB", 2 * 1024 * 1024 }
            };

            var pieceSizeCombo = new ComboBox()
            {
                X = 20,
                Y = y,
                Width = 20,
                ReadOnly = true,
                Height = pieceSizes.Count
            };
            pieceSizeCombo.SetSource(new List<string>(pieceSizes.Keys));
            pieceSizeCombo.SelectedItem = 0;
            scroll.Add(pieceSizeCombo);
            y += 2;

            #endregion

            #region CHECKBOXES

            var startSeedingAfterCreationCheckbox = new CheckBox(Resources.Startseedingaftercreation)
            {
                X = 1,
                Y = y,
                Checked = true
            };
            scroll.Add(startSeedingAfterCreationCheckbox);
            y += 2;

            var privateTorrentCheckbox = new CheckBox(Resources.Private)
            {
                X = 1,
                Y = y,
                Checked = false
            };
            scroll.Add(privateTorrentCheckbox);
            y += 2;


          

            #endregion

            #region METADATA

            scroll.Add(new Label(Resources.Publisher) { X = 1, Y = y });
            var publisher = new TextField("") { X = 20, Y = y, Width = 40 };
            scroll.Add(publisher);
            y += 2;

            scroll.Add(new Label(Resources.Comment) { X = 1, Y = y });
            var comment = new TextView()
            {
                X = 20,
                Y = y,
                Width = 40,
                Height = 5
            };
            scroll.Add(comment);
            y += 2;

            #endregion

            var createTorBtn = new Button(Resources.Create) { X = 1, Y = y };
            scroll.Add(createTorBtn);
         

            scroll.ContentSize = new Terminal.Gui.Size(200, y + 2);



            
            #region BUTTON EVENTS

            filesDialogBtn.Clicked += () =>
            {
                string? path = DialogHelpers.ShowSaveFileDialog(Resources.Selectfolder, Resources.Selectthefolderthatcontainsyourfiles, new string[] { "" });
                if (!string.IsNullOrWhiteSpace(path))
                {
                    folderPath.Text = path;
                }
            };

            outputFolderDialogBtn.Clicked += () =>
            {
                string? path = DialogHelpers.ShowSaveFileDialog(Resources.Selectoutputfile, Resources.Selecttheoutputtorrentfilepath, [".torrent"]);
                if (!string.IsNullOrWhiteSpace(path))
                {
                    // Get directory from full path
                    outputPath.Text = path;
                }
            };  


            //On click
            createTorBtn.Clicked += async() =>
            {
                #region VALIDATION

                //start validation
                string inputPath = folderPath.Text.ToString()!.Trim();
                string outPath = outputPath.Text.ToString()!.Trim();
                string trackers = trackerLink.Text.ToString()!.Trim();

                // Check file/folder exists
                if (string.IsNullOrWhiteSpace(inputPath) ||
                    (!File.Exists(inputPath) && !Directory.Exists(inputPath)))
                {
                    MessageBox.ErrorQuery(Resources.Error, Resources.Invalidfile_folderpath, Resources.OK);
                    return;
                }

                // Check output directory
                if (string.IsNullOrWhiteSpace(outPath))
                {
                    MessageBox.ErrorQuery(Resources.Error, Resources.Outputpathdoesnotexist, Resources.OK);
                    return;
                }




                // Validate trackers (optional)
                List<string> trackerList = new();
                if (!string.IsNullOrWhiteSpace(trackers))
                {
                    foreach (var line in trackers.Split('\n', StringSplitOptions.RemoveEmptyEntries))
                    {
                        string trimmed = line.Trim();
                        if (Uri.IsWellFormedUriString(trimmed, UriKind.Absolute))
                            trackerList.Add(trimmed);
                        else
                        {
                            MessageBox.ErrorQuery(Resources.Error, $"Invalid tracker URL:\n{trimmed}", "OK");
                            return;
                        }
                    }
                }

                int selectedPieceSize = pieceSizes[new List<string>(pieceSizes.Keys)[pieceSizeCombo.SelectedItem]];

                #endregion

                //Create
                try
                {

                    TorrentData settings = new TorrentData
                    {
                        UseMagnetLink = false,
                        TorPath = inputPath,
                        DownPath = outPath,
                        Trackers = trackerList,
                        PieceSize = selectedPieceSize,
                        StartSeedingAfterCreation = startSeedingAfterCreationCheckbox.Checked,
                        IsPrivate = privateTorrentCheckbox.Checked,
                        Comment = comment.Text.ToString() ?? "",
                        Publisher = publisher.Text.ToString() ?? ""
                    };

                    await Task.Run(async () =>
                    {
                        try
                        {
                            await TorrentOperations.CreateTorrent(settings);
                        }
                        catch (Exception ex)
                        {
                            Application.MainLoop.Invoke(() =>
                                MessageBox.ErrorQuery(Resources.Error, $"Torrent creation failed:\n{ex.Message}", "OK")
                            );
                        }
                    });

                    MessageBox.Query("Success", "Torrent created successfully!", "OK");
                }
                catch (Exception ex)
                {
                    MessageBox.ErrorQuery(Resources.Error, $"Unexpected error:\n{ex.Message}", "OK");
                }
            };

            #endregion

        }
    }
}