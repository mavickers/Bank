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
        private readonly BankEmbeddedResource style1;
        private readonly BankEmbeddedResource script1;
        private readonly BankEmbeddedResource script2;
        private readonly string script1Rendered = "<script src=\"/prepend/Tests/TestAssets/HelloWorld.js\"></script>";
        private readonly string script2Rendered = "<script src=\"/prepend/Tests/TestAssets/HelloWorld.js\" defer class=\"testing\"></script>";
        private readonly string style1Contents = ".HelloWorld { font-weight: 900 }";
        private readonly string style1Rendered = "<link href=\"/Tests/TestAssets/HelloWorld.css\" rel=\"stylesheet\">";

        public Assets()
        {
            style1 = new BankEmbeddedResource
            {
                Assembly = Assembly.GetExecutingAssembly(),
                NameSpace = "TestAssets",
                FileName = "HelloWorld.css",
                ContentType = "text/css"
            };
            script1 = new BankEmbeddedResource
            {
                Assembly = Assembly.GetExecutingAssembly(),
                NameSpace = "TestAssets",
                FileName = "HelloWorld.js",
                ContentType = "application/javascript",
                UrlRenderPrepend = "/prepend"
            };
            script2 = new BankEmbeddedResource
            {
                Assembly = Assembly.GetExecutingAssembly(),
                NameSpace = "TestAssets",
                FileName = "HelloWorld.js",
                ContentType = "application/javascript",
                UrlRenderPrepend = "/prepend",
                Attributes = { { "defer", string.Empty }, { "class", "testing" } }
            };
        }

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
            var html3 = htmlHelper.RenderEmbeddedResource(style1);
            var html3Content = style1.Contents.AsString();

            Assert.Equal(script1Rendered, html1.ToString());
            Assert.Equal(script2Rendered, html2.ToString());
            Assert.Equal(style1Rendered, html3.ToString());
            Assert.Equal(style1Contents, html3Content);
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