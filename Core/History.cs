using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DataAnalysis.Core
{
    public static class History
    {
        public static List<Dictionary<string, object>> LoadRuns(string path)
        {
            try
            {
                if (!File.Exists(path)) return new List<Dictionary<string, object>>();
                var json = File.ReadAllText(path);
                var arr = JsonConvert.DeserializeObject<JArray>(json);
                return arr?.Select(o => o.ToObject<Dictionary<string, object>>()).ToList() ?? new List<Dictionary<string, object>>();
            }
            catch { return new List<Dictionary<string, object>>(); }
        }

        public static void SaveRuns(string path, List<Dictionary<string, object>> runs)
        {
            try { File.WriteAllText(path, JsonConvert.SerializeObject(runs, Formatting.Indented)); } catch { }
        }

        public static List<Dictionary<string, object>> AppendRun(string path, Dictionary<string, object> rec)
        {
            var runs = LoadRuns(path);
            runs.Insert(0, rec);
            if (runs.Count > 200) runs = runs.Take(200).ToList();
            SaveRuns(path, runs);
            return runs;
        }

        public static Dictionary<string, object> MakeRunRecord(string srcName, string outName,
            Dictionary<string, object> summary, List<Dictionary<string, object>> layerStats, Dictionary<string, object> cfg)
        {
            var layers = new List<Dictionary<string, object>>();
            if (layerStats != null)
            {
                foreach (var x in layerStats)
                {
                    layers.Add(new Dictionary<string, object>
                    {
                        ["信号层"] = x.GetValueOrDefault("信号层"),
                        ["越限率%"] = x.GetValueOrDefault("越限率%"),
                        ["平均阻抗"] = x.GetValueOrDefault("平均阻抗"),
                        ["样本数"] = x.GetValueOrDefault("样本数"),
                    });
                }
            }
            var cfgKeys = new[] { "group_keys", "check_fields", "combo", "limit_mode", "lower", "upper",
                "spec_value", "tolerance", "inclusive", "expected_layers", "require_complete",
                "allow_over_limit", "invalid_strategy" };
            var cfgRec = new Dictionary<string, object>();
            foreach (var k in cfgKeys)
                if (cfg.ContainsKey(k)) cfgRec[k] = cfg[k];

            return new Dictionary<string, object>
            {
                ["time"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                ["ts"] = (long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds,
                ["src"] = srcName,
                ["out"] = outName,
                ["summary"] = summary,
                ["layers"] = layers,
                ["cfg"] = cfgRec,
            };
        }

        public static Dictionary<string, object> TrendData(List<Dictionary<string, object>> runs)
        {
            var seq = runs.OrderBy(r =>
            {
                object ts = r.GetValueOrDefault("ts");
                return ts != null ? Convert.ToDouble(ts) : 0;
            }).ToList();

            var labels = seq.Select(r =>
            {
                string t = r.GetValueOrDefault("time")?.ToString() ?? "";
                return t.Length >= 16 ? t.Substring(5, 11) : t;
            }).ToList();

            var passRate = seq.Select(r =>
            {
                var s = r.GetValueOrDefault("summary") as JObject;
                if (s != null && s["合格率%"] != null) return s["合格率%"].Value<double>();
                return 0.0;
            }).ToList();

            var avgImp = seq.Select(r =>
            {
                var s = r.GetValueOrDefault("summary") as JObject;
                double? v = s?["平均阻抗"]?.Value<double?>();
                if (v.HasValue) return v.Value;
                var layers = r.GetValueOrDefault("layers") as JArray;
                if (layers != null && layers.Count > 0)
                {
                    var vals = layers.Select(x => x["平均阻抗"]?.Value<double?>()).Where(x => x.HasValue).Select(x => x.Value).ToList();
                    if (vals.Count > 0) return Math.Round(vals.Average(), 3);
                }
                return double.NaN;
            }).ToList();

            var oobRate = seq.Select(r =>
            {
                var s = r.GetValueOrDefault("summary") as JObject;
                if (s == null) return 0.0;
                int rec = s["记录数"]?.Value<int>() ?? 0;
                int oob = s["越限记录数"]?.Value<int>() ?? 0;
                return rec > 0 ? Math.Round((double)oob / rec * 100, 2) : 0.0;
            }).ToList();

            return new Dictionary<string, object>
            {
                ["labels"] = labels, ["pass_rate"] = passRate, ["avg_imp"] = avgImp, ["oob_rate"] = oobRate,
            };
        }
    }
}