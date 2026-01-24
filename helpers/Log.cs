using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace lain.helpers
{
    /// <summary>
    /// Simple logging helper that stores logs in memory and optionally writes them to disk.
    /// Fires an event whenever a new log entry is added.
    /// </summary>
    internal class Log
    {
        /// <summary>
        /// Event fired whenever a new log entry is added.
        /// Can be used by the UI to update log views in real-time.
        /// </summary>
        public static event Action? OnLogAdded;

        /// <summary>
        /// Internal in-memory storage for log entries.
        /// </summary>
        static internal List<string> log { get; set; } = new List<string>();

        /// <summary>
        /// Adds a new log entry with timestamp.
        /// </summary>
        /// <param name="msg">The message to log.</param>
        static internal void Write(string msg)
        {
            // Append timestamp to message and store in log
            log.Add($"[{DateTime.Now}] {msg}");

            // Notify any listeners that a new log entry was added
            OnLogAdded?.Invoke();
        }

        /// <summary>
        /// Saves all log entries to persistent storage.
        /// </summary>
        static internal void Save()
        {
            // Open file for writing, overwrite existing content
            using StreamWriter writer = new StreamWriter(Settings.Current.LogPath!, false, Encoding.UTF8);

            // Join all log entries with newline and write to file
            writer.Write(string.Join("\n", log));
        }
    }
}
