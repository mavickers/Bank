using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using LightPath.Bank.Builders;

namespace LightPath.Bank
{
    public static partial class BankHelpers
    {
        private static readonly List<string> UnclosedTags = new() { "link" };
        private static readonly List<string> SelfClosingTags = new() { "img" };
        private static readonly List<string> SupportedCssContentTypes = new() { "text/css" };
        private static readonly List<string> SupportedImageContentTypes = new() { "image/gif", "image/jpeg", "image/png", "image/tiff" };
        private static readonly List<string> SupportedScriptContentTypes = new() { "application/javascript", "text/javascript" };
        private static Dictionary<string, string> ReservedCssAttributes => new() { { "href", "#" }, { "rel", "stylesheet" } };
        private static Dictionary<string, string> ReservedImageAttributes => new() { { "src", "#" } };
        private static Dictionary<string, string> ReservedScriptAttributes => new() { { "src", "#" } };
        public static bool IsTextType(BankEmbeddedResource resource) => SupportedCssContentTypes.Contains(resource.ContentType) || SupportedScriptContentTypes.Contains(resource.ContentType);

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

        public static RenderBuilder EmbeddedResource(this HtmlHelper htmlHelper, string resourceKey) => new(htmlHelper, resourceKey);

        public static byte[] GetEmbeddedBytes(BankEmbeddedResource resource) => GetEmbeddedBytes(resource.Assembly, resource.NameSpace, resource.FileName);

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

        public static string GetEmbeddedString(BankEmbeddedResource resource) => GetEmbeddedString(resource.Assembly, resource.NameSpace, resource.FileName);

        //public static Stream Open(BankEmbeddedResource resource) => resource.Assembly.GetManifestResourceStream($"{resource.Assembly.GetName().Name}.{resource.NameSpace}.{resource.FileName}");
        public static Stream Open(BankEmbeddedResource resource) => new MemoryStream(resource.Contents);

        [Obsolete("Use RenderBuilder")]
        public static MvcHtmlString RenderEmbeddedResource(this HtmlHelper htmlHelper, string resourceKey) => htmlHelper.RenderEmbeddedResource(resourceKey, false);

        [Obsolete("Use RenderBuilder")]
        public static MvcHtmlString RenderEmbeddedResource(this HtmlHelper htmlHelper, string resourceKey, dynamic helperAttributes = null) => RenderEmbeddedResource(htmlHelper, resourceKey, false, helperAttributes);

        [Obsolete("Use RenderBuilder")]
        public static MvcHtmlString RenderEmbeddedResource(this HtmlHelper htmlHelper, string resourceKey, bool withCacheBuster = false, dynamic helperAttributes = null)
        {
            if (string.IsNullOrWhiteSpace(resourceKey)) return MvcHtmlString.Create("<!-- unable to render embedded resource because the resourceKey was empty or null -->");
            if (!BankAssets.ContainsKey(resourceKey)) return MvcHtmlString.Create("<!-- unable to render embedded resource '{resourceKey}' because it was not found -->");

            return RenderEmbeddedResource(htmlHelper, BankAssets.GetByKey(resourceKey), withCacheBuster, helperAttributes);
        }

        [Obsolete("Use RenderBuilder")]
        public static MvcHtmlString RenderEmbeddedResource(this HtmlHelper htmlHelper, BankEmbeddedResource resource, bool withCacheBuster = false, dynamic helperAttributes = null)
        {
            if (resource == null) return MvcHtmlString.Create("<!-- embedded resource is null -->");
            if (resource.Exceptions.Any()) return MvcHtmlString.Create($"<!-- embedded resource '{(string.IsNullOrWhiteSpace(resource.FileName) ? "(NULL FILENAME)" : resource.FileName)}' contains exceptions, unable to render -->");
            if (SupportedCssContentTypes.Contains(resource.ContentType)) return RenderEmbeddedResource(resource, "link", "href", ReservedCssAttributes, withCacheBuster, helperAttributes);
            if (SupportedImageContentTypes.Contains(resource.ContentType)) return RenderEmbeddedResource(resource, "img", "src", ReservedImageAttributes, withCacheBuster, helperAttributes);
            if (SupportedScriptContentTypes.Contains(resource.ContentType)) return RenderEmbeddedResource(resource, "script", "src", ReservedScriptAttributes, withCacheBuster, helperAttributes);

            return MvcHtmlString.Create($"<!-- unable to render embedded resource '{resource.Url}' because the content type is not supported -->");
        }

        private static MvcHtmlString RenderEmbeddedResource
        (
            BankEmbeddedResource resource,
            string tag,
            string urlAttribute,
            Dictionary<string, string> reservedAttributes = null,
            bool withCacheBuster = false,
            dynamic helperAttributes = null
        )
        {
            if (resource == null) throw new ArgumentNullException(nameof(resource));

            // concat the attributes for rendering into a new dictionary; we don't actually
            // want to add the attributes passed in from the render call to the embedded resource.
            // we also want to camelcase -> dashed conversion of the keys along the way
            // so that attributes such as "dataTest" are converted to "data-test" in the tag.


            var attributesToRender = (reservedAttributes ?? new Dictionary<string, string>())
                                      .AddRange(ConvertObjectToDictionary(helperAttributes) as Dictionary<string, string> ?? new Dictionary<string, string>())
                                      .SetOrAdd(urlAttribute, $"{resource.Url}{(withCacheBuster ? $"?{DateTime.Now.Ticks}" : string.Empty)}");

            foreach (var attr in resource.Attributes)
            {
                var attrDashed = ConvertCamelCaseToDashed(attr.Key);

                if (!attributesToRender.ContainsKey(attrDashed)) attributesToRender.Add(attrDashed, attr.Value);
            }

            var attributesString = string.Join(" ", attributesToRender.Select(attr => attr.Key + (string.IsNullOrWhiteSpace(attr.Value) ? string.Empty : $"=\"{attr.Value}\""))).Trim();
            var closingMarkup = UnclosedTags.Contains(tag) ? ">" : SelfClosingTags.Contains(tag) ? " />" : $"></{tag}>";

            return MvcHtmlString.Create($"<{tag} {attributesString}{closingMarkup}");
        }
    }
}
