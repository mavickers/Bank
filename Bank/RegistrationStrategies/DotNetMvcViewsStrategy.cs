using LightPath.Bank.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LightPath.Bank.RegistrationStrategies
{
    public class DotNetMvcViewsStrategy : IBankAssetRegistrationStrategy
    {
        private readonly List<BankEmbeddedResource> _cache = new();
        private readonly List<string> _exclusions = new();
        public IDictionary<string, BankEmbeddedResource> All { get; }
        public Assembly Assembly { get; }
        public string NameSpace { get; }
        public string StartingPoint => Assembly == null || string.IsNullOrWhiteSpace(NameSpace) ? null : $"{Assembly.GetName().Name}.{NameSpace}";
        public string UrlPrepend { get; }
        public IBankAssetRegistrationStrategy Exclude(params string[] exclusions)
        {
            if (exclusions != null) _exclusions.AddRange(exclusions.Select(x => x));

            return this;
        }

        public DotNetMvcViewsStrategy(Assembly assembly, string @namespace, string urlPrepend = null)
        {
            Assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
            NameSpace = @namespace ?? throw new ArgumentNullException(nameof(@namespace));
            UrlPrepend = urlPrepend;
        }

        public BankEmbeddedResource this[string key] => _cache.FirstOrDefault(resource => resource.ResourceKey.ToLower().EndsWith(key.ToLower()));

        public IList<BankEmbeddedResource> Register()
        {
            var allEmbeddedResources = Assembly.GetManifestResourceNames();
            var filteredEmbeddedResources = allEmbeddedResources.Where(res => res.StartsWith(StartingPoint) && res.ToLower().EndsWith(".cshtml"));

            foreach (var embeddedResource in filteredEmbeddedResources)
            {
                if (_exclusions.Any(embeddedResource.EndsWith)) continue;

                var bankResource = new BankEmbeddedResource
                {
                    Assembly = Assembly,
                    FileName = embeddedResource.Remove(0, StartingPoint.Length + 1),
                    NameSpace = NameSpace,
                    ContentType = BankHelpers.MimeMappings["cshtml"],
                    UrlPrepend = UrlPrepend
                };

                BankAssets.Register(bankResource);
                _cache.Add(bankResource);
            }

            return _cache.AsReadOnly();
        }
    }
}
