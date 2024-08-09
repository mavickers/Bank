using LightPath.Bank.ContentProcessors;
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
        private readonly List<string> _exclusions = new();
        private dynamic _manifestJson;
        private readonly Dictionary<string, BankEmbeddedResource> _manifestMap = new();

        public IDictionary<string, BankEmbeddedResource> All => new ReadOnlyDictionary<string, BankEmbeddedResource>(_manifestMap);

        public Assembly Assembly { get; }
        /// <summary>
        /// The app root namespace of the React app.
        /// </summary>
        public string NameSpace { get; }
        public string StartingPoint { get; }
        public string UrlPrepend { get; }

        public ReactCRAStrategy(Assembly assembly, string nameSpace, string assetManifest = "asset-manifest.json", string urlPrepend = null)
        {
            Assembly = assembly;
            NameSpace = nameSpace;
            StartingPoint = assetManifest;
            UrlPrepend = urlPrepend;
        }

        public BankEmbeddedResource this[string key] => throw new NotImplementedException();

        public IBankAssetRegistrationStrategy Exclude(params string[] exclusions)
        {
            if (exclusions == null) return this;
            
            _exclusions.AddRange(exclusions.Select(x => x.ToLower()));

            return this;
        }

        public IList<BankEmbeddedResource> Register()
        {
            using var stream = Assembly.GetManifestResourceStream($"{Assembly.GetName().Name}.{NameSpace}.{StartingPoint}");
            using var reader = stream == null ? null : new StreamReader(stream);
            var manifestJson = reader == null ? null : System.Web.Helpers.Json.Decode(reader.ReadToEnd());

            if (manifestJson == null || manifestJson.files == null) return new List<BankEmbeddedResource>().AsReadOnly();

            _manifestJson = manifestJson;

            foreach (var file in manifestJson.files)
            {
                var key = (string)file.Key.ToString();
                var value = (string)file.Value.ToString();
                var path = value.Substring(1).Split('/');
                var filename = path.Last();
                var @namespace = $"{NameSpace}{(path.Length > 1 ? "." + string.Join(".", path.Where(p => p != path.Last())) : string.Empty)}";
                var extension = filename.Split('.').Last().ToLower();
                var contentType = BankHelpers.MimeMappings.TryGetValue(extension, out var mapping) ? mapping : null;

                if (_exclusions.Contains(extension)) continue;

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
                _manifestMap.Add(key, resource);
            }

            return _manifestMap.Select(item => item.Value).ToList().AsReadOnly();
        }
    }
}
