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
            : base("Create")
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

            scroll.Add(new Label("File/Folder:") { X = 1, Y = y });
            var folderPath = new TextField("") { X = 20, Y = y, Width = 40 };
            scroll.Add(folderPath);
            var filesDialogBtn = new Button("...") { X = 61, Y = y };
            scroll.Add(filesDialogBtn);
            y += 2;

            

            scroll.Add(new Label("Output path:") { X = 1, Y = y });
            var outputPath = new TextField("") { X = 20, Y = y, Width = 40 };
            scroll.Add(outputPath);
            var outputFolderDialogBtn = new Button("...") { X = 61, Y = y };
            scroll.Add(outputFolderDialogBtn);
            y += 2;

            #endregion

            #region TRACKERS

            scroll.Add(new Label("Trackers:") { X = 1, Y = y });
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

            scroll.Add(new Label("Piece Size:") { X = 1, Y = y });

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

            var startSeedingAfterCreationCheckbox = new CheckBox("Start seeding after creation")
            {
                X = 1,
                Y = y,
                Checked = true
            };
            scroll.Add(startSeedingAfterCreationCheckbox);
            y += 2;

            var privateTorrentCheckbox = new CheckBox("Private")
            {
                X = 1,
                Y = y,
                Checked = false
            };
            scroll.Add(privateTorrentCheckbox);
            y += 2;


            var generateMagnetURLCheckbox = new CheckBox("Generate magnet link")
            {
                X = 1,
                Y = y,
                Checked = false
            };
            scroll.Add(generateMagnetURLCheckbox);
            y += 2;

            #endregion

            #region METADATA

            scroll.Add(new Label("Name:") { X = 1, Y = y });
            var name = new TextField("") { X = 20, Y = y, Width = 40 };
            scroll.Add(name);
            y += 2;

            scroll.Add(new Label("Publisher:") { X = 1, Y = y });
            var publisher = new TextField("") { X = 20, Y = y, Width = 40 };
            scroll.Add(publisher);
            y += 2;

            scroll.Add(new Label("Comment:") { X = 1, Y = y });
            var comment = new TextView()
            {
                X = 20,
                Y = y,
                Width = 40,
                Height = 5
            };
            scroll.Add(comment);
            y += 6;

            #endregion

            var createTorBtn = new Button("Create") { X = 1, Y = y };
            scroll.Add(createTorBtn);
            y += 2;

            scroll.ContentSize = new Terminal.Gui.Size(200, y + 5);



            // Shows magnet url
            TorrentOperations.MagnetLinkGenerated += (url) =>
            {
                if (MessageBox.Query("Magnet URL Generated",
                        "Magnet link copied to clipboard!",
                        "OK") == 0)
                {
                    ClipboardService.SetText(url);
                }
            };


            #region BUTTON EVENTS

            filesDialogBtn.Clicked += () =>
            {
                string? path = DialogHelpers.ShowSaveFileDialog("Select folder", "Select the folder that contains your files.", new string[] { "" });
                if (!string.IsNullOrWhiteSpace(path))
                {
                    folderPath.Text = path;
                }
            };

            outputFolderDialogBtn.Clicked += () =>
            {
                string? path = DialogHelpers.ShowSaveFileDialog("Select output file", "Select the output .torrent file path.", [".torrent"]);
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
                string torName = name.Text.ToString()!.Trim();

                // Check file/folder exists
                if (string.IsNullOrWhiteSpace(inputPath) ||
                    (!File.Exists(inputPath) && !Directory.Exists(inputPath)))
                {
                    MessageBox.ErrorQuery("Error", "Invalid file/folder path.", "OK");
                    return;
                }

                // Check output directory
                if (string.IsNullOrWhiteSpace(outPath))
                {
                    MessageBox.ErrorQuery("Error", "Output path does not exist.", "OK");
                    return;
                }

                // Check name
                if (string.IsNullOrWhiteSpace(torName))
                {
                    MessageBox.ErrorQuery("Error", "Name cannot be empty.", "OK");
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
                            MessageBox.ErrorQuery("Error", $"Invalid tracker URL:\n{trimmed}", "OK");
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
                        UseMagnetLink = generateMagnetURLCheckbox.Checked,
                        TorPath = inputPath,
                        DownPath = outPath,
                        Trackers = trackerList,
                        PieceSize = selectedPieceSize,
                        StartSeedingAfterCreation = startSeedingAfterCreationCheckbox.Checked,
                        IsPrivate = privateTorrentCheckbox.Checked,
                        Name = torName,
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
                                MessageBox.ErrorQuery("Error", $"Torrent creation failed:\n{ex.Message}", "OK")
                            );
                        }
                    });

                    MessageBox.Query("Success", "Torrent created successfully!", "OK");
                }
                catch (Exception ex)
                {
                    MessageBox.ErrorQuery("Error", $"Unexpected error:\n{ex.Message}", "OK");
                }
            };

            #endregion

        }
    }
}