using System;
using System.Linq;
using System.Web.Mvc;

namespace LightPath.Bank
{
    public class BankVirtualPathProviderViewEngine : System.Web.Mvc.VirtualPathProviderViewEngine
    {
        private readonly string[] locationFormats = { "~/LightPath.Bank/key/{0}" };

        public BankVirtualPathProviderViewEngine()
        {
            FileExtensions = new[] { "cshtml", "vbhtml", "aspx", "ascx" };

            AreaViewLocationFormats = locationFormats;
            AreaMasterLocationFormats = locationFormats;
            AreaPartialViewLocationFormats = locationFormats;
            MasterLocationFormats = locationFormats;
            PartialViewLocationFormats = locationFormats;
            ViewLocationFormats = locationFormats;
            VirtualPathProvider = BankAssets.VirtualPathProvider;
        }

        protected override IView CreatePartialView(ControllerContext controllerContext, string partialPath)
        {
            var key = partialPath.Split('/').Last().Split('?').First().Split('#').First();
            var resource = BankAssets.GetByKey(key);

            if (resource == null) throw new ArgumentException(nameof(partialPath));

            return resource.FileName.EndsWith(".cshtml") || resource.FileName.EndsWith(".vbhtml")
                ? new RazorView(controllerContext, resource.VirtualPath, null, false, null)
                : new WebFormView(controllerContext, resource.VirtualPath);
        }

        protected override IView CreateView(ControllerContext controllerContext, string viewPath, string masterPath)
        {
            var key = viewPath.Split('/').Last().Split('?').First().Split('#').First();
            var resource = BankAssets.GetByKey(key);

            if (resource == null) throw new ArgumentException(nameof(viewPath));

            return resource.FileName.EndsWith(".cshtml") || resource.FileName.EndsWith(".vbhtml")
               ? new RazorView(controllerContext, resource.VirtualPath, masterPath, false, null)
               : new WebFormView(controllerContext, resource.VirtualPath, masterPath);
        }

        protected override bool FileExists(ControllerContext controllerContext, string virtualPath)
        {
            if (string.IsNullOrWhiteSpace(virtualPath)) return false;

            var paths = virtualPath.Split('/');

            if (paths.Length < 4) return false;

            var key = paths.Last().Split('?').First().Split('#').First();
            var method = paths[2];

            return method.ToLower() == "key" && BankAssets.ContainsKey(key);
        }
    }
}
