using LightPath.Bank;
using System.Reflection;

namespace Tests.RegistrationStrategies
{
    public class DotNetMvcViewsStrategy
    {
        [Fact]
        public void Basic()
        {
            var @namespace = "StrategyTestAssets.DotNetMvcStrategy.Views";
            var assembly = Assembly.GetExecutingAssembly();
            var resourceNames = assembly.GetManifestResourceNames();
            var filteredResourceNames = resourceNames.Where(name => name.StartsWith($"{assembly.GetName().Name}.{@namespace}")).ToList();
            var assets = new LightPath.Bank.RegistrationStrategies.DotNetMvcViewsStrategy(assembly, @namespace);

            BankAssets.Config.ThrowOnDuplicate = false;
            BankAssets.Register(assets);

            Assert.Equal(2, assets.All.Count);
            Assert.Equal(3, filteredResourceNames.Count);

            foreach (var asset in assets.All)
            {
                Assert.NotNull(BankAssets.All[asset.Key]);

                using var stream = assembly.GetManifestResourceStream(asset.Value.ResourceKey);
                using var reader = stream == null ? null : new StreamReader(stream);
                var resourceContents = reader?.ReadToEnd();
                var assetContents = BankAssets.All[asset.Key].Contents.AsString();

                Assert.Equal(resourceContents, assetContents);
            }
        }
    }
}
