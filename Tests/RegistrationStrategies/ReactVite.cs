using LightPath.Bank;
using System.Reflection;

namespace Tests.RegistrationStrategies
{
    public class ReactVite
    {
        private readonly string htmlContent = "<html>hello, world</html>";
        private readonly string jsContent = "(function () { console.log('hello, world'); } ())";
        private readonly string cssContentA = ".hello-world-a { }";
        private readonly string cssContentB = ".hello-world-b { background: url(/Tests/StrategyTestAssets.ReactVite/'vite.svg') }";

        [Fact]
        public void Basic()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var assets = new LightPath.Bank.RegistrationStrategies.ReactViteRegistrationStrategy(assembly, "StrategyTestAssets.ReactVite.ClientApp.dist");

            BankAssets.Register(assets);

            Assert.Equal(4, BankAssets.All.Count);
            Assert.Equal(htmlContent, BankAssets.All.Skip(0).First().Value.Contents.AsString());
            Assert.Equal(jsContent, BankAssets.All.Skip(1).First().Value.Contents.AsString());
            Assert.Equal(cssContentA, BankAssets.All.Skip(2).First().Value.Contents.AsString());
            Assert.Equal(cssContentB, BankAssets.All.Skip(3).First().Value.Contents.AsString());
        }
    }
}
