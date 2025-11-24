using MonoTorrent.Client;
using System;
using System.Collections.Generic;
using Terminal.Gui;

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

            // Scrollable container
            var scroll = new ScrollView()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                ShowVerticalScrollIndicator = true,
                ShowHorizontalScrollIndicator = false,
                ContentSize = new Terminal.Gui.Size(Application.Top.Frame.Width - 20, 30) // adjust height based on number of controls
            };

            // --- Controls ---
            var folderLabel = new Label("File/Folder:") { X = 1, Y = 1 };
            var folderPath = new TextField("") { X = 14, Y = 1, Width = 40 };

            var trackersLabel = new Label("Trackers:") { X = 1, Y = 3 };
            var trackerLink = new TextView() { X = 14, Y = 3, Width = 40, Height = 5 };

            var pieceSizeLabel = new Label("Piece Size:") { X = 1, Y = 9 };
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
                X = 14,
                Y = 9,
                Width = 12,
                ReadOnly = true,
                Height = 9
            };
            pieceSizeCombo.SetSource(new List<string>(pieceSizes.Keys));
            pieceSizeCombo.SelectedItem = 0;

            var startSeedingAfterCreationCheckbox = new CheckBox("Start seeding after creation") { X = 1, Y = 11, Checked = true };
            var privateTorrentCheckbox = new CheckBox("Private") { X = 1, Y = 13, Checked = false };

            var nameLabel = new Label("Name:") { X = 1, Y = 15 };
            var name = new TextField("") { X = 14, Y = 15, Width = 40 };

            var publisherLabel = new Label("Publisher:") { X = 1, Y = 17 };
            var publisher = new TextField("") { X = 14, Y = 17, Width = 40 };

            var commentLabel = new Label("Comment:") { X = 1, Y = 19 };
            var comment = new TextView() { X = 14, Y = 19, Width = 40, Height = 5 };

            var outputPathLabel = new Label("Output path:") { X = 1, Y = 25 };
            var outputPath = new TextField("") { X = 14, Y = 25, Width = 40 };

            var createTorBtn = new Button("Create") { X = 1, Y = 27 };

            // Add controls to scroll view
            scroll.Add(
                folderLabel, folderPath,
                trackersLabel, trackerLink,
                pieceSizeLabel, pieceSizeCombo,
                startSeedingAfterCreationCheckbox, privateTorrentCheckbox,
                nameLabel, name,
                publisherLabel, publisher,
                commentLabel, comment,
                createTorBtn, outputPath, outputPathLabel
            );

            // Add scroll view to FrameView
            Add(scroll);

            // --- Event handler ---
            createTorBtn.Clicked += async () =>
            {
                _ = Task.Run(async () =>
                {
                    await TorrentOperations.CreateTorrent(
                        folderPath.Text.ToString()!,
                        Settings.Current.DefaultDownloadPath!,
                        trackerLink.Text.ToString()!
                    );
                });

                MessageBox.Query("Create", "Torrent created.", "OK");
            };
        }
    }
}
