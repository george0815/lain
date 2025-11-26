using System;
using System.Collections.Generic;
using System.Text;
using Terminal.Gui;

namespace lain.helpers
{
    public struct TorrentHotkeys
    {
        public Key StartDownload { get; set; }
        public Key StopDownload { get; set; }
        public Key RemoveTorrent { get; set; }
        public Key StartSeeding { get; set; }
        public Key StopSeeding { get; set; }
    }
}
