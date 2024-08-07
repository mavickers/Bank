using LightPath.Bank;
using LightPath.Bank.Interfaces;
using System.Reflection;

namespace Tests
{
    public class Resolvers
    {
        public class TestResolver1 : IBankEmbeddedResourceResolver
        {
            public bool IsResolving(string locator, BankEmbeddedResource resource)
            {
                return resource.UrlPrepend == $"/{locator}";
            }
        }

        public class TestResolver2 : IBankEmbeddedResourceResolver
        {
            public bool IsResolving(string locator, BankEmbeddedResource resource)
            {
                var pathParts = locator.Split('/');
                var fileName = pathParts?.Last() ?? string.Empty;
                var keyFromFileName = fileName?.Split('.')?.First() ?? string.Empty;

                return !string.IsNullOrWhiteSpace(keyFromFileName) && BankAssets.ContainsKey(keyFromFileName);
            }
        }

        private readonly BankEmbeddedResource script1 = new()
        {
            Assembly = Assembly.GetExecutingAssembly(),
            NameSpace = "TestAssets",
            FileName = "HelloWorld.js",
            ContentType = "application/javascript"
        };
        private readonly BankEmbeddedResource script2 = new()
        {
            Assembly = Assembly.GetExecutingAssembly(),
            NameSpace = "TestAssets",
            FileName = "HelloVariable.js",
            ContentType = "application/javascript",
            UrlPrepend = "/prepend",
            Attributes = { { "defer", string.Empty }, { "class", "testing" } },
            Variables = { { "injected", "Hello World!" } }
        };

        public Resolvers()
        {
            BankAssets.Config.ThrowOnDuplicate = false;
            BankAssets.Register("script-1", script1);
            BankAssets.Register("script-2", script2);
            BankAssets.RegisterResolver(typeof(TestResolver1));
            BankAssets.RegisterResolver(typeof(TestResolver2));
        }

        [Fact]
        public void BasicTest()
        {
            Assert.True(BankAssets.ContainsByResolver("prepend"));
            Assert.False(BankAssets.ContainsByResolver("nope"));

            Assert.NotNull(BankAssets.GetByResolver("prepend"));
            Assert.Null(BankAssets.GetByResolver("nope"));

            BankAssets.UnregisterResolver(typeof(TestResolver1));

            Assert.False(BankAssets.ContainsByResolver("prepend"));
            Assert.Null(BankAssets.GetByResolver("prepend"));
        }

        [Fact]
        public void VirtualPathProviderTest()
        {
            var vpp = new BankVirtualPathProvider();

            Assert.True(vpp.FileExists("prepend"));
            Assert.False(vpp.FileExists("nope"));

            Assert.True(BankAssets.ContainsKey("script-2"));
            Assert.True(vpp.FileExists("~/Views/Default/script-2.aspx"));
            Assert.False(vpp.FileExists("~/Views/Default/script-3.aspx"));
        }
    }
}
