using Microsoft.Owin;
using System.Threading.Tasks;

namespace LightPath.Bank
{
    public class BankMiddleware : OwinMiddleware
    {
        public BankMiddleware(OwinMiddleware next) : base(next) { }

        public override async Task Invoke(IOwinContext context)
        {
            var resource = BankAssets.GetByUrl(context.Request.Path.Value);

            if (resource?.Contents == null)
            {
                context.Response.StatusCode = 404;
                context.Response.Body = null;
            }
            else
            {
                context.Response.ContentType = resource.ContentType;
                context.Response.StatusCode = 200;
                await context.Response.WriteAsync(resource.Contents);
            }
        }

        public static bool ConditionsMatch(IOwinContext context) => BankAssets.ContainsUrl(context.Request.Path.Value);
    }
}
