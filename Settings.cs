using System;
using System.Collections.Generic;
using System.Text;
using MonoTorrent.Client;

namespace lain
{




    internal class Settings
    {

        #region Properties

        internal ushort Port { get; set; }

        internal ushort DhtPort { get; set; }

        internal ushort MaxSeedersPerTorrent { get; set; }
        internal ushort MaxPeersPerTorrent { get; set; }

        internal ushort MaxLeechersPerTorrent { get; set; }

        internal int MaxDownloadSpeed { get; set; }

        internal int MaxUploadSpeed { get; set; }
        internal string? DefaultDownloadPath { get; set; }

        private EngineSettingsBuilder? EngineSettings { get; set; }

        #endregion






    }
}
