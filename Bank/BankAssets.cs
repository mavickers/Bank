using LightPath.Bank.Commands;
using LightPath.Bank.Extensions;
using LightPath.Bank.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web.Optimization;

namespace LightPath.Bank
{
    public static class BankAssets
    {
        private const string _pathPrefix = "/lightpath.bank/";
        private static readonly Dictionary<Constants.BundleTypes, string> _bundleTypeExtensionMap = new() { { Constants.BundleTypes.Script, ".js" }, { Constants.BundleTypes.Style, ".css" } };
        private static readonly ConcurrentDictionary<string, BankEmbeddedResource> _cache = new();
        private static readonly ConcurrentDictionary<string, IBankCommand> _commands = new();
        private static readonly List<IBankEmbeddedResourceResolver> _resolvers = new();

        static BankAssets()
        {
            _commands.AddOrUpdate("$table", new TableCommand(_cache), (_, v) => v);
        }

        public static IDictionary<string, BankEmbeddedResource> All => new ReadOnlyDictionary<string, BankEmbeddedResource>(_cache);

        public static Bundle AddBundle(IBankAssetRegistrationStrategy strategy, string path, Constants.BundleTypes bundleType)
        {
            if (strategy == null) throw new ArgumentNullException(nameof(strategy));
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path));
            if (!strategy.AnyAssets()) strategy.Register();
            if (!strategy.AnyAssets()) return null;

            var bundle = bundleType == Constants.BundleTypes.Style ? new StyleBundle(path) : new ScriptBundle(path) as Bundle;

            bundle.Transforms.Clear();

            var assets = strategy.AllAssets.Where(asset => asset.Value.FileName.ToLower().EndsWith(_bundleTypeExtensionMap[bundleType])).Select(asset => asset.Value);

            foreach (var asset in assets) bundle.Include(asset);

            BundleTable.Bundles.Add(bundle);

            return bundle;
        }

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

        public static BankEmbeddedResource GetByKey(string key) => _cache.ContainsKey(key) ? _cache[key] : _commands.ContainsKey(key) ? _commands[key].GetResource() : null;

        public static BankEmbeddedResource GetByUrl(string url)
        {
            var hasLightPathRef = url.StartsWith(_pathPrefix, StringComparison.OrdinalIgnoreCase);
            var key = hasLightPathRef ? url.Substring(_pathPrefix.Length) : url;

            return hasLightPathRef
                ? GetByKey(key) 
                : _cache.FirstOrDefault(res => string.Equals(res.Value.BaseUrl, url, StringComparison.InvariantCultureIgnoreCase) || string.Equals(res.Value.Url, url, StringComparison.InvariantCultureIgnoreCase)).Value;
        }

        public static BankEmbeddedResource GetByVirtualPath(string virtualPath)
        {
            // if the virtual path doesn't start with the bank virtual path prefix,
            // just perform an ordinary search against virtual path values; otherwise,
            // take the remainder and search against the keys.

            var hasLightPathRef = virtualPath.StartsWith($"~{_pathPrefix}", StringComparison.OrdinalIgnoreCase);
            if (!hasLightPathRef) return _cache.FirstOrDefault(res => string.Equals(res.Value.VirtualPath, virtualPath, StringComparison.OrdinalIgnoreCase)).Value;
            var key = virtualPath.Substring(_pathPrefix.Length + 1);

            return GetByKey(key);
        }        

        public static string GetUrlByKey(string key) => GetByKey(key)?.Url;

        public static IList<BankEmbeddedResource> Register(IBankAssetRegistrationStrategy strategy) => strategy.Register();

        public static bool Register(BankEmbeddedResource resource) => Register(resource.ResourceKey, resource);

        public static bool Register(string key, BankEmbeddedResource resource)
        {
            var _key = !string.IsNullOrWhiteSpace(key) ? key : resource.ResourceKey;

            if (_cache.ContainsKey(_key)) return Config.ThrowOnDuplicate ? throw new Exception($"Asset with key {_key} is already registered") : false;
            if (ContainsUrl(resource.Url)) return Config.ThrowOnDuplicate ? throw new Exception($"Embedded resource with url '${resource.Url}' is already registered") : false;
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

        public static BankEmbeddedResource GetByResolver(string locator)
        {
            foreach (var resource in _cache)
            {
                foreach (var resolver in _resolvers)
                {
                    if (resolver.IsResolving(locator, resource.Value)) return resource.Value;
                }
            }

            return null;
        }

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
