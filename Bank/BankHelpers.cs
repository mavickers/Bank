using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;

namespace LightPath.Bank
{
    public static class BankHelpers
    {
        private static readonly byte[] byteOrderMarker = { 0xEF, 0xBB, 0xBF };
        private static readonly List<string> UnclosedTags = new() { "link" };
        private static readonly List<string> SelfClosingTags = new() { "img" };
        private static readonly List<string> SupportedCssContentTypes = new() { "text/css" };
        private static readonly List<string> SupportedImageContentTypes = new() { "image/gif", "image/jpeg", "image/png", "image/tiff" };
        private static readonly List<string> SupportedScriptContentTypes = new() { "application/javascript", "text/javascript" };


        public static string AsString(this byte[] source)
        {
            // first, trim off the byte order marker if it's present

            var bytes = source[0] == byteOrderMarker[0] && source[1] == byteOrderMarker[1] && source[2] == byteOrderMarker[2] ? source.Skip(3).ToArray() : source;

            return System.Text.Encoding.UTF8.GetString(bytes);
        }

        public static byte[] GetEmbeddedBytes(Assembly assembly, string nameSpace, string fileName)
        {
            using var resourceStream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.{nameSpace}.{fileName}");
            using var memoryStream = resourceStream == null ? null : new MemoryStream();
            
            resourceStream?.CopyTo(memoryStream);
            
            var output = memoryStream?.ToArray();

            resourceStream?.Dispose();
            memoryStream?.Dispose();

            return output;
        }

        public static string GetEmbeddedString(Assembly assembly, string nameSpace, string fileName)
        {
            using var stream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.{nameSpace}.{fileName}");
            using var reader = stream == null ? null : new StreamReader(stream);

            var output = reader?.ReadToEnd();

            return output;
        }

        public static MvcHtmlString RenderEmbeddedResource(this HtmlHelper htmlHelper, string resourceKey)
        {
            if (string.IsNullOrWhiteSpace(resourceKey)) return MvcHtmlString.Create("<!-- unable to render embedded resource because the resourceKey was empty or null -->");
            if (!BankAssets.ContainsKey(resourceKey)) return MvcHtmlString.Create("<!-- unable to render embedded resource '{resourceKey}' because it was not found -->");

            return RenderEmbeddedResource(htmlHelper, BankAssets.GetByKey(resourceKey));
        }

        public static MvcHtmlString RenderEmbeddedResource(this HtmlHelper htmlHelper, BankEmbeddedResource resource)
        {
            if (resource == null) return MvcHtmlString.Create("<!-- embedded resource is null -->");
            if (resource.Exceptions.Any()) return MvcHtmlString.Create($"<!-- embedded resource '{(string.IsNullOrWhiteSpace(resource.FileName) ? "(NULL FILENAME)" : resource.FileName)}' contains exceptions, unable to render -->");
            if (SupportedCssContentTypes.Contains(resource.ContentType)) return RenderEmbeddedResource(resource, "link", "href", true);
            if (SupportedImageContentTypes.Contains(resource.ContentType)) return RenderEmbeddedResource(resource, "img", "src", true);
            if (SupportedScriptContentTypes.Contains(resource.ContentType)) return RenderEmbeddedResource(resource, "script", "src", false);

            return MvcHtmlString.Create($"<!-- unable to render embedded resource '{resource.Url}' because the content type is not supported -->");
        }

        private static MvcHtmlString RenderEmbeddedResource(BankEmbeddedResource resource, string tag, string urlAttribute, bool isSelfClosing = false)
        {
            if (resource == null) throw new ArgumentNullException(nameof(resource));

            var usableAttributes = resource.Attributes?.Where(attr => urlAttribute != attr.Key.ToLower()).ToDictionary(attr => attr.Key, attr => attr.Value) ?? new Dictionary<string, string>();
            var attributesString = string.Join(" ", usableAttributes.Select(attr => attr.Key + (string.IsNullOrWhiteSpace(attr.Value) ? string.Empty : $"=\"{attr.Value}\""))).Trim();
            var closingMarkup = UnclosedTags.Contains(tag) ? ">" : SelfClosingTags.Contains(tag) ? " />" : $"></{tag}>";

            attributesString = string.IsNullOrWhiteSpace(attributesString) ? string.Empty : $" {attributesString}";

            return MvcHtmlString.Create($"<{tag} {urlAttribute}=\"{(string.IsNullOrWhiteSpace(resource.UrlRenderPrepend) ? string.Empty : resource.UrlRenderPrepend)}{resource.Url}\"{attributesString}{closingMarkup}");
        }
    }
}
