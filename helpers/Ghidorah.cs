using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using System.Text.Json;


namespace lain.helpers
{

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


            using (var process = new Process { StartInfo = psi })
            {
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();
                if (process.ExitCode != 0)
                {
                    Console.WriteLine("Error executing ghidorah:");
                    Console.WriteLine(error);
                    return error;
                }
                else
                {
                    return output;
                }
            }

        }

    }
}
