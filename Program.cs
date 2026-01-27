using lain.helpers;
using MonoTorrent;
using MonoTorrent.Client;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Terminal.Gui;

namespace lain
{
    /// <summary>
    /// Entry point for the Lain application.
    /// Initializes settings, plugins, and starts the Terminal.Gui interface.
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


                Settings.Load();
                Ghidorah.LoadQbittorrentPlugins();

                string command = args[0].ToLowerInvariant();

                switch (command)
                {
                    // ------------------------------
                    // Check status
                    // ------------------------------
                    case "--status":
                        {
                            Console.WriteLine($"{Resources.Checkingstatus}...");

                            await Task.Run(() =>
                            {
                                try
                                {
                                    var result = Ghidorah.CheckStatusPlugins(true);
                                    Console.WriteLine(result);
                                }
                                catch (Exception ex) { Console.WriteLine(ex.Message); }
                            });

                            break;
                        }



                    // ------------------------------
                    // Download torrent / magnet
                    // ------------------------------
                    case "--download":
                        {

                            //Invalid arg length
                            if (args.Length != 3) { PrintUsageAndExit(); break; }

                            string input = args[2];
                            string flag = args[1];

                            //Invalid flags
                            if (flag != "-M" && flag != "-F") { PrintUsageAndExit(); break; }


                            //Invlaid torrent/magnet
                            if (!input.StartsWith("magnet:?") && flag == "-M")
                            {
                                Console.WriteLine($"{Resources.Error}: {Resources.Thisdoesnotappeartobeavalidmagnetlink}");
                                break;
                            }
                            if (flag == "-F" && !File.Exists(input))
                            {
                                Console.WriteLine($"{Resources.Error}: {Resources.Torrentfiledoesnotexist}");
                                break;
                            }

                            Console.WriteLine($"Starting download: {input}");

                            // Build settings object 
                            TorrentData settings = new()
                            {
                                UseMagnetLink = flag == "-M",
                                MagnetUrl = input,
                                TorPath = input,
                                DownPath = Settings.Current.DefaultDownloadPath!,
                                MaxConnections = Settings.Current.MaxConnections,
                                MaxDownloadRate = Settings.Current.MaxDownloadSpeed,
                                MaxUploadRate = Settings.Current.MaxUploadSpeed,
                                UseDht = true
                            };

                            var cts = new CancellationTokenSource();

                            TorrentOperations.UpdateProgress += PrintStatus;

                            try
                            {
                                Task downloadTask = TorrentOperations.AddTorrent(settings, false, false);

                                await downloadTask; // This just starts the torrent, doesn't wait for completion

                                Console.WriteLine("Download started, waiting for completion...");

                                // Wait for all torrents to finish
                                await WaitForTorrentsAsync();

                                cts.Cancel();

                                Console.WriteLine("All downloads complete!");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error: {ex.Message}");
                            }





                            break;
                        }

                    // ------------------------------
                    // Create torrent
                    // ------------------------------
                    case "--create":
                        {


                            //Invalid arg length
                            if (args.Length != 4) { PrintUsageAndExit(); break; }

                            string inputPath = args[1];
                            string outPath = args[2];
                            string trackers = args[3];

                            // Validate input file/folder path.
                            if (string.IsNullOrWhiteSpace(inputPath) ||
                                (!File.Exists(inputPath) && !Directory.Exists(inputPath)))
                            {
                                Console.WriteLine($"{Resources.Error}: {Resources.Invalidfile_folderpath}");
                                return;
                            }

                            // Validate output path.
                            if (string.IsNullOrWhiteSpace(outPath))
                            {
                                Console.WriteLine($"{Resources.Error}: {Resources.Outputpathdoesnotexist}");
                                return;
                            }

                            // Parse and validate tracker URLs (optional).
                            List<string> trackerList = [];
                            if (!string.IsNullOrWhiteSpace(trackers))
                            {
                                foreach (var line in trackers.Split('\n', StringSplitOptions.RemoveEmptyEntries))
                                {
                                    string trimmed = line.Trim();
                                    if (Uri.IsWellFormedUriString(trimmed, UriKind.Absolute))
                                        trackerList.Add(trimmed);
                                    else
                                    {
                                        Console.WriteLine($"{Resources.Error}: {Resources.InvalidtrackerURL} - {trimmed}");
                                        return;
                                    }
                                }
                            }




                            // -------------------
                            // Torrent creation
                            // -------------------
                            try
                            {
                                TorrentData settings = new()
                                {
                                    UseMagnetLink = false,
                                    TorPath = inputPath,
                                    DownPath = outPath,
                                    Trackers = trackerList,
                                    PieceSize = 512 * 1024,
                                    StartSeedingAfterCreation = true,
                                    IsPrivate = false,

                                };

                                // Run torrent creation off the UI thread.
                                await Task.Run(async () =>
                                {
                                    try
                                    {
                                        await TorrentOperations.CreateTorrent(settings);
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"{Resources.Torrentcreationfailed}\n{ex.Message}");
                                    }
                                });

                                Console.WriteLine(Resources.Torrentcreatedsuccessfully_);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"{Resources.Torrentcreationfailed}\n{ex.Message}");
                            }
                        
                       
                        break;
                
                        }



                    // ------------------------------
                    // Search
                    // ------------------------------
                    case "--search":
                        {

                            //Invalid arg length
                            if (args.Length != 2) { PrintUsageAndExit(); break; }



                            // Build Ghidorah search arguments from current settings
                            SearchArgs searchArgs = new()
                            {
                                Query = args[1] ?? "",
                                Limit = Settings.Current.SearchResultsLimitPerSource,
                                TotalLimit = Settings.Current.SearchResultsLimit,
                                Sources = Settings.Current.SearchSources,
                                Categories = Settings.Current.Categories,
                                SortBy = Settings.Current.SortBy
                            };

                            Console.WriteLine($"{Resources.Searching}...");


                            // Perform the search in the background
                            await Task.Run(() =>
                            {
                                try
                                {
                                    var res = Ghidorah.Search(searchArgs);

                                    Console.WriteLine(res);
                                    SaveToJson(res);
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine("Results saved to JSON.");
                                    Console.ResetColor();
                                }
                                catch (Exception ex) { Console.WriteLine(ex.Message); }

                            });
                            break;
                        }



                    // ------------------------------
                    // Help / unknown command
                    // ------------------------------
                    default:
                        {
                            PrintUsageAndExit();
                            break;
                        }
                }

                return;
            }


            else
            {

                // ------------------------------
                // Optional debug: change culture to Japanese
                // ------------------------------
                CultureInfo ci = new("ja-JP");
                //Thread.CurrentThread.CurrentUICulture = ci;

                // Register encodings for legacy code pages (e.g., Shift-JIS)
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                // Enable UTF-8
                Application.UseSystemConsole = true;
                Console.InputEncoding = Encoding.UTF8;
                Console.OutputEncoding = Encoding.UTF8;


                // ------------------------------
                // Load settings and plugins
                // ------------------------------
                Settings.Load(); // load user settings from file
                Ghidorah.LoadQbittorrentPlugins(); // load qbittorrent plugin sources

                // Disable QB plugin usage if none loaded
                if (Ghidorah.QbSources == null || Ghidorah.QbSources.Length == 0)
                {
                    Settings.Current.UseQbittorrentPlugins = false;
                }

                // ------------------------------
                // Initialize Terminal.Gui
                // ------------------------------
                Application.Init();

                // Top-level container for windows
                var top = Application.Top;

                // Main window (contains header, sidebar, and content panels)
                var mainWin = new LainUI();

                // ------------------------------
                // Setup color scheme for the main window
                // ------------------------------
                ColorScheme myScheme = new()
                {
                    Normal = Application.Driver.MakeAttribute(Settings.Current.TextColor, Settings.Current.BackgroundColor), // normal text
                    Focus = Application.Driver.MakeAttribute(Settings.Current.FocusTextColor, Settings.Current.FocusBackgroundColor), // focused element
                    HotNormal = Application.Driver.MakeAttribute(Settings.Current.HotTextColor, Settings.Current.BackgroundColor), // hotkey text
                    HotFocus = Application.Driver.MakeAttribute(Settings.Current.FocusTextColor, Settings.Current.FocusBackgroundColor), // focused hotkey
                };

                mainWin.ColorScheme = myScheme;

                // Add main window to the top-level container
                top.Add(mainWin);

                // ------------------------------
                // Run the application
                // ------------------------------
                try
                {
                    Application.Run(); // blocks until user exits
                }
                finally
                {
                    // ------------------------------
                    // Cleanup / persist data
                    // ------------------------------
                    Log.Save(); // save logs to file

                    // Shutdown Terminal.Gui
                    Application.Shutdown();

                    // Restore terminal state if on Linux or macOS
                    if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
                    {
                        Console.Write("\x1b[0m");      // reset attributes
                        Console.Write("\x1b[?25h");   // show cursor
                        Console.Write("\x1b[?1049l"); // leave alt screen
                        Console.Out.Flush();
                    }

                }
            }


            // ------------------------------
            // Helper functions
            // ------------------------------


            // Waits for all active torrents to complete downloading
            static async Task WaitForTorrentsAsync() {


                while (TorrentOperations.Managers.Count > 0)
                {
                    bool allDone = true;

                    foreach (var m in TorrentOperations.Managers)
                    {
                        if (m == null) continue;

                        // Still downloading if state is Downloading or Metadata
                        if (m.State == TorrentState.Downloading || m.State == TorrentState.Metadata)
                        {
                            allDone = false;
                            break;
                        }
                    }

                    if (allDone) break;

                    await Task.Delay(1000); // Wait 1s before checking again
                }



            }

            // Prints usage instructions and exits the application
            static void PrintUsageAndExit()
            {
                Console.WriteLine("----------------------------USAGE----------------------------");
                Console.WriteLine("Download: lain --download <-F | -M> <torrent file | magnet link>");
                Console.WriteLine("Create: lain --create <folder/file path> <output path> <tracker links>");
                Console.WriteLine("Search: lain --search <query>");
                Console.WriteLine("Check status: lain --status");
                Console.WriteLine("-------------------------------------------------------------");


                Environment.ExitCode = 1;
            }

            //Saves search results to a JSON file
            static void SaveToJson(string res)
            {
                try
                {
                    using JsonDocument doc = JsonDocument.Parse(res);

                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        
                    };
                    
                    string jsonString = JsonSerializer.Serialize(doc.RootElement, options);
                    File.WriteAllText("search_results.json", jsonString);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{Resources.Error}: {ex.Message}");
                }
            }




            //updates status of torrent download in CLI mode
            static void PrintStatus()
            {
                try
                {              
                        if (Log.LogList.Count > 0 )
                        {
                            Console.WriteLine(Log.LogList[^1]); // latest entry
                        }    
                }
                catch (TaskCanceledException) {} // Expected when cancelled — safe to ignore
            
            
            }



        }
    }
}
