using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Terminal.Gui;
using System.Text.Json.Serialization;
using System.Linq;
using System.Threading.Tasks;

namespace lain.helpers
{
    /// <summary>
    /// Represents the JSON response returned by Ghidorah for status queries.
    /// </summary>
    public class StatusResponse
    {
        [JsonPropertyName("paths")]
        public string Paths { get; set; }  // Paths used in Ghidorah execution

        [JsonPropertyName("message")]
        public string Message { get; set; } // Optional message from Ghidorah

        [JsonPropertyName("results")]
        public List<SourceStatus> Results { get; set; } // List of sources and their status
    }

    /// <summary>
    /// Represents the status of a single source/plugin.
    /// </summary>
    public class SourceStatus
    {
        [JsonPropertyName("source")]
        public string Source { get; set; } // Name of the source

        [JsonPropertyName("status")]
        public string Status { get; set; } // ONLINE, ERROR, or UNKNOWN

        [JsonPropertyName("results")]
        public int? Results { get; set; } // Number of results found, if applicable

        [JsonPropertyName("error")]
        public string Error { get; set; } // Error message, if any
    }

    /// <summary>
    /// Struct to hold arguments for a Ghidorah search.
    /// </summary>
    internal struct SearchArgs
    {
        internal string Query;       // Search query
        internal int Limit;          // Number of results per source
        internal int TotalLimit;     // Maximum total results
        internal string[] Sources;   // Sources to search
        internal string[] Categories;// Categories to filter
        internal string SortBy;      // Sorting criteria
    }

    /// <summary>
    /// Handles interaction with the Ghidorah executable for searches and plugin checks.
    /// </summary>
    internal class Ghidorah
    {
        // Determine executable name based on OS
        static string ExeFileName = OperatingSystem.IsWindows() ? "ghidorah.exe" : "ghidorah";

        // Stores sources discovered from qBittorrent plugins
        internal static string[] QbSources { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Loads qBittorrent plugin sources by querying Ghidorah.
        /// </summary>
        internal static void LoadQbittorrentPlugins()
        {
            string result = CheckStatusPlugins(false); // Fetch plugin list
            try
            {
                var plugins = JsonSerializer.Deserialize<List<string>>(result);
                if (plugins != null)
                {
                    QbSources = plugins.ToArray(); // Store plugins as array
                }
            }
            catch (Exception ex)
            {
                Log.Write($"{Resources.Error}: {ex.Message}");
            }
        }

        /// <summary>
        /// Checks plugin or status information from Ghidorah.
        /// </summary>
        /// <param name="status">True to check status, false to check plugins</param>
        /// <returns>Output string from Ghidorah or error message</returns>
        internal static string CheckStatusPlugins(bool status)
        {
            // Configure process to run Ghidorah executable
            var psi = new ProcessStartInfo
            {
                FileName = ExeFileName,
                Arguments = status ? "--check_status" : "--check_plugins",
                RedirectStandardOutput = true,    // Capture stdout
                RedirectStandardError = true,     // Capture stderr
                UseShellExecute = false,          // Run without shell
                CreateNoWindow = true,            // Hide window
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            using var process = new Process { StartInfo = psi };
            process.Start();

            // Start asynchronous reading of stdout and stderr
            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();

            // Wait for process to exit (timeout: 2 minutes)
            if (!process.WaitForExit(120000))
            {
                try { process.Kill(entireProcessTree: true); } catch { }
                Log.Write(Resources.ghidorahtimeout);
                return Resources.ghidorahtimeout;
            }

            // Ensure async reads are completed
            Task.WaitAll(outputTask, errorTask);

            string output = outputTask.Result;
            string error = errorTask.Result;

            // Non-zero exit code indicates an error
            if (process.ExitCode != 0)
            {
                Log.Write($"{Resources.Ghidoraherror}: {error}");
                return $"{Resources.Ghidoraherror}: {error}";
            }

            // Return raw plugin list if status check not requested
            if (!status) return output;

            // Deserialize JSON status response
            var stat = JsonSerializer.Deserialize<StatusResponse>(
                output,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            // Build human-readable summary of status
            return BuildSummary(stat);
        }

        /// <summary>
        /// Executes a search using Ghidorah with the given arguments.
        /// </summary>
        internal static string Search(SearchArgs args)
        {
            // Convert arrays to space-separated strings for CLI
            string sources = string.Join(" ", args.Sources);
            string categories = string.Join(" ", args.Categories);

            var psi = new ProcessStartInfo
            {
                FileName = ExeFileName,
                Arguments =
                    $"{args.Query} " +
                    $"--limit {args.Limit} " +
                    $"--total_limit {args.TotalLimit} " +
                    $"--categories {categories} " +
                    $"--sort_by {args.SortBy} " +
                    $"--sources {sources}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            // Append qBittorrent plugin flag if enabled
            if (Settings.Current.UseQbittorrentPlugins)
            {
                psi.Arguments += " --use_qb_plugins";
            }

            using var process = new Process { StartInfo = psi };
            process.Start();

            // Asynchronously read stdout and stderr
            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();

            // Wait for exit with configurable timeout
            if (!process.WaitForExit(Settings.Current.Timeout))
            {
                try { process.Kill(entireProcessTree: true); } catch { }
                Log.Write(Resources.ghidorahtimeout);
                return $"{{\"data\":[],\"errors\":[\"{Resources.ghidorahtimeout}\"]}}";
            }

            Task.WaitAll(outputTask, errorTask);

            string output = outputTask.Result;
            string error = errorTask.Result;

            if (process.ExitCode != 0)
            {
                Log.Write($"{Resources.Ghidoraherror}: {error}");
                return $"{{\"data\":[],\"errors\":[\"{Resources.Ghidoraherror}: {error}\"]}}";
            }

            return output; // Return JSON search result
        }

        /// <summary>
        /// Builds a human-readable summary of plugin/source status.
        /// </summary>
        internal static string BuildSummary(StatusResponse status)
        {
            var sb = new StringBuilder();

            // ---- Debug paths ----
            if (!string.IsNullOrWhiteSpace(status.Paths))
            {
                sb.AppendLine(status.Paths.TrimEnd());
                sb.AppendLine(); // Add spacing
            }

            // ---- Header ----
            sb.AppendLine(Resources.Ghidorahpluginstatus);
            sb.AppendLine();

            // ---- Results ----
            foreach (var r in status.Results)
            {
                if (r.Status == "ONLINE")
                {
                    sb.AppendLine($"{r.Source} — {Resources.Online} ({r.Results} {Resources.Results}");
                }
                else if (r.Status == "ERROR")
                {
                    sb.AppendLine($"{r.Source} — {Resources.Error}");
                }
                else
                {
                    sb.AppendLine($"{r.Source} — {Resources.Unknown}");
                }
            }

            // ---- Error details ----
            var errors = status.Results
                .Where(r => r.Status == "ERROR")
                .ToList();

            if (errors.Any())
            {
                sb.AppendLine();
                sb.AppendLine($"{Resources.Errors}:");

                foreach (var e in errors)
                {
                    sb.AppendLine($"- {e.Source}: {TrimError(e.Error)}");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Trims an error message to a maximum length for display.
        /// </summary>
        internal static string TrimError(string error, int maxLength = 120)
        {
            if (string.IsNullOrEmpty(error))
                return Resources.Unknownerror;

            return error.Length > maxLength
                ? error.Substring(0, maxLength) + "…"
                : error;
        }
    }
}
