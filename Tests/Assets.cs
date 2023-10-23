using System.Reflection;
using LightPath.Bank;

namespace Tests
{
    public class Assets
    {
        [Fact]
        public void Register()
        {
            var resource = new BankEmbeddedResource
            {
                Assembly = Assembly.GetExecutingAssembly(),
                NameSpace = "TestAssets",
                FileName = "HelloWorld.js",
                ContentType = "application/javascript"
            };

            BankAssets.Register("hello-world-script", resource);

            Assert.True(BankAssets.ContainsKey("hello-world-script"));
            Assert.True(BankAssets.ContainsUrl("/Tests/TestAssets/HelloWorld.js"));
        }
    }
}