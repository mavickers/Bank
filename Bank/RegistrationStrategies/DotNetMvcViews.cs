using LightPath.Bank.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LightPath.Bank.RegistrationStrategies
{
    public class DotNetMvcViews : IBankAssetRegistrationStrategy
    {
        private readonly List<string> _exclusions = new();
        public IDictionary<string, BankEmbeddedResource> All { get; }
        public Assembly Assembly { get; }
        public string KeyPrefix { get; }
        public string NameSpace { get; }
        public string StartingPoint => Assembly == null || string.IsNullOrWhiteSpace(NameSpace) ? null : $"{Assembly.GetName().Name}.{NameSpace}";
        public string UrlPrepend { get; }
        public IBankAssetRegistrationStrategy Exclude(params string[] exclusions)
        {
            if (exclusions != null) _exclusions.AddRange(exclusions.Select(x => x));

            return this;
        }

        public DotNetMvcViews(Assembly assembly, string @namespace, string urlPrepend = null, string keyPrefix = null)
        {
            Assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
            KeyPrefix = string.IsNullOrWhiteSpace(keyPrefix) ? null : keyPrefix;
            NameSpace = @namespace ?? throw new ArgumentNullException(nameof(@namespace));
            UrlPrepend = urlPrepend;
        }

        public bool Register()
        {
            var allEmbeddedResources = Assembly.GetManifestResourceNames();
            var filteredEmbeddedResources = allEmbeddedResources.Where(res => res.StartsWith(StartingPoint) && res.ToLower().EndsWith(".cshtml"));

            foreach (var embeddedResource in filteredEmbeddedResources)
            {
                var bankResource = new BankEmbeddedResource
                {
                    Assembly = Assembly,
                    FileName = embeddedResource.Remove(0, StartingPoint.Length + 1),
                    NameSpace = NameSpace,
                    ContentType = BankHelpers.MimeMappings["cshtml"],
                    ResourceKey = embeddedResource,
                    UrlPrepend = UrlPrepend
                };

                BankAssets.Register(bankResource);
            }

            return true;
        }
    }
}
