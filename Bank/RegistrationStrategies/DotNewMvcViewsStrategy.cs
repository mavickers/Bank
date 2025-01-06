using System;
using System.Reflection;

namespace LightPath.Bank.RegistrationStrategies
{
    [Obsolete("Use Scoop strategy with .IncludeExtensions(\".cshtml\") instead")]
    public class DotNetMvcViewsStrategy : ScoopStrategy
    {
        public DotNetMvcViewsStrategy(Assembly assembly, string @namespace, string urlPrepend = null) : base(assembly, @namespace, urlPrepend)
        {
            IncludeExtensions(".cshtml");
        }
    }
}
