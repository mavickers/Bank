using LightPath.Bank.ContentProcessors;
using LightPath.Bank.Extensions;
using LightPath.Bank.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;

namespace LightPath.Bank.RegistrationStrategies
{
    public class ReactCRAStrategy : IBankAssetRegistrationStrategy
    {
        private readonly object _cacheLock = new();
        private readonly Dictionary<string, BankEmbeddedResource> _cache = new();
        private readonly List<string> _extensionInclusions = new();
        private readonly List<string> _extensionExclusions = new();
        private readonly List<string> _pathExclusions = new();
        private readonly List<string> _pathInclusions = new();

        public IDictionary<string, BankEmbeddedResource> All => AllAssets;
        public IDictionary<string, BankEmbeddedResource> AllAssets => new ReadOnlyDictionary<string, BankEmbeddedResource>(_cache);

        public Assembly Assembly { get; }
        /// <summary>
        /// The app root namespace of the React app.
        /// </summary>
        public string NameSpace { get; }
        public string StartingPoint { get; }
        public string UrlPrepend { get; }

        public ReactCRAStrategy(Assembly assembly, string nameSpace, string assetManifest = "asset-manifest.json", string urlPrepend = null)
        {
            Assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
            NameSpace = nameSpace ?? throw new ArgumentNullException(nameof(nameSpace));
            StartingPoint = assetManifest ?? throw new ArgumentNullException(nameof(assetManifest));
            UrlPrepend = urlPrepend;
        }

        public BankEmbeddedResource this[string key] => throw new NotImplementedException();

        [Obsolete("Use ExcludePaths instead")]
        public IBankAssetRegistrationStrategy Exclude(params string[] exclusions) => ExcludePaths(exclusions);

        public IBankAssetRegistrationStrategy ExcludeExtensions(params string[] exclusions) => this.SafeAdd(_extensionExclusions, exclusions);

        public IBankAssetRegistrationStrategy ExcludePaths(params string[] exclusions) => this.SafeAdd(_pathExclusions, exclusions);

        public IBankAssetRegistrationStrategy IncludeExtensions(params string[] inclusions) => this.SafeAdd(_extensionInclusions, inclusions);

        public IBankAssetRegistrationStrategy IncludePaths(params string[] inclusions) => this.SafeAdd(_pathInclusions, inclusions);

        public IList<string> Filters(Constants.FilterTypes filter) => this.GetFilters(filter, _extensionExclusions, _extensionInclusions, _extensionExclusions, _pathInclusions);

        public IList<BankEmbeddedResource> Register()
        {
            if (this.AnyAssets()) return AllAssets.Values.ToList();

            using var stream = Assembly.GetManifestResourceStream($"{Assembly.GetName().Name}.{NameSpace}.{StartingPoint}");
            using var reader = stream == null ? null : new StreamReader(stream);
            var manifestJson = reader == null ? null : System.Web.Helpers.Json.Decode(reader.ReadToEnd());

            if (manifestJson == null || manifestJson.entrypoints == null) return new List<BankEmbeddedResource>().AsReadOnly();

            foreach (var entryPoint in manifestJson.entrypoints)
            {
                var fullPath = $"/{(string)entryPoint.ToString()}";
                var path = fullPath.Substring(1).Split('/');
                var filename = path.Last();

                if (!this.PassesFilters(filename)) continue;

                var @namespace = $"{NameSpace}{(path.Length > 1 ? "." + string.Join(".", path.Where(p => p != path.Last())) : string.Empty)}";
                var fileKey = $"{Assembly.GetName().Name}.{@namespace}.{filename}";
                var extension = filename.Split('.').Last().ToLower();
                var contentType = BankHelpers.MimeMappings.TryGetValue(extension, out var mapping) ? mapping : null;
                var resource = new BankEmbeddedResource
                {
                    Assembly = Assembly,
                    NameSpace = @namespace,
                    FileName = filename,
                    ContentType = contentType,
                    UrlPrepend = UrlPrepend,
                    ContentProcessors = filename.ToLower().EndsWith("css") ? new() { new ReactCssContentProcessor(Assembly, NameSpace, UrlPrepend) } : null,
                };

                BankAssets.Register(resource);
                lock (_cacheLock) if (!_cache.ContainsKey(fileKey)) _cache.Add(fileKey, resource);
            }

            return All.Values.ToList();
        }

        public IList<BankEmbeddedResource> RegisterOld()
        {
            using var stream = Assembly.GetManifestResourceStream($"{Assembly.GetName().Name}.{NameSpace}.{StartingPoint}");
            using var reader = stream == null ? null : new StreamReader(stream);
            var manifestJson = reader == null ? null : System.Web.Helpers.Json.Decode(reader.ReadToEnd());

            if (manifestJson == null || manifestJson.files == null) return new List<BankEmbeddedResource>().AsReadOnly();

            foreach (var file in manifestJson.files)
            {
                var key = (string)file.Key.ToString();
                var value = (string)file.Value.ToString();
                var path = value.Substring(1).Split('/');
                var filename = path.Last();

                if (!this.PassesFilters(value)) continue;

                var @namespace = $"{NameSpace}{(path.Length > 1 ? "." + string.Join(".", path.Where(p => p != path.Last())) : string.Empty)}";
                var extension = filename.Split('.').Last().ToLower();
                var contentType = BankHelpers.MimeMappings.TryGetValue(extension, out var mapping) ? mapping : null;
                var resource = new BankEmbeddedResource
                {
                    Assembly = Assembly,
                    NameSpace = @namespace,
                    FileName = filename,
                    ContentType = contentType,
                    UrlPrepend = UrlPrepend,
                    ContentProcessors = filename.ToLower().EndsWith("css") ? new() { new ReactCssContentProcessor(Assembly, NameSpace, UrlPrepend) } : null,
                };

                BankAssets.Register(resource);
                lock (_cacheLock) if (!_cache.ContainsKey(key)) _cache.Add(key, resource);
            }

            return _cache.Select(item => item.Value).ToList().AsReadOnly();
        }

        public IEnumerable<KeyValuePair<string, BankEmbeddedResource>> Where(Func<KeyValuePair<string, BankEmbeddedResource>, bool> predicate) => new ReadOnlyDictionary<string, BankEmbeddedResource>(_cache).Where(predicate);
    }
}
