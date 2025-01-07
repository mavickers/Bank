using System.ComponentModel;

namespace LightPath.Bank
{
    public class Constants
    {
        public enum FilterTypes
        {
            [Description("Extension Exclusions")] ExtensionExclusions,
            [Description("Extension Inclusions")] ExtensionInclusions,
            [Description("Path Exclusions")] PathExclusions,
            [Description("Path Inclusions")] PathInclusions
        }
    }
}
