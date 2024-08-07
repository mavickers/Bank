using LightPath.Bank.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace LightPath.Bank
{
    public static class BankAssets
    {
        private const string _pathPrefix = "/lightpath.bank/";
        private static readonly ConcurrentDictionary<string, BankEmbeddedResource> _cache = new();
        private static readonly List<IBankEmbeddedResourceResolver> _resolvers = new();

        public static IDictionary<string, BankEmbeddedResource> All => new ReadOnlyDictionary<string, BankEmbeddedResource>(_cache);

        public struct Config
        {
            public static bool ThrowOnDuplicate = true;
        }

        public static void Clear() => _cache.Clear();

        public static bool Contains(BankEmbeddedResource resource) => _cache.Values.Contains(resource);

        public static bool ContainsKey(string key) => _cache.ContainsKey(key);

        public static bool ContainsByResolver(string locator) => GetByResolver(locator) != null;

        public static bool ContainsUrl(string url) => GetByUrl(url) != null;

        public static bool ContainsVirtualPath(string virtualPath) => GetByVirtualPath(virtualPath) != null;

        public static BankEmbeddedResource GetByKey(string key) => _cache.ContainsKey(key) ? _cache[key] : null;

        public static BankEmbeddedResource GetByUrl(string url)
        {
            // same setup here as GetByVirtualPath, see notes below

            var hasLightPathRef = url.ToLower().StartsWith(_pathPrefix);

            if (!hasLightPathRef) return _cache.FirstOrDefault(res => new[] { res.Value.BaseUrl, res.Value.Url }.Contains(url, StringComparer.InvariantCultureIgnoreCase)).Value;

            var key = url.Remove(0, _pathPrefix.Length);

            return GetByKey(key);
        }

        public static BankEmbeddedResource GetByVirtualPath(string virtualPath)
        {
            var hasLightPathRef = virtualPath.ToLower().StartsWith($"~{_pathPrefix}");

            // if the virtual path doesn't start with the bank virtual path prefix,
            // just perform an ordingary search against virtual path values.

            if (!hasLightPathRef) return _cache.FirstOrDefault(res => string.Equals(res.Value.VirtualPath, virtualPath, StringComparison.CurrentCultureIgnoreCase)).Value;

            // since the url starts with the bank virtual path prefix,
            // take the remainder and search against the keys.

            var key = virtualPath.Remove(0, _pathPrefix.Length + 1);

            return GetByKey(key);
        }

        public static void Register(IBankAssetRegistrationStrategy strategy) => strategy.Register();

        public static bool Register(BankEmbeddedResource resource) => Register($"EmbeddedResource-{Guid.NewGuid()}-({resource.Url})", resource);

        public static bool Register(string key, BankEmbeddedResource resource)
        {
            var _key = string.IsNullOrWhiteSpace(key) ? $"EmbeddedResource-{Guid.NewGuid()}-({resource.Url})" : key;

            if (_cache.ContainsKey(_key) && Config.ThrowOnDuplicate) throw new Exception($"Asset with key {_key} is already registered");
            if (ContainsUrl(resource.Url) && Config.ThrowOnDuplicate) throw new Exception($"Embedded resource with url '${resource.Url}' is already registered");
            if (resource.Exceptions?.Any() ?? false) throw new Exception("Embedded resource contains exceptions", resource.Exceptions.First());

            return _cache.TryAdd(_key, resource);
        }

        public static void RegisterResolver(Type BankEmbeddedResourceResolverType)
        {
            if (!BankEmbeddedResourceResolverType.GetInterfaces().Contains(typeof(IBankEmbeddedResourceResolver))) return;
            
            _resolvers.Add((IBankEmbeddedResourceResolver)Activator.CreateInstance(BankEmbeddedResourceResolverType));
        }

        public static bool Rekey(string oldKey, string newKey)
        {
            if (string.IsNullOrWhiteSpace(oldKey)) return false;
            if (string.IsNullOrWhiteSpace(newKey)) return false;
            if (!_cache.ContainsKey(oldKey)) return false;
            if (_cache.ContainsKey(newKey)) return false;

            if (!_cache.TryRemove(oldKey, out var oldValue)) return false;
            if (!_cache.TryAdd(newKey, oldValue)) return false;

            return true;
        }

        public static bool Rekey(BankEmbeddedResource resource, string newKey)
        {
            if (resource == null) return false;
            if (!_cache.Values.Contains(resource)) return false;

            return Rekey(_cache.FirstOrDefault(res => res.Value == resource).Key, newKey);
        }

        public static BankEmbeddedResource GetByResolver(string locator) => _cache.FirstOrDefault(resource => _resolvers.Any(resolver => resolver.IsResolving(locator, resource.Value))).Value;

        public static int Total => _cache?.Count ?? 0;

        public static bool TryGetByKey(string key, out BankEmbeddedResource result)
        {
            result = GetByKey(key);

            return result != null;
        }

        public static bool TryGetByResolver(string locator, out BankEmbeddedResource result)
        {
            result = GetByResolver(locator);

            return result != null;
        }

        public static bool TryGetByUrl(string url, out BankEmbeddedResource result)
        {
            result = GetByUrl(url);

            return result != null;
        }

        public static bool TryGetByVirtualPath(string virtualPath, out BankEmbeddedResource result)
        {
            result = GetByVirtualPath(virtualPath);

            return result != null;
        }

        public static bool Unregister(string key)
        {
            return _cache.TryRemove(key, out _);
        }

        public static void UnregisterResolver(Type BankEmbeddedResourceResolverType)
        {
            if (!BankEmbeddedResourceResolverType.GetInterfaces().Contains(typeof(IBankEmbeddedResourceResolver))) return;

            var resolverToUnregister = _resolvers.FirstOrDefault(resolver => resolver.GetType() == BankEmbeddedResourceResolverType);

            if (resolverToUnregister != null) _resolvers.Remove(resolverToUnregister);
        }

        public static BankVirtualPathProvider VirtualPathProvider = new();
    }
}
