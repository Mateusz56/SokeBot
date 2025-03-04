namespace SokeBot
{
    using System;
    using System.IO;

    public class FileLogger
    {
        private readonly string _logFilePath = "soke.log";
        private readonly object _lock = new object();

        public FileLogger()
        {
            if (!File.Exists("soke.log"))
            {
                File.CreateText("soke.log");
            }
        }

        public void Log(string message, LogLevel level = LogLevel.Info)
        {
            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}";

            lock (_lock) // Prevent concurrent write issues
            {
                File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
            }
        }

        public enum LogLevel
        {
            Info,
            Warning,
            Error
        }
    }
}
