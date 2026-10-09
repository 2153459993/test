using System.Collections.Generic;

namespace DataAnalysis.Core
{
    internal static class DictExtensions
    {
        public static TValue GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key)
        {
            TValue v;
            return dict.TryGetValue(key, out v) ? v : default(TValue);
        }
    }
}