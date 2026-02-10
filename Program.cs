using lain.protocol;
using lain.protocol.dto;
using System;
using System.Diagnostics;
using System.Text;

namespace lain
{
    /// <summary>
    /// Entry point for the Lain application.
    /// The main branch uses MonoTorrent, which is a C# bittorrent library, however I wanted
    /// to try rolling my own BitTorrent protocol implementation 
    /// so I created this branch separately. I mainly made this for
    /// research/learning purposes, so comments will be more common,
    /// and more thorough as I feel explaining the program to myself as
    /// I write it helps me get a deeper understanding. 
    ///
    /// The original idea comes from Sean O'Flynn's 
    /// "Building a BitTorrent client from scratch in C#", so big thanks to him. 
    /// </summary>
    /// <author>George Hunter S.</author>
    /// <created>Jan, 2026</created>

    class Program
    {
        public static async Task Main(string[] args)
        {

            // ------------------------------
            // CLI Entry point
            // ------------------------------
            if (args.Length > 0)
            {



                string command = args[0].ToLowerInvariant();

                switch (command)
                {

                    // ------------------------------
                    // Test parsing torrent file
                    // ------------------------------

                    case "--parse":
                        {


                            try
                            {

                          

                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error parsing torrent: {ex.Message}");
                            }





                            break;
                        }

                }

                return;
            }


           



        }
    }
}
