using MonoTorrent.Client;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace lain
{




    internal class Settings
    {

        #region Properties

        internal ushort Port { get; set; }

        internal ushort DhtPort { get; set; }

        internal ushort MaxSeedersPerTorrent { get; set; }
        internal ushort MaxConnections { get; set; }

        internal ushort MaxLeechersPerTorrent { get; set; }

        internal int MaxDownloadSpeed { get; set; }

        internal int MaxUploadSpeed { get; set; }

        internal bool EnableDht { get; set; }

        internal bool EnablePortForwarding { get; set; }
        internal string? DefaultDownloadPath { get; set; }

        internal EngineSettingsBuilder? EngineSettings { get; set; }

        #endregion

        internal Settings(ushort port = 55123, ushort dhtport = 55124, bool allowportforwarding = true)
        {
            Port = port;
            DhtPort = dhtport;
            MaxSeedersPerTorrent = 50;
            MaxConnections = 200;
            MaxLeechersPerTorrent = 50;
            MaxDownloadSpeed = 100;
            EnablePortForwarding = allowportforwarding;
            MaxUploadSpeed = 0;
            DefaultDownloadPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + System.IO.Path.DirectorySeparatorChar + "Downloads";
            
            EngineSettings = new EngineSettingsBuilder
            {
                AllowPortForwarding = EnablePortForwarding,
                ListenEndPoints = new Dictionary<string, IPEndPoint> { { "main", new IPEndPoint(IPAddress.Any, port) } },
                DhtEndPoint = new System.Net.IPEndPoint(System.Net.IPAddress.Any, DhtPort),
           
              
                

            };
        }







    }
}
