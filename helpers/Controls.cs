using System;
using System.Collections.Generic;
using System.Text;
using Terminal.Gui;

namespace lain.helpers
{
    public struct TorrentHotkeys
    {
        public TorrentHotkeys()
        {
        }

        public Key StartDownload { get; set; } = Key.F3;
        public Key StopDownload { get; set; } = Key.F4;
        public Key RemoveTorrent { get; set; } = Key.F5;
        public Key StartSeeding { get; set; } = Key.F6;
        public Key StopSeeding { get; set; } = Key.F7;
    }
}
