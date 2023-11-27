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
            return BankAssets.ContainsKey(locator) || BankAssets.ContainsUrl(locator) || BankAssets.ContainsVirtualPath(locator);
        }

        /// <summary>
        /// Fetches the virtual file matching a locator value.
        /// </summary>
        /// <param name="locator">A resource key, resource url or resource virtual path</param>
        /// <returns></returns>
        public override VirtualFile GetFile(string locator)
        {
            return FileExistsInBank(locator)
                   ? new BankEmbeddedVirtualFile(BankAssets.GetByKey(locator) ?? BankAssets.GetByUrl(locator) ?? BankAssets.GetByVirtualPath(locator))
                   : base.GetFile(locator);
        }

        public override CacheDependency GetCacheDependency(string virtualPath, IEnumerable virtualPathDependencies, DateTime utcStart)
        {
            return FileExistsInBank(virtualPath) ? base.GetCacheDependency(virtualPath, virtualPathDependencies, utcStart) : null;
        }
    }
}
