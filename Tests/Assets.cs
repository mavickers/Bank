using LightPath.Bank;
using Moq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Tests
{
    public class Assets
    {
        private readonly BankEmbeddedResource style1 = new()
        {
            Assembly = Assembly.GetExecutingAssembly(),
            NameSpace = "TestAssets",
            FileName = "HelloWorld.css",
            ContentType = "text/css"
        };
        private readonly BankEmbeddedResource script1 = new()
        {
            Assembly = Assembly.GetExecutingAssembly(),
            NameSpace = "TestAssets",
            FileName = "HelloWorld.js",
            ContentType = "application/javascript",
            UrlPrepend = "/prepend"
        };
        private readonly BankEmbeddedResource script2 = new()
        {
            Assembly = Assembly.GetExecutingAssembly(),
            NameSpace = "TestAssets",
            FileName = "HelloWorld.js",
            ContentType = "application/javascript",
            UrlPrepend = "/prepend",
            Attributes = { { "defer", string.Empty }, { "class", "testing" } }
        };
        private readonly BankEmbeddedResource script3 = new()
        {
            Assembly = Assembly.GetExecutingAssembly(),
            NameSpace = "TestAssets",
            FileName = "HelloVariable.js",
            ContentType = "application/javascript",
            UrlPrepend = "/prepend",
            Attributes = { { "defer", string.Empty }, { "class", "testing" } },
            Variables = { { "injected", "Hello World!" } }
        };


        private readonly string script1Rendered = "<script src=\"/prepend/Tests/TestAssets/HelloWorld.js\"></script>";
        private readonly string script2Rendered = "<script src=\"/prepend/Tests/TestAssets/HelloWorld.js\" defer class=\"testing\"></script>";
        private readonly string script3Rendered = "<script src=\"/prepend/Tests/TestAssets/HelloVariable.js\" defer class=\"testing\"></script>";
        private readonly string script3Contents = "alert('Hello World!');";
        private readonly string style1Contents = ".HelloWorld { font-weight: 900 }";
        private readonly string style1Rendered = "<link href=\"/Tests/TestAssets/HelloWorld.css\" rel=\"stylesheet\">";

        [Fact]
        public void Register()
        {
            BankAssets.Register("hello-world-script", script1);

            Assert.True(BankAssets.ContainsKey("hello-world-script"));
            Assert.True(BankAssets.ContainsUrl("/Tests/TestAssets/HelloWorld.js"));
            Assert.NotNull(BankAssets.GetByKey("hello-world-script").Contents);
        }

        [Fact]
        public void Render()
        {
            var htmlHelper = CreateHtmlHelper(new ViewDataDictionary());
            var html1 = htmlHelper.RenderEmbeddedResource(script1);
            var html2 = htmlHelper.RenderEmbeddedResource(script2);
            var html3 = htmlHelper.RenderEmbeddedResource(script3);
            var html3Content = script3.Contents.AsString(script3.Variables);
            var html4 = htmlHelper.RenderEmbeddedResource(style1);
            var html4Content = style1.Contents.AsString();

            Assert.Equal(script1Rendered, html1.ToString());
            Assert.Equal(script2Rendered, html2.ToString());
            Assert.Equal(script3Rendered, html3.ToString());
            Assert.Equal(script3Contents, html3Content);
            Assert.Equal(style1Rendered, html4.ToString());
            Assert.Equal(style1Contents, html4Content);
        }

        private static HtmlHelper CreateHtmlHelper(ViewDataDictionary viewData)
        {
            var httpContextBase = new Mock<HttpContextBase>().Object;
            var controllerBase = new Mock<ControllerBase>().Object;
            var controllerContext = new ControllerContext(httpContextBase, new RouteData(), controllerBase);
            var view = new Mock<IView>().Object;
            var viewContext = new Mock<ViewContext>(controllerContext, view, viewData, new TempDataDictionary(), new StreamWriter(new MemoryStream()));
            var dataContainer = new Mock<IViewDataContainer>();

            dataContainer.Setup(c => c.ViewData).Returns(viewData);

            return new HtmlHelper(viewContext.Object, dataContainer.Object);
        }
    }
}