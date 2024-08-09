using LightPath.Bank.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace LightPath.Bank.ContentProcessors
{
    /// <summary>
    /// Adjust url refs in css so they reflect embedded namespacing.
    /// </summary>
    public class ReactCssContentProcessor : IBankAssetContentProcessor
    {
        private readonly Assembly _assembly;
        private readonly string _nameSpace;
        private readonly string _prepend;
        private static readonly string[] exclusions = new string[] { "http:", "https://" };

        public ReactCssContentProcessor(Assembly assembly, string nameSpace, string prepend = null)
        {
            _assembly = assembly;
            _nameSpace = nameSpace;
            _prepend = prepend;
        }

        public byte[] Process(byte[] content)
        {
            if (content == null) return null;
            if (_assembly == null) return content;
            if (string.IsNullOrWhiteSpace(_nameSpace)) return content;

            var urlRegex = new Regex("url(?:\\(['\"]?)(.*?)(?:['\"]?\\))");
            var oldText = content.AsString();
            var newText = oldText;
            var urlMatches = urlRegex.Matches(oldText);
            var urls = new Dictionary<string, string>();

            // iterate through the urls found in the file and build a dictionary
            // of refs to fix

            for (var i = 0; i < urlMatches.Count; i++)
            {
                if (urls.Keys.Contains(urlMatches[i].Value)) continue;
                if (exclusions.Any(exc => urlMatches[i].Value.ToLower().Contains(exc))) continue;

                var replacement = urlMatches[i].Value.Replace("\"", string.Empty);
                var filePath = replacement.Split('(', ')')[1].Split('/');
                var file = filePath.Last();
                var path = filePath.Take(filePath.Length - 1);

                urls.Add(urlMatches[i].Value, $"url({_prepend ?? string.Empty}/{_assembly.GetName().Name}/{_nameSpace}{string.Join(".", path)}/{file})");
            }

            // now iterate through the dictionary and fix the refs in the file

            foreach (var url in urls)
            {
                newText = newText.Replace(url.Key, url.Value);
            }

            return System.Text.Encoding.UTF8.GetBytes(newText);
        }
    }
}
