using System.Collections.Generic;
using Microsoft.Owin;
using System.Threading.Tasks;

namespace LightPath.Bank
{
    public class BankMiddleware 
    {
        private readonly OwinMiddleware _next;

        public BankMiddleware(OwinMiddleware next)
        {
            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> args)
        {
            var context = new OwinContext(args);

            if (!ConditionsMatch(context))
            {
                await _next.Invoke(context);

                return;
            }

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
