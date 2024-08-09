using System;
using LightPath.Bank.ContentProcessors;
using LightPath.Bank.Interfaces;
using Microsoft.Ajax.Utilities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;

namespace LightPath.Bank.RegistrationStrategies;

/// <summary>
/// Asset registration strategy for Vite React library builds
/// </summary>
public class ViteReactLibStrategy : IBankAssetRegistrationStrategy
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

    public ViteReactLibStrategy(Assembly assembly, string nameSpace, string assetManifest = null, string urlPrepend = null)
    {
        Assembly = assembly;
        NameSpace = nameSpace;
        StartingPoint = assetManifest == null || assetManifest == default ? "manifest.json" : assetManifest;
        UrlPrepend = urlPrepend;
    }

    public BankEmbeddedResource this[string key] => _manifestMap.FirstOrDefault(item => string.Equals(item.Key, key, StringComparison.CurrentCultureIgnoreCase)).Value;

    public IBankAssetRegistrationStrategy Exclude(params string[] exclusions)
    {
        if (exclusions == null) return this;
            
        _exclusions.AddRange(exclusions.Select(x => x.ToLower()));

        return this;
    }

    public IList<BankEmbeddedResource> Register()
    {
        using var stream = Assembly.GetManifestResourceStream($"{Assembly.GetName().Name}.{NameSpace}..vite.{StartingPoint}");
        using var reader = stream == null ? null : new StreamReader(stream);
        var manifestJson = reader == null ? null : System.Web.Helpers.Json.Decode(reader.ReadToEnd());

        if (manifestJson == null) return new List<BankEmbeddedResource>().AsReadOnly();

        _manifestJson = manifestJson;

        foreach (var entry in manifestJson)
        {
            var entryConfig = entry.Value;
            var file = (string)entryConfig.file;
            var filePath = file.Split('/');
            var fileName = filePath.Last();
            var fileExtension = file.Split('.').Last().ToLower();
            var @namespace = $"{NameSpace}{(filePath.Length > 1 ? "." + string.Join(".", filePath.Where(p => p != filePath.Last())) : string.Empty)}";
            var contentType = BankHelpers.MimeMappings.TryGetValue(fileExtension, out var mapping) ? mapping : null;
            var resource = new BankEmbeddedResource
            {
                Assembly = Assembly,
                FileName = fileName,
                NameSpace = @namespace,
                ContentProcessors = fileExtension.EndsWith("css") ? new() { new ReactCssContentProcessor(Assembly, NameSpace, UrlPrepend) } : null,
                ContentType = contentType,
                UrlPrepend = UrlPrepend,
            };

            BankAssets.Register(resource);
            _manifestMap.Add(entry.Key, resource);
        }

        return _manifestMap.Select(item => item.Value).ToList().AsReadOnly();
    }
}