using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.UI.WebControls;
using static System.Web.Optimization.Scripts;

namespace LightPath.Bank.Builders
{
    public class TagBundleBuilder
    {
        private readonly Dictionary<string, string> _attributes = new();
        private readonly string _bundlePath;
        private readonly HtmlHelper _htmlHelper;
        private bool _isEnabled = true;
        private bool _withCacheBuster = false;

        public TagBundleBuilder(HtmlHelper htmlHelper, string bundlePath)
        {
            _htmlHelper = htmlHelper;
            _bundlePath = bundlePath;
        }

        private static string ConvertCamelCaseToDashed(string source)
        {
            return string.Concat(source.Select((x, i) => i > 0 && char.IsUpper(x) ? $"-{x}" : x.ToString())).ToLower();
        }

        public MvcHtmlString Render()
        {
            if (!_isEnabled) return MvcHtmlString.Empty;
            if (string.IsNullOrWhiteSpace(_bundlePath)) return MvcHtmlString.Create(Constants.MvcMessages.NullResourceKey);

            var bundle = BundleTable.Bundles.GetBundleFor(_bundlePath);

            if (bundle == null) return MvcHtmlString.Create(Constants.MvcMessages.NullBundle);

            var tag = bundle is ScriptBundle ? "script" : bundle is StyleBundle ? "link" : "unknown";

            if (tag == "unknown") return MvcHtmlString.Create(Constants.MvcMessages.UnsupportedBundleType);

            return tag == "script" ? RenderScript() : RenderStyle();
        }

        private MvcHtmlString RenderScript()
        {
            var renderedPath = $"{_bundlePath}{(_withCacheBuster ? $"?v={DateTime.Now.Ticks}" : string.Empty)}";
            var baseHtml = Scripts.Render(renderedPath).ToHtmlString();
            var attributesToRender = new Dictionary<string, string>();

            foreach (var attr in _attributes)
            {
                var attrDashed = ConvertCamelCaseToDashed(attr.Key);

                if (!attributesToRender.ContainsKey(attrDashed)) attributesToRender.Add(attrDashed, attr.Value);
            }

            var attributesString = string.Join(" ", attributesToRender.Select(attr => attr.Key + (string.IsNullOrWhiteSpace(attr.Value) ? string.Empty : $"=\"{attr.Value}\""))).Trim();

            return new MvcHtmlString(baseHtml.Replace("></script>", $"{(string.IsNullOrWhiteSpace(attributesString) ? string.Empty : $" {attributesString}")}></script>"));
        }

        private MvcHtmlString RenderStyle()
        {
            var renderedPath = $"{_bundlePath}{(_withCacheBuster ? $"?{DateTime.Now.Ticks}" : string.Empty)}";
            var baseHtml = Styles.Render(renderedPath).ToHtmlString();
            var attributesToRender = new Dictionary<string, string>();

            foreach (var attr in _attributes)
            {
                var attrDashed = ConvertCamelCaseToDashed(attr.Key);

                if (!attributesToRender.ContainsKey(attrDashed)) attributesToRender.Add(attrDashed, attr.Value);
            }

            var attributesString = string.Join(" ", attributesToRender.Select(attr => attr.Key + (string.IsNullOrWhiteSpace(attr.Value) ? string.Empty : $"=\"{attr.Value}\""))).Trim();

            return new MvcHtmlString(baseHtml.Replace("/>", $"{(string.IsNullOrWhiteSpace(attributesString) ? string.Empty : $" {attributesString}")}/>"));
        }

        public TagBundleBuilder WithAttribute(string key)
        {
            _attributes.Add(key, null);

            return this;
        }

        public TagBundleBuilder WithAttribute(string key, string value)
        {
            _attributes.Add(key, value);

            return this;
        }

        /// <summary>
        /// Adds a cache buster to the resource URL.
        /// </summary>
        /// <returns></returns>
        public TagBundleBuilder WithCacheBuster()
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
        public TagBundleBuilder WithCondition(bool conditionFlag)
        {
            _isEnabled = conditionFlag;

            return this;
        }
    }
}
