using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using System.Text.Json;

namespace lain.helpers
{

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

            using var process = new Process { StartInfo = psi };
            process.Start();

            const int timeoutMs = 60_000;

            // Read output asynchronously
            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();

            if (!process.WaitForExit(timeoutMs))
            {
                try { process.Kill(entireProcessTree: true); } catch { }
                return Resources.ghidorahtimeout;
            }

            // Ensure async reads completed
            Task.WaitAll(outputTask, errorTask);

            string output = outputTask.Result;
            string error = errorTask.Result;

            if (process.ExitCode != 0)
            {
                Debug.WriteLine($"{Resources.Ghidoraherror}: {error}");
                return error;
            }

            return output;
        }

    }
}
