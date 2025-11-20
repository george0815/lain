using System;
using System.Collections.Generic;
using System.Text;

namespace lain
{
    internal class Log
    {

        // Event fired whenever a new log entry is added
        public static event Action? OnLogAdded;

        static internal List<string> log { get; set; } = [];


        static internal void Write(string msg)
        {
            log.Add($"[{DateTime.Now}] {msg}");

            // Fire event to notify UI
            OnLogAdded?.Invoke();

        }

        static void LoadLog()
        {

        }


        static void SaveLog()
        {


        }







    }
}