﻿using LightPath.Bank;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

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

            var scriptResource = new BankEmbeddedResource
            {
                Assembly = Assembly.GetExecutingAssembly(),
                NameSpace = "Scripts",
                FileName = "HelloWorld.js",
                FacadeFileName = "GoodbyeWorld.js",
                ContentType = "application/javascript",
                IsCached = true
            };

            BankAssets.Register("hello-world-script", scriptResource);
        }
    }
}