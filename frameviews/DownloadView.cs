using System;
using System.Collections.Generic;
using System.Text;
using Terminal.Gui;

namespace lain.frameviews
{
    internal class DownloadView : FrameView
    {

        public DownloadView()
            : base("Download")
        {


            X = 20;
                Y = 3;
                Width = Dim.Fill();
                Height = Dim.Fill();
          

            var magnetLabel = new Label("Magnet URL:")
            {
                X = 1,
                Y = 1
            };

            var magnetInput = new TextField("")
            {
                X = 15,
                Y = 1,
                Width = 40
            };

            var fileLabel = new Label("Torrent File Path:")
            {
                X = 1,
                Y = 3
            };

            var fileInput = new TextField("")
            {
                X = 15,
                Y = 3,
                Width = 40
            };

            var downloadBtn = new Button("Download")
            {
                X = 1,
                Y = 5
            };

            Add(magnetLabel, magnetInput, fileLabel, fileInput, downloadBtn);



            downloadBtn.Clicked += async () =>
            {

                _ = Task.Run(async () =>
                {
                    await TorrentOperations.AddTorrent(
                        Settings.DefaultDownloadPath!,
                        fileInput.Text.ToString()!
                    );
                });

                MessageBox.Query("Download", "Torrent download started.", "OK");

            };



        }
    }
}
