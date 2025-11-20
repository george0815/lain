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


            Add(new Label("File/Folder:") { X = 1, Y = 1 });
            Add(new TextField("") { X = 15, Y = 1, Width = 40 });

            Add(new Label("Trackers:") { X = 1, Y = 3 });
            Add(new TextView() { X = 15, Y = 3, Width = 40, Height = 5 });

            Add(new Label("Piece Size:") { X = 1, Y = 9 });
            Add(new TextField("16384") { X = 15, Y = 9, Width = 10 });

            Add(new Button("Create Torrent") { X = 1, Y = 11 });

        }

    }
}
