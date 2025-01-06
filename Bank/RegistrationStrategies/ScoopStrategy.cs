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
        private readonly Dictionary<string, BankEmbeddedResource> _cache = new();
        private readonly List<string> _extensionInclusions = new();
        private readonly List<string> _pathExclusions = new();
        public IDictionary<string, BankEmbeddedResource> All => new ReadOnlyDictionary<string, BankEmbeddedResource>(_cache);
        public Assembly Assembly { get; }
        public string NameSpace { get; }
        public string StartingPoint => Assembly == null || string.IsNullOrWhiteSpace(NameSpace) ? null : $"{Assembly.GetName().Name}.{NameSpace}";
        public string UrlPrepend { get; }

        [Obsolete("Use ExcludePaths instead")]
        public IBankAssetRegistrationStrategy Exclude(params string[] exclusions) => ExcludePaths(exclusions);

        public IBankAssetRegistrationStrategy ExcludePaths(params string[] exclusions) => this.ExcludePaths(_pathExclusions, exclusions);

        public IBankAssetRegistrationStrategy IncludeExtensions(params string[] inclusions) => this.IncludeExtensions(_extensionInclusions, inclusions);

        public ScoopStrategy(Assembly assembly, string @namespace, string urlPrepend = null)
        {
            Assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
            NameSpace = @namespace ?? throw new ArgumentNullException(nameof(@namespace));
            UrlPrepend = urlPrepend;
        }

        public BankEmbeddedResource this[string key] => _cache.FirstOrDefault(resource => string.Equals(resource.Key, key, StringComparison.InvariantCultureIgnoreCase)).Value;

        public IList<BankEmbeddedResource> Register()
        {
            var allEmbeddedResources = Assembly.GetManifestResourceNames();
            var filteredEmbeddedResources = allEmbeddedResources.Where(res => res.StartsWith(StartingPoint) && (_extensionInclusions.Count == 0 || _extensionInclusions.Any(ext => res.ToLower().EndsWith(ext))));

            foreach (var embeddedResource in filteredEmbeddedResources)
            {
                if (_pathExclusions.Any(embeddedResource.ToLower().EndsWith)) continue;

                var bankResource = new BankEmbeddedResource
                {
                    Assembly = Assembly,
                    FileName = embeddedResource.Remove(0, StartingPoint.Length + 1),
                    NameSpace = NameSpace,
                    ContentType = BankHelpers.MimeMappings["cshtml"],
                    UrlPrepend = UrlPrepend
                };

                BankAssets.Register(bankResource);
                _cache.Add(bankResource.ResourceKey, bankResource);
            }

            return _cache.Select(item => item.Value).ToList().AsReadOnly();
        }
    }
}
