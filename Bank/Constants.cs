using System.Collections.Generic;
using System.ComponentModel;

namespace LightPath.Bank
{
    public static class Constants
    {
        public enum FilterTypes
        {
            [Description("Extension Exclusions")] ExtensionExclusions,
            [Description("Extension Inclusions")] ExtensionInclusions,
            [Description("Path Exclusions")] PathExclusions,
            [Description("Path Inclusions")] PathInclusions
        }

        public struct MvcMessages
        {
            public const string KeyNotFound = "<!-- unable to render embedded resource because the resourceKey '{0}' was not found -->";
            public const string NullResource = "<!-- embedded resource is null -->";
            public const string NullResourceKey = "<!-- unable to render embedded resource because the resourceKey was empty or null -->";
            public const string UnsupportedContentType = "<!-- unable to render embedded resource '{0}' because the content type '{1}' is not supported -->";
        }

        public enum TagClosingTypes
        {
            [Description("Self Closing Tag")] SelfClosingTag,
            [Description("Unclosed Tag")] UnclosedTag,
            [Description("Closed Tag")] ClosedTag
        }

        public static readonly Dictionary<TagClosingTypes, string> TagClosures = new()
        {
            { TagClosingTypes.SelfClosingTag, "/>" },
            { TagClosingTypes.UnclosedTag, ">" },
            { TagClosingTypes.ClosedTag, "></{0}>" }
        };
    }
}
