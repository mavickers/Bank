using LightPath.Bank;
using System.Reflection;
using LightPath.Bank.ContentProcessors;

namespace Tests.RegistrationStrategies
{
    public class ReactCRA
    {
        private readonly BankEmbeddedResource testStyle = new()
        {
            Assembly = Assembly.GetExecutingAssembly(),
            NameSpace = "TestAssets",
            FileName = "UrlTest.css",
            ContentType = "text/css",
            ContentProcessors = new() { new ReactCRACssContentProcessor(Assembly.GetExecutingAssembly(), "TestAssets") }
        };

        private readonly string testStyleContents = ".UrlTest1 { background-image: url(/Tests/TestAssets.one.two.three/image.jgp); } .UrlTest2 { background-image: url(/Tests/TestAssets.four.five/image.jgp); }";

        [Fact]
        public void Contents()
        {
            Assert.Equal(testStyleContents, testStyle.Contents.AsString());
        }
    }
}
