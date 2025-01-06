using LightPath.Bank.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace LightPath.Bank.Extensions
{
    public static class IBankAssetRegistrationStrategyExtensions
    {
        public static IBankAssetRegistrationStrategy ExcludePaths(this IBankAssetRegistrationStrategy strategy, List<string> existingExclusions, params string[] exclusions)
        {
            if (exclusions != null) existingExclusions.AddRange(exclusions.Select(x => x));

            return strategy;
        }


        public static IBankAssetRegistrationStrategy IncludeExtensions(this IBankAssetRegistrationStrategy strategy, List<string> existingInclusions, params string[] newInclusions)
        {
            if (newInclusions != null) existingInclusions.AddRange(existingInclusions.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim().ToLower()));

            return strategy;
        }
    }
}
