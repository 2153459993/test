using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DataAnalysis.Core
{
    public static class IoFiles
    {
        private static readonly string[] Supported = { ".xlsx", ".xls", ".csv" };

        public static List<string> ScanInput(string folder)
        {
            var files = new List<string>();
            foreach (var ext in Supported)
                files.AddRange(Directory.GetFiles(folder, "*" + ext));
            return files.Distinct().OrderBy(f => f).ToList();
        }

        public static string BuildOutputName(string srcPath, string outDir, string suffix = "筛选结果")
        {
            string baseName = Path.GetFileNameWithoutExtension(srcPath);
            string stamp = DateTime.Now.ToString("yyyyMMdd-HHmm");
            return Path.Combine(outDir, $"{baseName}-{suffix}-{stamp}.xlsx");
        }

        public static void EnsureDir(string p)
        {
            Directory.CreateDirectory(p);
        }
    }
}