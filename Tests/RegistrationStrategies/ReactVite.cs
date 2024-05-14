using LightPath.Bank;
using System.Reflection;

namespace Tests.RegistrationStrategies
{
    public class ReactVite
    {
        [Fact]
        public void Basic()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var assets = new LightPath.Bank.RegistrationStrategies.ReactViteStrategy(assembly, "StrategyTestAssets.ReactVite");
            var htmlContent = "<html>hello, world</html>";
            var jsContent = "(function () { console.log('hello, world'); } ())";
            var cssContentA = ".hello-world-a { }";
            var cssContentB = ".hello-world-b { background: url(/Tests/StrategyTestAssets.ReactVite/'vite.svg') }";

            BankAssets.Register(assets);

            Assert.Equal(4, BankAssets.All.Count);
            Assert.Equal(htmlContent, BankAssets.All.Skip(0).First().Value.Contents.AsString());
            Assert.Equal(jsContent, BankAssets.All.Skip(1).First().Value.Contents.AsString());
            Assert.Equal(cssContentA, BankAssets.All.Skip(2).First().Value.Contents.AsString());
            Assert.Equal(cssContentB, BankAssets.All.Skip(3).First().Value.Contents.AsString());
        }
    }
}
