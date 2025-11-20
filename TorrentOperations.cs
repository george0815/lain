using MonoTorrent;
using MonoTorrent.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace lain
{
    internal class TorrentOperations
    {

        ClientEngine engine;





        internal List<TorrentManager>? managers { get; set; }


        internal TorrentOperations()
        {


            engine = new ClientEngine(Settings.EngineSettings!.ToSettings());

        }


        internal void DeleteTorrent(short index)
        {

            //pause torrent 

            //stop seeding 

            //remove from list 




        }

        internal void CreateTorrent(string? trackerUrl, List<FileInfo> files, string? magnetlink, short port)
        {

            //create torrent 


            //start seeding 



        }




        internal void PauseTorrent(short index)
        {

            //stop leeching       

            //write progress to disk


        }

        internal void ResumeTorrent(short index)
        {

            //load progress from disk 

            //start seeding 

        }



        internal async Task AddTorrent(string downPath, string torPath)
        {


            Torrent torrent = await Torrent.LoadAsync(torPath);
            var manager = await engine.AddAsync(torrent, downPath);

            managers.Add(manager);





            manager.TorrentStateChanged += (o, e) =>
            {

                //update TUI

                Log.log.Add($"State changed: {e.OldState} -> {e.NewState}");

            };

            manager.PieceHashed += (o, e) =>
            {

                //update TUI

                //update log
                Log.log.Add($"Piece hashed: {e.PieceIndex} - {e.HashPassed}");


            };

            Log.log.Add("Downloading... Press any key to exit.");


            manager.StartAsync().GetAwaiter().GetResult();




            /*

            TODO move this to torrent list UI codebehind and have it update the TUI rather than write to console 

            while (manager.State != TorrentState.Stopped)
            {
                Console.WriteLine(

                    $"Progress: {manager.Progress:0.00}% - " +
                    $"Download Speed: {manager.Monitor.DownloadRate / 1024:0.00} kB/s - " +
                    $"Upload Speed: {manager.Monitor.UploadRate / 1024:0.00} kB/s - "

                    );

                await Task.Delay(1000);

            }
            */


        }

    }
}