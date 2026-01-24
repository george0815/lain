using System;
using System.Collections.Generic;
using System.Text;
using Terminal.Gui;

namespace lain.helpers
{
    /// <summary>
    /// Represents a set of configurable hotkeys for torrent operations.
    /// This struct holds the key mappings for starting/stopping downloads,
    /// starting/stopping seeding, removing torrents, and generating magnet links.
    /// </summary>
    public struct TorrentHotkeys
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TorrentHotkeys"/> struct.
        /// The default keys are assigned automatically via property initializers.
        /// </summary>
        public TorrentHotkeys() { }

        /// <summary>
        /// The key used to start/resume a torrent download.
        /// Default is F3.
        /// </summary>
        public Key StartDownload { get; set; } = Key.F3;

        /// <summary>
        /// The key used to pause a torrent download.
        /// Default is F4.
        /// </summary>
        public Key StopDownload { get; set; } = Key.F4;

        /// <summary>
        /// The key used to start seeding a torrent.
        /// Default is F5.
        /// </summary>
        public Key StartSeeding { get; set; } = Key.F5;

        /// <summary>
        /// The key used to stop seeding a torrent.
        /// Default is F6.
        /// </summary>
        public Key StopSeeding { get; set; } = Key.F6;

        /// <summary>
        /// The key used to remove a torrent from the client.
        /// Default is F7.
        /// </summary>
        public Key RemoveTorrent { get; set; } = Key.F7;

        /// <summary>
        /// The key used to generate and copy a torrent's magnet link.
        /// Default is F8.
        /// </summary>
        public Key GenMagLink { get; set; } = Key.F8;
    }
}
