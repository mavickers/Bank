using LightPath.Bank.Extensions;
using LightPath.Bank.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace LightPath.Bank.RegistrationStrategies
{
    public class ScoopStrategy : IBankAssetRegistrationStrategy
    {
        private readonly object _cacheLock = new();
        private readonly Dictionary<string, BankEmbeddedResource> _cache = new();
        private readonly List<string> _extensionExclusions = new();
        private readonly List<string> _extensionInclusions = new();
        private readonly List<string> _pathExclusions = new();
        private readonly List<string> _pathInclusions = new();

        public IDictionary<string, BankEmbeddedResource> All => AllAssets;
        public IDictionary<string, BankEmbeddedResource> AllAssets => new ReadOnlyDictionary<string, BankEmbeddedResource>(_cache);
        public Assembly Assembly { get; }
        public string NameSpace { get; }
        public string StartingPoint => Assembly == null || string.IsNullOrWhiteSpace(NameSpace) ? null : $"{Assembly.GetName().Name}.{NameSpace}";
        public string UrlPrepend { get; }

        public ScoopStrategy(Assembly assembly, string @namespace, string urlPrepend = null)
        {
            Assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
            NameSpace = @namespace ?? throw new ArgumentNullException(nameof(@namespace));
            UrlPrepend = urlPrepend;
        }

        public BankEmbeddedResource this[string key] => _cache.FirstOrDefault(resource => string.Equals(resource.Key, key, StringComparison.InvariantCultureIgnoreCase)).Value;

        [Obsolete("Use ExcludePaths instead")]
        public IBankAssetRegistrationStrategy Exclude(params string[] exclusions) => ExcludePaths(exclusions);

        public IBankAssetRegistrationStrategy ExcludeExtensions(params string[] exclusions) => this.SafeAdd(_extensionExclusions, exclusions);

        public IBankAssetRegistrationStrategy ExcludePaths(params string[] exclusions) => this.SafeAdd(_pathExclusions, exclusions);

        public IBankAssetRegistrationStrategy IncludeExtensions(params string[] inclusions) => this.SafeAdd(_extensionInclusions, inclusions);

        public IBankAssetRegistrationStrategy IncludePaths(params string[] inclusions) => this.SafeAdd(_pathInclusions, inclusions);

        public IList<string> Filters(Constants.FilterTypes filter) => this.GetFilters(filter, _extensionExclusions, _extensionInclusions, _extensionExclusions, _pathInclusions);

        public IList<BankEmbeddedResource> Register()
        {
            var allEmbeddedResources = Assembly.GetManifestResourceNames();
            var filteredEmbeddedResources = this.ApplyFilters(allEmbeddedResources);

            foreach (var embeddedResource in filteredEmbeddedResources)
            {
                var filename = embeddedResource.Remove(0, StartingPoint.Length + 1);
                var fileExtension = filename.Split('.').Last().ToLower();
                var bankResource = new BankEmbeddedResource
                {
                    Assembly = Assembly,
                    FileName = filename,
                    NameSpace = NameSpace,
                    ContentType = BankHelpers.MimeMappings.TryGetValue(fileExtension, out var mapping) ? mapping : "text/plain",
                    UrlPrepend = UrlPrepend
                };

                BankAssets.Register(bankResource);

                lock(_cacheLock) if (!_cache.ContainsKey(bankResource.ResourceKey)) _cache.Add(bankResource.ResourceKey, bankResource);
            }

            return _cache.Select(item => item.Value).ToList().AsReadOnly();
        }
    }
}
