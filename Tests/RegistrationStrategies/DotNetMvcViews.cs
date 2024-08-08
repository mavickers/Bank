using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using LightPath.Bank;

namespace Tests.RegistrationStrategies
{
    public class DotNetMvcViews
    {
        [Fact]
        public void Basic()
        {
            var @namespace = "StrategyTestAssets.DotNetMvc.Views";
            var assembly = Assembly.GetExecutingAssembly();
            var resourceNames = assembly.GetManifestResourceNames();
            var filteredResourceNames = resourceNames.Where(name => name.StartsWith($"{assembly.GetName().Name}.{@namespace}")).ToList();
            var assets = new LightPath.Bank.RegistrationStrategies.DotNetMvcViews(assembly, @namespace);

            BankAssets.Register(assets);

            foreach (var name in filteredResourceNames)
            {
                Assert.NotNull(BankAssets.All[name]);

                using var stream = assembly.GetManifestResourceStream(name);
                using var reader = stream == null ? null : new StreamReader(stream);
                var resourceContents = reader?.ReadToEnd();
                var assetContents = BankAssets.All[name].Contents.AsString();

                Assert.Equal(resourceContents, assetContents);
            }
        }
    }
}
