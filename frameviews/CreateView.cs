using lain;
using MonoTorrent.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Xml.Linq;
using lain.helpers;
using Terminal.Gui;
using TextCopy;

namespace lain.frameviews
{
    /// <summary>
    /// UI view responsible for creating new torrent files.
    ///
    /// This view provides:
    /// - Input/output path selection
    /// - Tracker configuration
    /// - Piece size selection
    /// - Torrent metadata (publisher/comment)
    /// - Optional flags such as private torrent and auto-seeding
    ///
    /// All logic here is UI orchestration and validation; the actual
    /// torrent creation is delegated to TorrentOperations.
    /// </summary>
    internal class CreateView : FrameView
    {
        public CreateView()
            : base(Resources.Create)
        {
            // Position and size the frame relative to the application layout.
            X = 20;
            Y = SettingsData.HeaderHeight;
            Width = Dim.Fill();
            Height = Dim.Fill();

            // ScrollView is used to avoid layout overflow as controls stack vertically.
            var scroll = new ScrollView()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                ShowVerticalScrollIndicator = true,
            };

            Add(scroll);

            // Vertical cursor used to place controls top-to-bottom.
            int y = 1;

            #region PATHS
            // -------------------
            // Input / Output paths
            // -------------------

            scroll.Add(new Label(Resources.File_Folder) { X = 1, Y = y });
            var folderPath = new TextField("") { X = 20, Y = y, Width = 40 };
            scroll.Add(folderPath);

            // Button opens a file/folder picker dialog.
            var filesDialogBtn = new Button("...") { X = 61, Y = y };
            scroll.Add(filesDialogBtn);
            y += 2;

            scroll.Add(new Label(Resources.Outputpath) { X = 1, Y = y });
            var outputPath = new TextField("") { X = 20, Y = y, Width = 40 };
            scroll.Add(outputPath);

            // Button opens a save dialog for the output .torrent file.
            var outputFolderDialogBtn = new Button("...") { X = 61, Y = y };
            scroll.Add(outputFolderDialogBtn);
            y += 2;

            #endregion

            #region TRACKERS
            // -------------------
            // Tracker configuration
            // -------------------

            scroll.Add(new Label(Resources.Trackers) { X = 1, Y = y });

            // Multi-line text view allows one tracker URL per line.
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
            // -------------------
            // Piece size selection
            // -------------------

            scroll.Add(new Label(Resources.Piecesize) { X = 1, Y = y });

            // Human-readable piece sizes mapped to their byte values.
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

            // Read-only ComboBox ensures users can only select valid sizes.
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
            // -------------------
            // Optional torrent flags
            // -------------------

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
            // -------------------
            // Optional metadata fields
            // -------------------

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

            // Primary action button that triggers torrent creation.
            var createTorBtn = new Button(Resources.Create) { X = 1, Y = y };
            scroll.Add(createTorBtn);

            // Ensure the ScrollView knows the total virtual content height.
            scroll.ContentSize = new Terminal.Gui.Size(200, y + 2);

            #region BUTTON EVENTS
            // -------------------
            // Dialog and action handlers
            // -------------------

            filesDialogBtn.Clicked += () =>
            {
                string? path = DialogHelpers.ShowSaveFileDialog(
                    Resources.Selectfolder,
                    Resources.Selectthefolderthatcontainsyourfiles,
                    [""]
                );

                if (!string.IsNullOrWhiteSpace(path))
                {
                    folderPath.Text = path;
                }
            };

            outputFolderDialogBtn.Clicked += () =>
            {
                string? path = DialogHelpers.ShowSaveFileDialog(
                    Resources.Selectoutputfile,
                    Resources.Selecttheoutputtorrentfilepath,
                    [".torrent"]
                );

                if (!string.IsNullOrWhiteSpace(path))
                {
                    outputPath.Text = path;
                }
            };

            // Main torrent creation workflow.
            createTorBtn.Clicked += async () =>
            {
                #region VALIDATION
                // -------------------
                // Input validation
                // -------------------

                string inputPath = folderPath.Text.ToString()!.Trim();
                string outPath = outputPath.Text.ToString()!.Trim();
                string trackers = trackerLink.Text.ToString()!.Trim();

                // Validate input file/folder path.
                if (string.IsNullOrWhiteSpace(inputPath) ||
                    (!File.Exists(inputPath) && !Directory.Exists(inputPath)))
                {
                    MessageBox.ErrorQuery(Resources.Error, Resources.Invalidfile_folderpath, Resources.OK);
                    return;
                }

                // Validate output path.
                if (string.IsNullOrWhiteSpace(outPath))
                {
                    MessageBox.ErrorQuery(Resources.Error, Resources.Outputpathdoesnotexist, Resources.OK);
                    return;
                }

                // Parse and validate tracker URLs (optional).
                List<string> trackerList = [];
                if (!string.IsNullOrWhiteSpace(trackers))
                {
                    foreach (var line in trackers.Split('\n', StringSplitOptions.RemoveEmptyEntries))
                    {
                        string trimmed = line.Trim();
                        if (Uri.IsWellFormedUriString(trimmed, UriKind.Absolute))
                            trackerList.Add(trimmed);
                        else
                        {
                            MessageBox.ErrorQuery(
                                Resources.Error,
                                $"{Resources.InvalidtrackerURL}\n{trimmed}",
                                Resources.OK
                            );
                            return;
                        }
                    }
                }

                // Resolve the selected piece size from the ComboBox index.
                int selectedPieceSize =
                    pieceSizes[new List<string>(pieceSizes.Keys)[pieceSizeCombo.SelectedItem]];

                #endregion

                // -------------------
                // Torrent creation
                // -------------------
                try
                {
                    TorrentData settings = new()
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

                    // Run torrent creation off the UI thread.
                    await Task.Run(async () =>
                    {
                        try
                        {
                            await TorrentOperations.CreateTorrent(settings);
                        }
                        catch (Exception ex)
                        {
                            // Marshal UI updates back onto the main loop.
                            Application.MainLoop.Invoke(() =>
                                MessageBox.ErrorQuery(
                                    Resources.Error,
                                    $"{Resources.Torrentcreationfailed}\n{ex.Message}",
                                    Resources.OK
                                )
                            );
                        }
                    });

                    MessageBox.Query(Resources.Success, Resources.Torrentcreatedsuccessfully_, Resources.OK);
                }
                catch (Exception ex)
                {
                    MessageBox.ErrorQuery(
                        Resources.Error,
                        $"{Resources.Unexpectederror}\n{ex.Message}",
                        Resources.OK
                    );
                }
            };

            #endregion
        }
    }
}
