using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.UI.WebControls;

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

        private static Dictionary<string, string> ConvertObjectToDictionary(object @object)
        {
            if (@object == null) return new Dictionary<string, string>();

            var props = @object.GetType().GetProperties();
            
            return props.ToDictionary(x => ConvertCamelCaseToDashed(x.Name), x => x.GetValue(@object, null)?.ToString());
        }

        private static string ConvertCamelCaseToDashed(string source)
        {
            return string.Concat(source.Select((x, i) => i > 0 && char.IsUpper(x) ? $"-{x}" : x.ToString())).ToLower();
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

        public static MvcHtmlString RenderEmbeddedResource(this HtmlHelper htmlHelper, string resourceKey, dynamic helperAttributes = null)
        {
            if (string.IsNullOrWhiteSpace(resourceKey)) return MvcHtmlString.Create("<!-- unable to render embedded resource because the resourceKey was empty or null -->");
            if (!BankAssets.ContainsKey(resourceKey)) return MvcHtmlString.Create("<!-- unable to render embedded resource '{resourceKey}' because it was not found -->");

            return RenderEmbeddedResource(htmlHelper, BankAssets.GetByKey(resourceKey), helperAttributes);
        }

        public static MvcHtmlString RenderEmbeddedResource(this HtmlHelper htmlHelper, BankEmbeddedResource resource, dynamic helperAttributes = null)
        {
            if (resource == null) return MvcHtmlString.Create("<!-- embedded resource is null -->");
            if (resource.Exceptions.Any()) return MvcHtmlString.Create($"<!-- embedded resource '{(string.IsNullOrWhiteSpace(resource.FileName) ? "(NULL FILENAME)" : resource.FileName)}' contains exceptions, unable to render -->");
            if (SupportedCssContentTypes.Contains(resource.ContentType)) return RenderEmbeddedResource(resource, "link", "href", helperAttributes, true);
            if (SupportedImageContentTypes.Contains(resource.ContentType)) return RenderEmbeddedResource(resource, "img", "src", helperAttributes, true);
            if (SupportedScriptContentTypes.Contains(resource.ContentType)) return RenderEmbeddedResource(resource, "script", "src", helperAttributes, false);

            return MvcHtmlString.Create($"<!-- unable to render embedded resource '{resource.Url}' because the content type is not supported -->");
        }

        private static MvcHtmlString RenderEmbeddedResource(BankEmbeddedResource resource, string tag, string urlAttribute, dynamic helperAttributes = null, bool isSelfClosing = false)
        {
            if (resource == null) throw new ArgumentNullException(nameof(resource));

            // concat the attributes for rendering into a new dictionary; we don't actually
            // want to add the attributes passed in from the render call to the embedded resource.
            // we also want to camelcase -> dashed conversion of the keys along the way
            // so that attributes such as "dataTest" are converted to "data-test" in the tag.

            var attributesToRender = ConvertObjectToDictionary(helperAttributes) as Dictionary<string, string> ?? new Dictionary<string, string>();

            foreach (var attr in resource.Attributes)
            {
                var attrDashed = ConvertCamelCaseToDashed(attr.Key);

                if (!attributesToRender.ContainsKey(attrDashed)) attributesToRender.Add(attrDashed, attr.Value);
            }

            var usableAttributes = attributesToRender.Where(attr => urlAttribute != attr.Key.ToLower()).ToDictionary(attr => attr.Key, attr => attr.Value);
            var attributesString = string.Join(" ", usableAttributes.Select(attr => attr.Key + (string.IsNullOrWhiteSpace(attr.Value) ? string.Empty : $"=\"{attr.Value}\""))).Trim();
            var closingMarkup = UnclosedTags.Contains(tag) ? ">" : SelfClosingTags.Contains(tag) ? " />" : $"></{tag}>";

            attributesString = string.IsNullOrWhiteSpace(attributesString) ? string.Empty : $" {attributesString}";

            return MvcHtmlString.Create($"<{tag} {urlAttribute}=\"{(string.IsNullOrWhiteSpace(resource.UrlRenderPrepend) ? string.Empty : resource.UrlRenderPrepend)}{resource.Url}\"{attributesString}{closingMarkup}");
        }
    }
}
