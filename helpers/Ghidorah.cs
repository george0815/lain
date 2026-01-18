using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Terminal.Gui;
using System.Text.Json.Serialization;


namespace lain.helpers
{


    public class StatusResponse
    {
        [JsonPropertyName("paths")]
        public string Paths { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("results")]
        public List<SourceStatus> Results { get; set; }
    }

    public class SourceStatus
    {
        [JsonPropertyName("source")]
        public string Source { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("results")]
        public int? Results { get; set; }

        [JsonPropertyName("error")]
        public string Error { get; set; }
    }

    // Struct to hold search arguments
    internal struct SearchArgs
    {
        internal string Query;
        internal int Limit;
        internal int TotalLimit;
        internal String[] Sources;
        internal String[] Categories;
        internal String SortBy;
    
    }



    internal class Ghidorah
    {

        internal static String[] QbSources { get; set; } = [];

        internal static void LoadQbittorrentPlugins()
        {
            string result = CheckStatusPlugins(false);
            try
            {
                var plugins = JsonSerializer.Deserialize<List<string>>(result);
                if (plugins != null)
                {
                    QbSources = plugins.ToArray();
                }
            }
            catch (Exception ex)
            {
                Log.Write($"{Resources.Error}: {ex.Message}");
            }
        }

        internal static string CheckStatusPlugins(bool status)
        {
          

            var psi = new ProcessStartInfo
            {
                FileName = "ghidorah.exe",
                Arguments =
                    status ? $"--check_status" : $"--check_plugins",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            using var process = new Process { StartInfo = psi };
            process.Start();



            // Read output asynchronously
            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();

            if (!process.WaitForExit(120000)) //2 mins
            {
                try { process.Kill(entireProcessTree: true); } catch { }
                Log.Write(Resources.ghidorahtimeout);
                return Resources.ghidorahtimeout;

            }

            // Ensure async reads completed
            Task.WaitAll(outputTask, errorTask);

            string output = outputTask.Result;
            string error = errorTask.Result;

            if (process.ExitCode != 0)
            {
                Log.Write($"{Resources.Ghidoraherror}: {error}");
                return $"{Resources.Ghidoraherror}: {error}";

            }

            if (status == false)
            {
                return output;
            }

            var stat = JsonSerializer.Deserialize<StatusResponse>(
                output,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );



            return BuildSummary(stat);
        }

        // Execute Ghidorah (paython exe as a subprocess) search with given arguments
        internal static string Search(SearchArgs args)
        {
            string sources = string.Join(" ", args.Sources);
            string categories = string.Join(" ", args.Categories);

            var psi = new ProcessStartInfo
            {
                FileName = "ghidorah.exe",
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

            if (Settings.Current.UseQbittorrentPlugins)
            {
                psi.Arguments += " --use_qb_plugins";   
            }

            using var process = new Process { StartInfo = psi };
            process.Start();

            

            // Read output asynchronously
            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();

            if (!process.WaitForExit(Settings.Current.Timeout))
            {
                try { process.Kill(entireProcessTree: true); } catch { }
                Log.Write(Resources.ghidorahtimeout);
                return $"{{\"data\":[],\"errors\":[\"{Resources.ghidorahtimeout}\"]}}";
                
            }

            // Ensure async reads completed
            Task.WaitAll(outputTask, errorTask);

            string output = outputTask.Result;
            string error = errorTask.Result;

            if (process.ExitCode != 0)
            {
                Log.Write($"{Resources.Ghidoraherror}: {error}");
                return $"{{\"data\":[],\"errors\":[\"{Resources.Ghidoraherror}: {error}\"]}}";

            }

            return output;
        }

        internal static string BuildSummary(StatusResponse status)
        {
            var sb = new StringBuilder();

            // ---- Debug paths block ----
            if (!string.IsNullOrWhiteSpace(status.Paths))
            {
                sb.AppendLine(status.Paths.TrimEnd());
                sb.AppendLine(); // spacing
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
