using LightPath.Bank;
using LightPath.Bank.ContentProcessors;
using System.Reflection;

namespace Tests.RegistrationStrategies
{
    public class ReactCRAStrategy
    {
        private readonly BankEmbeddedResource testStyle = new()
        {
            Assembly = Assembly.GetExecutingAssembly(),
            NameSpace = "TestAssets",
            FileName = "UrlTest.css",
            ContentType = "text/css",
            ContentProcessors = new() { new ReactCssContentProcessor(Assembly.GetExecutingAssembly(), "TestAssets") }
        };

        private readonly string testStyleContents = ".UrlTest1 { background-image: url(/Tests/TestAssets.one.two.three/image.jgp); } .UrlTest2 { background-image: url(/Tests/TestAssets.four.five/image.jgp); }";

        [Fact]
        public void Contents()
        {
            Assert.Equal(testStyleContents, testStyle.Contents.AsString());
        }
    }
}
