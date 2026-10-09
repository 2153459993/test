using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DataAnalysis.Core
{
    public static class Config
    {
        public static readonly string[] StdFields = { "Set_ID", "信号层", "平均值", "最大值", "最小值", "规格值", "上限", "下限" };

        private static readonly Dictionary<string, string> AliasMap = new Dictionary<string, string>
        {
            {"setid","Set_ID"},{"set_id","Set_ID"},{"组编号","Set_ID"},{"组id","Set_ID"},
            {"编号","Set_ID"},{"id","Set_ID"},{"组号","Set_ID"},{"批次","Set_ID"},{"batch","Set_ID"},
            {"信号层","信号层"},{"层","信号层"},{"layer","信号层"},{"signal","信号层"},{"信号","信号层"},{"测试层","信号层"},
            {"平均值","平均值"},{"平均","平均值"},{"avg","平均值"},{"mean","平均值"},{"平均阻抗","平均值"},{"均值","平均值"},
            {"最大值","最大值"},{"最大","最大值"},{"max","最大值"},
            {"最小值","最小值"},{"最小","最小值"},{"min","最小值"},
            {"规格值","规格值"},{"规格","规格值"},{"设计值","规格值"},{"spec","规格值"},{"标准值","规格值"},{"目标值","规格值"},
            {"上限","上限"},{"upper","上限"},
            {"下限","下限"},{"lower","下限"},
        };

        public static string Norm(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            s = s.Trim().ToLower();
            foreach (char ch in new[] { ' ', '\t', '-', '_', '/', '（', '）', '(', ')', '．', '.' })
                s = s.Replace(ch.ToString(), "");
            return s;
        }

        public static Dictionary<string, object> DefaultCfg()
        {
            return new Dictionary<string, object>
            {
                ["group_keys"] = new List<string> { "Set_ID" },
                ["check_fields"] = new List<string> { "平均值", "最大值", "最小值" },
                ["combo"] = "all",
                ["combo_groups"] = new List<object>
                {
                    new Dictionary<string, object> { ["fields"] = new List<string> { "平均值", "最大值", "最小值" }, ["combo"] = "all" }
                },
                ["limit_mode"] = "direct",
                ["spec_field"] = "规格值",
                ["spec_value"] = 90.0,
                ["tolerance"] = 8.0,
                ["lower"] = 82.8,
                ["upper"] = 97.2,
                ["inclusive"] = true,
                ["layers"] = new List<string>(),
                ["expected_layers"] = new List<string>(),
                ["layer_groups"] = new List<object>(),
                ["layer_overrides"] = new Dictionary<string, object>(),
                ["require_complete"] = true,
                ["dup_strategy"] = "first",
                ["invalid_strategy"] = "fail",
                ["allow_over_limit"] = 0,
                ["span_limit"] = null,
                ["grading"] = false,
            };
        }

        public static Dictionary<string, object> DefaultAppConfig()
        {
            return new Dictionary<string, object>
            {
                ["input_dir"] = "input",
                ["output_dir"] = "output",
                ["cfg"] = DefaultCfg(),
            };
        }

        public static Dictionary<string, string> LoadUserAliases(string path)
        {
            try
            {
                if (!File.Exists(path)) return new Dictionary<string, string>();
                var json = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
            }
            catch { return new Dictionary<string, string>(); }
        }

        public static void SaveUserAliases(string path, Dictionary<string, string> d)
        {
            try { File.WriteAllText(path, JsonConvert.SerializeObject(d, Formatting.Indented)); } catch { }
        }

        public static JObject LoadAppConfig(string path)
        {
            try
            {
                if (!File.Exists(path)) return JObject.FromObject(DefaultAppConfig());
                return JObject.Parse(File.ReadAllText(path));
            }
            catch { return JObject.FromObject(DefaultAppConfig()); }
        }

        public static void SaveAppConfig(string path, object d)
        {
            try { File.WriteAllText(path, JsonConvert.SerializeObject(d, Formatting.Indented)); } catch { }
        }

        public static string MatchField(string name, Dictionary<string, string> userAliases)
        {
            if (string.IsNullOrEmpty(name)) return null;
            string key = Norm(name);
            if (userAliases.ContainsKey(key)) return userAliases[key];
            return AliasMap.ContainsKey(key) ? AliasMap[key] : null;
        }
    }
}