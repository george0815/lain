using MonoTorrent.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

            //Scroll view           
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


            #region PATHS

            //Folder/file path
            scroll.Add(new Label("File/Folder:") { X = 1, Y = y });
            var folderPath = new TextField("") { X = 20, Y = y, Width = 40 };
            scroll.Add(folderPath);
            y += 2;

            //Output path
            scroll.Add(new Label("Output path:") { X = 1, Y = y });
            var outputPath = new TextField("") { X = 20, Y = y, Width = 40 };
            scroll.Add(outputPath);
            y += 2;

            #endregion


            #region MISC OPTIONS

            //Trackers
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



            //Piece size
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
                Width = 14,
                ReadOnly = true,
                Height = pieceSizes.Count
            };
            pieceSizeCombo.SetSource(new List<string>(pieceSizes.Keys));
            pieceSizeCombo.SelectedItem = 0;
            scroll.Add(pieceSizeCombo);
            y += 2;

            #endregion


            #region CHECKBOXES

            //Checkboxes
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

            #endregion


            #region METADATA

            //Name
            scroll.Add(new Label("Name:") { X = 1, Y = y });
            var name = new TextField("") { X = 20, Y = y, Width = 40 };
            scroll.Add(name);
            y += 2;

            //Publisher
            scroll.Add(new Label("Publisher:") { X = 1, Y = y });
            var publisher = new TextField("") { X = 20, Y = y, Width = 40 };
            scroll.Add(publisher);
            y += 2;

            //Comment
            scroll.Add(new Label("Comment:") { X = 1, Y = y });
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




            //Create button
            var createTorBtn = new Button("Create") { X = 1, Y = y };
            scroll.Add(createTorBtn);
            y += 2;



            //Scroll content size
            scroll.ContentSize = new Terminal.Gui.Size(200, y + 5);



            //Create button action
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
