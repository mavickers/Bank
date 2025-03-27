using LightPath.Bank.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using static LightPath.Bank.Constants.TagClosingTypes;

namespace LightPath.Bank.Builders
{
    public class RenderBuilder
    {
        private readonly Dictionary<string, string> _attributes = new();
        private readonly HtmlHelper _htmlHelper;
        private readonly string _resourceKey;
        private bool _withCacheBuster = false;
        private bool _isEnabled = true;
        private static readonly List<RenderModel> _renderTypes = new();

        static RenderBuilder()
        {
            _renderTypes.Add(new RenderModel("link", "href", UnclosedTag, new List<string> { "text/css" }, new Dictionary<string, string> { { "href", "#" }, { "rel", "stylesheet" } }));
            _renderTypes.Add(new RenderModel("img", "src", SelfClosingTag, new List<string> { "image/gif", "image/jpeg", "image/png", "image/tiff" }, new Dictionary<string, string> { { "src", "#" } }));
            _renderTypes.Add(new RenderModel("script", "src", ClosedTag, new List<string> { "application/javascript", "text/javascript" }, new Dictionary<string, string> { { "src", "#" } }));
        }

        public RenderBuilder(HtmlHelper htmlHelper, string resourceKey)
        {
            _htmlHelper = htmlHelper;
            _resourceKey = resourceKey;
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

        public MvcHtmlString Render()
        {
            if (!_isEnabled) return MvcHtmlString.Empty;
            if (string.IsNullOrWhiteSpace(_resourceKey)) return MvcHtmlString.Create(Constants.MvcMessages.NullResourceKey);
            if (!BankAssets.ContainsKey(_resourceKey)) return MvcHtmlString.Create(string.Format(Constants.MvcMessages.KeyNotFound, _resourceKey));

            var asset = BankAssets.GetByKey(_resourceKey);

            if (asset == null) return MvcHtmlString.Create(Constants.MvcMessages.NullResource);
            if (asset.Exceptions.Any()) return MvcHtmlString.Create($"<!-- embedded resource '{(string.IsNullOrWhiteSpace(asset.FileName) ? "(NULL FILENAME)" : asset.FileName)}' contains exceptions, unable to render -->");
            if (!_renderTypes.Any(model => model.ContentTypes.Contains(asset.ContentType))) return MvcHtmlString.Create($"<!-- unable to render embedded resource '{asset.Url}' because the content type '{asset.ContentType}' is not supported -->");

            return RenderEmbeddedResource(asset);
        }

        private MvcHtmlString RenderEmbeddedResource(BankEmbeddedResource resource)
        {
            if (resource == null) return new MvcHtmlString(Constants.MvcMessages.NullResource);

            var renderType = _renderTypes.FirstOrDefault(model => model.ContentTypes.Contains(resource.ContentType));

            if (renderType == null) return MvcHtmlString.Create(string.Format(Constants.MvcMessages.UnsupportedContentType, resource.Url, resource.ContentType));

            // concat the attributes for rendering into a new dictionary; we don't actually
            // want to add the attributes passed in from the render call to the embedded resource.
            // we also want to camelcase -> dashed conversion of the keys along the way
            // so that attributes such as "dataTest" are converted to "data-test" in the tag.

            var attributesToRender = 
                (renderType.ReservedAttributes ?? new Dictionary<string, string>())
                .AddRange(resource.Attributes ?? new Dictionary<string, string>())
                .AddRange(_attributes ?? new Dictionary<string, string>())
                .SetOrAdd(renderType.UrlAttribute, $"{resource.Url}{(_withCacheBuster ? $"?{DateTime.Now.Ticks}" : string.Empty)}");

            foreach (var attr in resource.Attributes)
            {
                var attrDashed = ConvertCamelCaseToDashed(attr.Key);

                if (!attributesToRender.ContainsKey(attrDashed)) attributesToRender.Add(attrDashed, attr.Value);
            }

            var attributesString = string.Join(" ", attributesToRender.Select(attr => attr.Key + (string.IsNullOrWhiteSpace(attr.Value) ? string.Empty : $"=\"{attr.Value}\""))).Trim();

            return MvcHtmlString.Create($"<{renderType.Tag} {attributesString}{string.Format(Constants.TagClosures[renderType.TagClosingType], renderType.Tag)}");
        }

        public RenderBuilder WithAttribute(string key)
        {
            _attributes.Add(key, null);

            return this;
        }

        public RenderBuilder WithAttribute(string key, string value)
        {
            _attributes.Add(key, value);

            return this;
        }

        /// <summary>
        /// Adds a cache buster to the resource URL.
        /// </summary>
        /// <returns></returns>
        public RenderBuilder WithCacheBuster()
        {
            _withCacheBuster = true;

            return this;
        }

        /// <summary>
        /// Conditionally enable or disable the rendering of the resource.
        /// </summary>
        /// <param name="conditionFlag"></param>
        /// <returns></returns>
        /// <remarks>
        /// If the conditionFlag is false, the resource will not be rendered. The
        /// default state of the builder is enabled.
        /// </remarks>
        public RenderBuilder WithCondition(bool conditionFlag)
        {
            _isEnabled = conditionFlag;

            return this;
        }
    }
}
