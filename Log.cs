using System;
using System.Collections.Generic;
using System.Text;

namespace lain
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

        // Loads the log from persistent storage
        static void LoadLog()
        {

        }

        // Saves the log to persistent storage
        static void SaveLog()
        {


        }

        //Clears the log
        static void ClearLog()
        {
            log.Clear();
            // Fire event to notify UI
            OnLogAdded?.Invoke();
        }







    }
}