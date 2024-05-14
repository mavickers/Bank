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
    public class ReactViteStrategy : IBankAssetRegistrationStrategy
    {
        private readonly List<string> _exclusions = new();
        private dynamic _manifestJson;
        private readonly Dictionary<string, BankEmbeddedResource> _manifestMap = new();

        public IDictionary<string, BankEmbeddedResource> All => new ReadOnlyDictionary<string, BankEmbeddedResource>(_manifestMap);

        public Assembly Assembly { get; }
        /// <summary>
        /// The app root namespace of the React app.
        /// </summary>
        public string KeyPrefix { get; }
        public string NameSpace { get; }
        public string StartingPoint { get; }
        public string UrlPrepend { get; }

        public ReactViteStrategy(Assembly assembly, string nameSpace, string assetManifest = "manifest.json", string keyPrefix = "", string urlPrepend = null)
        {
            Assembly = assembly;
            KeyPrefix = keyPrefix;
            NameSpace = nameSpace;
            StartingPoint = assetManifest;
            UrlPrepend = urlPrepend;
        }

        public IBankAssetRegistrationStrategy Exclude(params string[] exclusions)
        {
            if (exclusions == null) return this;
            
            _exclusions.AddRange(exclusions.Select(x => x.ToLower()));

            return this;
        }

        public bool Register()
        {
            using var stream = Assembly.GetManifestResourceStream($"{Assembly.GetName().Name}.{NameSpace}..vite.{StartingPoint}");
            using var reader = stream == null ? null : new StreamReader(stream);
            var manifestJson = reader == null ? null : System.Web.Helpers.Json.Decode(reader.ReadToEnd());

            if (manifestJson == null) return false;

            foreach (var entry in manifestJson)
            {
                // first parse out entry properties


                var files = new List<string>();
                var entryConfig = entry.Value;
                var entryName = (string)entryConfig.Name;
                var entryCss = ((object[])entryConfig.css).Select(obj => (string)obj).ToArray();

                // add the entry filename... are going to use the file specified as the entry key.

                var entryFile = (string)entry.Key;
                var entryExtension = entryFile.Split('.').Last().ToLower();

                if (!_exclusions.Contains(entryExtension)) files.Add(entryFile);

                // now add the entry app js... it should be the file property in the config

                var appFile = (string)entryConfig.file;
                var appExtension = appFile.Split('.').Last().ToLower();

                if (!_exclusions.Contains(appExtension)) files.Add(appFile);

                // now iterate through the css files and add

                foreach (var cssFile in entryCss)
                {
                    var cssFileExtensions = cssFile.Split('.').Last().ToLower();

                    if (!_exclusions.Contains(cssFileExtensions)) files.Add(cssFile);
                }

                // iterate through the built file list and create/add the resource

                foreach (var file in files)
                {
                    var fileIndex = files.IndexOf(file);
                    var filePath = file.Split('/');
                    var fileName = filePath.Last();
                    var fileExtension = file.Split('.').Last().ToLower();
                    var @namespace = $"{NameSpace}{(filePath.Length > 1 ? "." + string.Join(".", filePath.Where(p => p != filePath.Last())) : string.Empty)}";
                    var contentType = BankHelpers.MimeMappings.TryGetValue(fileExtension, out var mapping) ? mapping : null;
                    var resource = new BankEmbeddedResource
                    {
                        Assembly = Assembly,
                        NameSpace = @namespace,
                        FileName = fileName,
                        ContentType = contentType,
                        UrlPrepend = UrlPrepend,
                        ContentProcessors = fileExtension.EndsWith("css") ? new() { new ReactCRACssContentProcessor(Assembly, NameSpace, UrlPrepend) } : null
                    };
                    var resourceKeyFilename = fileExtension == "js" ? $"{entryName}-js-{fileIndex:0000}" :
                                              fileExtension == "css" ? $"{entryName}-css-{fileIndex:0000}" :
                                              $"{entryName}-{fileExtension}-{fileIndex:0000}";
                    var resourceKey = KeyPrefix.Trim() == "" ? $"EmbeddedResource-{Guid.NewGuid()}-({resource.Url})" : $"{KeyPrefix.Trim()}{resourceKeyFilename}";

                    BankAssets.Register(resourceKey, resource);
                    _manifestMap.Add(file, resource);
                }
            }

            return true;
        }
    }
}
