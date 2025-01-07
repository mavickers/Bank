using LightPath.Bank;
using System.Reflection;

namespace Tests.RegistrationStrategies
{
    public class ViteReactLibStrategy
    {
        [Fact]
        public void Basic()
        {
            BankAssets.Clear();

            var assembly = Assembly.GetExecutingAssembly();
            var strategy = new LightPath.Bank.RegistrationStrategies.ViteReactLibStrategy(assembly, "StrategyTestAssets.ViteReactLibStrategy.ClientApp.dist");
            var assets = BankAssets.Register(strategy);
            var jsAsset = assets.FirstOrDefault(asset => asset.FileName.Equals("react-vite-test.umd.cjs"));
            var cssAsset = assets.FirstOrDefault(asset => asset.FileName.Equals("style.css"));

            Assert.NotNull(jsAsset);
            Assert.NotNull(cssAsset);

            var js = jsAsset.Contents?.AsString() ?? string.Empty;
            var css = cssAsset.Contents?.AsString() ?? string.Empty;

            Assert.NotEmpty(js);
            Assert.NotEmpty(css);

            Assert.Contains("return \"hello\"", js);
            Assert.Contains("noop-2", css);
            Assert.Contains("noop-3", css);

            Assert.NotNull(strategy["src/main.jsx"]);
            Assert.NotNull(strategy["style.css"]);

            Assert.Equal(js, strategy["src/main.jsx"].Contents.AsString());
            Assert.Equal(css, strategy["style.css"].Contents.AsString());
        }
    }
}
