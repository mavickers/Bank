using LightPath.Bank.ContentProcessors;
using LightPath.Bank.Extensions;
using LightPath.Bank.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;

namespace LightPath.Bank.RegistrationStrategies;

[Obsolete("No longer supported - use ViteReactLib strategy")]
public class ReactViteRegistrationStrategy : IBankAssetRegistrationStrategy
{
    private readonly List<string> _extensionExclusions = new();
    private readonly List<string> _extensionInclusions = new();
    private readonly List<string> _pathExclusions = new();
    private readonly List<string> _pathInclusions = new();
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

    public ReactViteRegistrationStrategy(Assembly assembly, string nameSpace, string assetManifest = "manifest.json", string keyPrefix = "", string urlPrepend = null)
    {
        Assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
        KeyPrefix = keyPrefix ?? throw new ArgumentNullException(nameof(keyPrefix));
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
        using var stream = Assembly.GetManifestResourceStream($"{Assembly.GetName().Name}.{NameSpace}..vite.{StartingPoint}");
        using var reader = stream == null ? null : new StreamReader(stream);
        var manifestJson = reader == null ? null : System.Web.Helpers.Json.Decode(reader.ReadToEnd());

        if (manifestJson == null) return new List<BankEmbeddedResource>().AsReadOnly();

        _manifestJson = manifestJson;

        foreach (var entry in manifestJson)
        {
            // first parse out entry properties

            var files = new List<string>();
            var entryConfig = entry.Value;
            var entryName = (string)entryConfig.Name;
            var entryCss = ((object[])entryConfig?.css ?? new object[] { }).Select(obj => (string)obj).ToArray();
            var entryAssets = ((object[])entryConfig?.assets ?? new object[] { }).Select(obj => (string)obj).ToArray();

            // add the entry filename... are going to use the file specified as the entry key.

            var entryFile = (string)entry.Key;

            if (this.PassesFilters(entryFile)) files.Add(entryFile);

            // now add the entry app js... it should be the file property in the config

            var appFile = (string)entryConfig.file;

            if (this.PassesFilters(appFile)) files.Add(appFile);

            // now iterate through the css files and add

            foreach (var cssFile in entryCss)
            {
                if (this.PassesFilters(cssFile)) files.Add(cssFile);
            }

            foreach (var assetFile in entryAssets)
            {
                if (this.PassesFilters(assetFile)) files.Add(assetFile);
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
                    ContentProcessors = fileExtension.EndsWith("css") ? new() { new ReactCssContentProcessor(Assembly, NameSpace, UrlPrepend) } : null
                };
                var resourceKeyFilename = fileExtension == "js" 
                                          ? $"{entryName}-js-{fileIndex:0000}" 
                                          : fileExtension == "css" 
                                              ? $"{entryName}-css-{fileIndex:0000}" 
                                              : $"{entryName}-{fileExtension}-{fileIndex:0000}";
                var resourceKey = KeyPrefix.Trim() == "" ? $"EmbeddedResource-{Guid.NewGuid()}-({resource.Url})" : $"{KeyPrefix.Trim()}{resourceKeyFilename}";

                BankAssets.Register(resourceKey, resource);
                _manifestMap.Add(file, resource);
            }
        }

        return _manifestMap.Select(item => item.Value).ToList().AsReadOnly();
    }
}