using System;
using System.Collections.Generic;

namespace LightPath.Bank.Models
{
    public class RenderModel
    {
        public string Tag { get; set; }
        public Constants.TagClosingTypes TagClosingType { get; set; }
        public string UrlAttribute { get; set; }
        public List<string> ContentTypes { get; set; }
        public Dictionary<string, string> ReservedAttributes { get; set; }

        public RenderModel(string tag, string urlAttribute, Constants.TagClosingTypes tagClosingType, List<string> contentTypes, Dictionary<string, string> reservedAttributes = null)
        {
            if (string.IsNullOrWhiteSpace(tag)) throw new ArgumentException("Tag cannot be null or empty.", nameof(tag));
            if (contentTypes == null || contentTypes.Count == 0) throw new ArgumentException("ContentTypes cannot be null or empty.", nameof(contentTypes));

            Tag = tag;
            ContentTypes = contentTypes;
            ReservedAttributes = reservedAttributes ?? new();
            TagClosingType = tagClosingType;
            UrlAttribute = urlAttribute;
        }
    }
}
