using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace LightPath.Bank
{
    public static class BankAssets
    {
        public static IDictionary<string, BankEmbeddedResource> All => new ReadOnlyDictionary<string, BankEmbeddedResource>(_cache);
        private static readonly Dictionary<string, BankEmbeddedResource> _cache = new();

        public static bool ContainsKey(string key) => _cache.ContainsKey(key);

        public static bool ContainsUrl(string url) => GetByUrl(url) != null;

        public static BankEmbeddedResource GetByKey(string key) => _cache.ContainsKey(key) ? _cache[key] : null;

        public static BankEmbeddedResource GetByUrl(string url) => _cache.FirstOrDefault(res => string.Equals(res.Value.Url, url, StringComparison.CurrentCultureIgnoreCase)).Value;

        public static void Register(BankEmbeddedResource resource) => Register($"EmbeddedResource-({resource.Url})", resource);

        public static void Register(string key, BankEmbeddedResource resource)
        {
            var _key = string.IsNullOrWhiteSpace(key) ? $"EmbeddedResource-({resource.Url})" : key;

            if (_cache.ContainsKey(_key)) throw new Exception($"Asset with key {_key} is already registered");
            if (resource.Exceptions?.Any() ?? false) throw new Exception("Embedded resource contains exceptions", resource.Exceptions.First());
            if (ContainsUrl(resource.Url)) throw new Exception($"Embedded resource with url '${resource.Url}' is already registered");

            lock (_cache)
            {
                _cache.Add(_key, resource);
            }
        }

        public static int Total => _cache?.Count ?? 0;

        public static bool Unregister(string key)
        {
            lock (_cache)
            {
                return _cache.Remove(key);
            }
        }
    }
}
