using System.Web.Hosting;

namespace LightPath.Bank
{
    public class BankEmbeddedVirtualFile : VirtualFile
    {
        private readonly BankEmbeddedResource _resource;

        public BankEmbeddedVirtualFile(BankEmbeddedResource resource) : base(resource.VirtualPath)
        {
            _resource = resource;
        }

        public BankEmbeddedVirtualFile(string virtualPath) : base(virtualPath) { }

        public override System.IO.Stream Open() => BankHelpers.Open(_resource);
    }
}
