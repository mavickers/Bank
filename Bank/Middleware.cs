using Microsoft.Owin;
using System.Threading.Tasks;

namespace LightPath.Bank
{
    public class Middleware : OwinMiddleware
    {
        public Middleware(OwinMiddleware next) : base(next) { }

        public override Task Invoke(IOwinContext context)
        {
            var resource = Assets.GetByUrl(context.Request.Path.Value);

            if (resource?.ByteContents == null)
            {
                context.Response.StatusCode = 404;
                context.Response.Body = null;
            }
            else
            {
                context.Response.ContentType = resource.ContentType;
                context.Response.StatusCode = 200;
                context.Response.WriteAsync(resource.ByteContents);
            }

            return Task.CompletedTask;
        }

        public static bool Predicate(IOwinContext context) => Assets.ContainsUrl(context.Request.Path.Value);
    }
}
