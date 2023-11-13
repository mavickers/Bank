using System.Collections.Generic;
using System.Linq;

namespace LightPath.Bank
{
    public static class BankExtensions
    {
        private static readonly byte[] byteOrderMarker = { 0xEF, 0xBB, 0xBF };

        public static string AsString(this byte[] source)
        {
            // first, trim off the byte order marker if it's present

            var bytes = source[0] == byteOrderMarker[0] && source[1] == byteOrderMarker[1] && source[2] == byteOrderMarker[2] ? source.Skip(3).ToArray() : source;

            return System.Text.Encoding.UTF8.GetString(bytes);
        }

        internal static Dictionary<string, string> AddRange(this Dictionary<string, string> source, Dictionary<string, string> supplementalDictionary)
        {
            if (supplementalDictionary == null) return source;

            foreach (var pair in supplementalDictionary)
            {
                if (!source.ContainsKey(pair.Key)) source.Add(pair.Key, pair.Value);
            }

            return source;
        }

        internal static Dictionary<string, string> SetOrAdd(this Dictionary<string, string> source, string key, string value)
        {
            if (source == null) return source;
            if (string.IsNullOrWhiteSpace(key)) return source;
            
            if (source.ContainsKey(key)) source[key] = value;
            else source.Add(key, value);

            return source;
        }
    }
}
