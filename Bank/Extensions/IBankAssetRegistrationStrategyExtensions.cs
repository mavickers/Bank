using LightPath.Bank.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using static LightPath.Bank.Constants.FilterTypes;

namespace LightPath.Bank.Extensions
{
    public static class IBankAssetRegistrationStrategyExtensions
    {
        private static IList<string> _emptyStringList => new List<string>().AsReadOnly();

        public static IEnumerable<string> ApplyFilters(this IBankAssetRegistrationStrategy strategy, string[] resources) =>
            resources?.Where(resource => resource.StartsWith(strategy.StartingPoint, StringComparison.InvariantCultureIgnoreCase))
                      .Where(resource => PassesFilters(strategy, resource)) ?? new List<string>().AsReadOnly();

        public static bool PassesFilters(this IBankAssetRegistrationStrategy strategy, string filename)
        {
            if (strategy.Filters(ExtensionExclusions).Any(filter => filename.EndsWith(filter, StringComparison.InvariantCultureIgnoreCase))) return false;
            if (strategy.Filters(PathExclusions).Any(filter => filename.ToLower().Contains(filter.ToLower()))) return false;
            if (strategy.Filters(ExtensionInclusions).Any() && strategy.Filters(ExtensionInclusions).All(filter => !filename.EndsWith(filter, StringComparison.InvariantCultureIgnoreCase))) return false;
            if (strategy.Filters(PathInclusions).Any() && strategy.Filters(PathInclusions).All(filter => !filename.ToLower().Contains(filter.ToLower()))) return false;

            return true;
        }

        public static IList<string> GetFilters
        (
            this IBankAssetRegistrationStrategy strategy,
            Constants.FilterTypes filter,
            List<string> extensionExclusions,
            List<string> extensionInclusions,
            List<string> pathExclusions,
            List<string> pathInclusions
        )
        {
            return filter == ExtensionExclusions ? extensionExclusions?.AsReadOnly() ?? _emptyStringList :
                   filter == ExtensionInclusions ? extensionInclusions?.AsReadOnly() ?? _emptyStringList :
                   filter == PathExclusions ? pathExclusions?.AsReadOnly() ?? _emptyStringList :
                   filter == PathInclusions ? pathInclusions?.AsReadOnly() ?? _emptyStringList :
                   _emptyStringList;
        }


        public static IBankAssetRegistrationStrategy SafeAdd(this IBankAssetRegistrationStrategy strategy, List<string> baseList, params string[] newItems)
        {
            if (newItems != null) baseList?.AddRange(newItems.Where(item => !string.IsNullOrWhiteSpace(item)).Select(item => item.Trim().ToLower()));

            return strategy;
        }
    }
}
