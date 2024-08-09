using LightPath.Bank;
using System.Reflection;

namespace Tests.RegistrationStrategies
{
    public class DotNetMvcViewsStrategy
    {
        [Fact]
        public void Basic()
        {
            var @namespace = "StrategyTestAssets.DotNetMvc.Views";
            var assembly = Assembly.GetExecutingAssembly();
            var resourceNames = assembly.GetManifestResourceNames();
            var filteredResourceNames = resourceNames.Where(name => name.StartsWith($"{assembly.GetName().Name}.{@namespace}")).ToList();
            var assets = new LightPath.Bank.RegistrationStrategies.DotNetMvcViewsStrategy(assembly, @namespace);

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
