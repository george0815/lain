using MonoTorrent.Client;
using System;
using System.Collections.Generic;
using System.Text;
using Terminal.Gui;

namespace lain.frameviews
{
    internal class CreateView : FrameView
    {

        public CreateView()
            : base("Create")
        {

            X = 20;
            Y = 3;
            Width = Dim.Fill();
            Height = Dim.Fill();


            var folderLabel = new Label("File/Folder:") { X = 1, Y = 1 };
            Add(folderLabel);

            var folderPath = new TextField("") { X = 15, Y = 1, Width = 40 };
            Add(folderPath);


            var trackersLabel = new Label("Trackers:") { X = 1, Y = 3 };
            Add(trackersLabel);

            var trackerLink = new TextView() { X = 15, Y = 3, Width = 40, Height = 5 };
            Add(trackerLink);

            var pieceSizeLabel = new Label("Piece Size:") { X = 1, Y = 9 };
            Add(pieceSizeLabel);
            var pieceSize = new TextField("16384") { X = 15, Y = 9, Width = 10 };
            Add(pieceSize);

            var createTorBtn = new Button("Create Torrent") { X = 1, Y = 11 };
            Add(createTorBtn);


            createTorBtn.Clicked += async () =>
            {


                _ = Task.Run(async () =>
                {
                    await TorrentOperations.CreateTorrent(
                        Settings.DefaultDownloadPath!,
                        folderPath.Text.ToString()!,
                        trackerLink.Text.ToString()!
                    );
                });

                MessageBox.Query("Create", "Torrent created.", "OK");

            };


        }

    }
}
