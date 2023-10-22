using System;
using System.Collections.Generic;
using System.Linq;

namespace LightPath.Bank
{
    public static class Assets
    {
        private static readonly Dictionary<string, EmbeddedResource> _cache = new();

        public static bool ContainsKey(string key) => _cache.ContainsKey(key);

        public static bool ContainsUrl(string url) => GetByUrl(url) != null;

        public static EmbeddedResource GetByKey(string key) => _cache.ContainsKey(key) ? _cache[key] : null;

        public static EmbeddedResource GetByUrl(string url) => _cache.FirstOrDefault(res => string.Equals(res.Value.Url, url, StringComparison.CurrentCultureIgnoreCase)).Value;

        public static void Register(EmbeddedResource resource, bool withContentCaching = false) => Register($"EmbeddedResource-({resource.Url})", resource, withContentCaching);

        public static void Register(string key, EmbeddedResource resource, bool withContentCaching = false)
        {
            var _key = string.IsNullOrWhiteSpace(key) ? $"EmbeddedResource-({resource.Url})" : key;

            if (_cache.ContainsKey(_key)) throw new Exception($"Asset with key {_key} is already registered");

            lock (_cache)
            {
                _cache.Add(_key, resource);
            }

        }

        public static bool Unregister(string key) => _cache.Remove(key);
    }
}
