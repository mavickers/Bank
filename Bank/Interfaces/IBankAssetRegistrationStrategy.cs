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

        BankEmbeddedResource this[string key] { get; }

        /// <summary>
        /// Exclude file(s) from registration.
        /// </summary>
        /// <param name="exclusions"></param>
        /// <remarks>
        /// Exclusions can be any string (a filename or extension for example) that can
        /// be used to identify the resources that should be excluded from registration.
        /// </remarks>
        IBankAssetRegistrationStrategy Exclude(params string[] exclusions);

        /// <summary>
        /// Execute the registration process.
        /// </summary>
        /// <returns>A list of BankEmbeddedResource registered by the strategy</returns>
        IList<BankEmbeddedResource> Register();
    }
}
