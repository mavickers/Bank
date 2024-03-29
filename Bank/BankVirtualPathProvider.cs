using System;
using System.Collections;
using System.Reflection;
using System.Web.Caching;
using System.Web.Hosting;

namespace LightPath.Bank
{
    public class BankVirtualPathProvider : VirtualPathProvider
    {
        private readonly Assembly assembly = typeof(BankVirtualPathProvider).Assembly;
        private readonly string[] _resourceNames;

        public BankVirtualPathProvider()
        {
            _resourceNames = assembly.GetManifestResourceNames();
        }

        /// <summary>
        /// Determines if the resource exists based on a locator value.
        /// </summary>
        /// <param name="locator">A resource key, resource url or resource virtual path</param>
        /// <returns></returns>
        public override bool FileExists(string locator)
        {
            return FileExistsInBank(locator) || base.FileExists(locator);
        }

        private bool FileExistsInBank(string locator)
        {
            return BankAssets.ContainsKey(locator) || BankAssets.ContainsUrl(locator) || BankAssets.ContainsVirtualPath(locator) || BankAssets.ContainsByResolver(locator);
        }

        private bool FileExistsInBank(string locator, out BankEmbeddedResource result)
        {
            if (BankAssets.TryGetByKey(locator, out result)) return true;
            if (BankAssets.TryGetByUrl(locator, out result)) return true;
            if (BankAssets.TryGetByVirtualPath(locator, out result)) return true;
            if (BankAssets.TryGetByResolver(locator, out result)) return true;

            return false;
        }

        /// <summary>
        /// Fetches the virtual file matching a locator value.
        /// </summary>
        /// <param name="locator">A resource key, resource url or resource virtual path</param>
        /// <returns></returns>
        public override VirtualFile GetFile(string locator)
        {
            return FileExistsInBank(locator, out var result) ? new BankEmbeddedVirtualFile(result) : base.GetFile(locator);
        }

        public override CacheDependency GetCacheDependency(string locator, IEnumerable virtualPathDependencies, DateTime utcStart)
        {
            // we are shimming around requests for bundles; if a bundle request fall through
            // to Previous.GetCacheDependency it is throwing an error.

            if (FileExistsInBank(locator) || locator.StartsWith("~/bundles", StringComparison.InvariantCultureIgnoreCase)) return null;
            
            return Previous.GetCacheDependency(locator, virtualPathDependencies, utcStart);
        }
    }
}
