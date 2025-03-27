using System.Linq;
using LightPath.Bank;
using System.Reflection;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using LightPath.Bank.RegistrationStrategies;

namespace TestApp
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            var imageResource = new BankEmbeddedResource
            {
                Assembly = Assembly.GetExecutingAssembly(),
                NameSpace = "Content",
                FileName = "ASPNetLogo.png",
                ContentType = "image/png",
                Attributes = { { "style", "height:50px" }, { "class", "doNothing" } }
            };

            var scriptResource1 = new BankEmbeddedResource
            {
                Assembly = Assembly.GetExecutingAssembly(),
                NameSpace = "Scripts",
                FileName = "HelloWorld.js",
                FacadeFileName = "GoodbyeWorld.js",
                ContentType = "application/javascript"
            };

            var scriptResource2 = new BankEmbeddedResource
            {
                Assembly = Assembly.GetExecutingAssembly(),
                NameSpace = "Scripts",
                FileName = "HelloInjected.js",
                ContentType = "application/javascript",
                Variables = { { "injected", "THIS IS A TEST OF THE EMERGENCY BROADCAST SYSTEM" } }
            };

            var scriptResource3 = new BankEmbeddedResource
            {
                Assembly = Assembly.GetAssembly(typeof(TestApp.Library.Constants)),
                NameSpace = "Scripts",
                FileName = "HelloWorldBundled.js",
                FacadeFileName = "HelloWorldBundledScript.js",
                ContentType = "application/javascript"
            };

            var scriptResource4 = new BankEmbeddedResource
            {
                Assembly = Assembly.GetExecutingAssembly(),
                NameSpace = "Scripts",
                FileName = "RandoScript.js",
                ContentType = "application/javascript"
            };

            BankAssets.Register("asp-net-logo", imageResource);
            BankAssets.Register("hello-world-script", scriptResource1);
            BankAssets.Register("hello-injected-script", scriptResource2);
            BankAssets.Register("hello-world-script-bundled", scriptResource3);
            BankAssets.Register("rando-script", scriptResource4);
            
            var testAssets = BankAssets.Register(new ViteReactLibStrategy(Assembly.GetAssembly(typeof(TestApp.Library.Constants)), "Scripts.ClientApp.dist"));

            HostingEnvironment.RegisterVirtualPathProvider(BankAssets.VirtualPathProvider);
            BundleTable.VirtualPathProvider = HostingEnvironment.VirtualPathProvider;
            BundleTable.EnableOptimizations = true;
            BundleTable.Bundles.Add(new ScriptBundle("~/bundles/hello-world").Include(BankAssets.GetByKey("hello-world-script-bundled")));
            BundleTable.Bundles.Add(new ScriptBundle("~/bundles/test").Include(BankAssets.GetByKey(testAssets.First().ResourceKey)));
        }
    }
}
