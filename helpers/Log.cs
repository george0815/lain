using System;
using System.Collections.Generic;
using System.Text;



namespace lain.helpers
{
    internal class Log
    {

        // Event fired whenever a new log entry is added
        public static event Action? OnLogAdded;


        // Internal log storage
        static internal List<string> log { get; set; } = [];

        // Method to write a new log entry, appends timestamp
        static internal void Write(string msg)
        {
            log.Add($"[{DateTime.Now}] {msg}");

            // Fire event to notify UI
            OnLogAdded?.Invoke();

        }


        // Saves the log to persistent storage
        static internal void Save(){ 
            using StreamWriter writer = new StreamWriter(Settings.Current.LogPath!, false, Encoding.UTF8);
            writer.Write(String.Join("\n",log));
        }

   





    }
}