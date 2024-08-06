using System;
using System.Reflection;

namespace LightPath.Bank.RegistrationStrategies
{
    [Obsolete("Switch to ReactViteRegistrationStrategy")]
    public class ReactViteStrategy : ReactViteRegistrationStrategy
    {
        public ReactViteStrategy(Assembly assembly, string nameSpace, string assetManifest = "manifest.json", string keyPrefix = "", string urlPrepend = null) : base(assembly, nameSpace, assetManifest, keyPrefix, urlPrepend) { }
    }
}
