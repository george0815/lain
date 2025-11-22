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
            Y = Settings.HeaderHeight;
            Width = Dim.Fill();
            Height = Dim.Fill();
          

            var magnetLabel = new Label("Magnet URL:")
            {
                X = 1,
                Y = 1
            };

            Add(magnetLabel);

            var magnetInput = new TextField("")
            {
                X = 20,
                Y = 1,
                Width = 40
            };

            Add(magnetInput);


            var magnetCheckbox = new CheckBox("Use magnet link")
            {
                X = 1,
                Y = 3,
                Checked = false
            };

            Add(magnetCheckbox);



            var fileLabel = new Label("Torrent file path:")
            {
                X = 1,
                Y = 5
            };

            Add(fileLabel);

            var fileInput = new TextField("")
            {
                X = 20,
                Y = 5,
                Width = 40
            };

            Add(fileInput);

            var downloadPathLabel = new Label("Download path:")
            {
                X = 1,
                Y = 7
            };

            Add(downloadPathLabel);

            var downloadPathInput = new TextField("")
            {
                X = 20,
                Y = 7,
                Width = 40
            };

            Add(downloadPathInput);

            var downloadBtn = new Button("Download")
            {
                X = 1,
                Y = 9
            };

            Add(downloadBtn);



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
