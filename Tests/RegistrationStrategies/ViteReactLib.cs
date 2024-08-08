using LightPath.Bank;
using System.Reflection;

namespace Tests.RegistrationStrategies
{
    public class ViteReactLib
    {
        [Fact]
        public void Basic()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var assets = new LightPath.Bank.RegistrationStrategies.ViteReactLib(assembly, "StrategyTestAssets.ViteReactLib.ClientApp.dist", "manifest.json", "ClientApp");

            BankAssets.Register(assets);

            var js = BankAssets.All.FirstOrDefault(asset => asset.Value.FileName.Equals("react-vite-test.umd.cjs")).Value?.Contents?.AsString() ?? string.Empty;
            var css = BankAssets.All.FirstOrDefault(asset => asset.Value.FileName.Equals("style.css")).Value?.Contents?.AsString() ?? string.Empty;

            Assert.NotEmpty(js);
            Assert.NotEmpty(css);

            Assert.Contains("return \"hello\"", js);
            Assert.Contains("noop-2", css);
            Assert.Contains("noop-3", css);

            Assert.NotNull(assets["src/main.jsx"]);
            Assert.NotNull(assets["style.css"]);

            Assert.Equal(js, assets["src/main.jsx"].Contents.AsString());
            Assert.Equal(css, assets["style.css"].Contents.AsString());
        }
    }
}
