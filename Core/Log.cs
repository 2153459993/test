using System;
using System.IO;
using System.Linq;

namespace DataAnalysis.Core
{
    public static class Log
    {
        private static string _logDir;
        private static string _logPath;

        public static void Setup(string logDir)
        {
            Directory.CreateDirectory(logDir);
            _logDir = logDir;
            CleanupOld();
            _logPath = Path.Combine(logDir, $"app_{DateTime.Now:yyyyMM}.log");
        }

        private static void CleanupOld()
        {
            try
            {
                var now = DateTime.Now;
                foreach (var p in Directory.GetFiles(_logDir, "app_*.log"))
                {
                    if ((now - File.GetLastWriteTime(p)).TotalDays > 30)
                        File.Delete(p);
                }
            }
            catch { }
        }

        public static void Info(string msg) => Write("INFO", msg);
        public static void Warn(string msg) => Write("WARN", msg);
        public static void Error(string msg) => Write("ERROR", msg);

        private static void Write(string level, string msg)
        {
            try
            {
                var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {msg}";
                File.AppendAllText(_logPath, line + Environment.NewLine);
            }
            catch { }
        }
    }
}