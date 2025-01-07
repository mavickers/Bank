using System.Collections.Generic;
using System.Reflection;

namespace LightPath.Bank.Interfaces
{
    public interface IBankAssetRegistrationStrategy
    {
        /// <summary>
        /// All resources registered through this strategy instance.
        /// </summary>
        IDictionary<string, BankEmbeddedResource> All { get; }

        Assembly Assembly { get; }
        string NameSpace { get; }
        string StartingPoint { get; }
        string UrlPrepend { get; }

        IList<string> Filters(Constants.FilterTypes filter);

        BankEmbeddedResource this[string key] { get; }

        /// <summary>
        /// Exclude extensions from registration
        /// </summary>
        /// <param name="exclusions"></param>
        /// <returns>The same registration strategy instance</returns>
        /// <remarks>
        /// Each file extension should be prepended with a period. Exclusions override inclusions!
        /// </remarks>
        IBankAssetRegistrationStrategy ExcludeExtensions(params string[] exclusions);

        /// <summary>
        /// Exclude paths from registration.
        /// </summary>
        /// <param name="exclusions"></param>
        /// <returns>The same registration strategy instance</returns>
        /// <remarks>
        /// Path exclusions can be any string (a filename or extension for example) that can
        /// be used to identify the resources that should be excluded from registration. If the
        /// exclusion string is found anywhere in the path, the path will be excluded. Path
        /// exclusions will override inclusions!
        /// </remarks>
        IBankAssetRegistrationStrategy ExcludePaths(params string[] exclusions);

        /// <summary>
        /// Include extensions in registration
        /// </summary>
        /// <param name="inclusions"></param>
        /// <returns>The same registration strategy instance</returns>
        /// <remarks>
        /// Each file extension should be prepended with a period.
        /// </remarks>
        IBankAssetRegistrationStrategy IncludeExtensions(params string[] inclusions);

        /// <summary>
        /// Include paths in registration
        /// </summary>
        /// <param name="inclusions"></param>
        /// <returns>The same registration strategy instance</returns>
        /// <remarks>
        /// Path inclusions - this works like a reverse exclusion! Only the paths specified here
        /// will be included.
        /// </remarks>
        IBankAssetRegistrationStrategy IncludePaths(params string[] inclusions);

        /// <summary>
        /// Execute the registration process.
        /// </summary>
        /// <returns>A list of BankEmbeddedResource registered by the strategy</returns>
        IList<BankEmbeddedResource> Register();
    }
}
