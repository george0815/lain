using System;
using System.Collections.Generic;
using System.Text;
using Terminal.Gui;

namespace lain.frameviews
{
    internal class TorrentListView : FrameView
    {

        public TorrentListView()
            : base("Torrents")
        {


            X = 20;
            Y = 3;
            Width = Dim.Fill();
            Height = Dim.Fill();

            List<string> ActiveTorrents = new List<string>()
             {
                "Ubuntu ISO",
                "Arch Linux ISO",
                "Fedora ISO"
            };


            var list = new ListView(ActiveTorrents)
            {
                X = 0,
                Y = 0
            };

            Add(list);

            // Pause/resume with key P
            list.KeyPress += (e) =>
            {
                if (e.KeyEvent.Key == Key.P)
                {
                    MessageBox.Query("Torrent", "Pause/Resume requested", "OK");
                    e.Handled = true;
                }
            };

            
        }
    }
}
