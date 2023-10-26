using Microsoft.Owin;
using Owin;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LightPath.Bank
{
    public static class BankMiddleware
    {
        public static IAppBuilder MapBankMiddleware(this IAppBuilder app)
        {
            app.MapWhen(ConditionsMatch, builder => builder.Use<BankMiddlewareImpl>());

            return app;
        }

        public static IAppBuilder UseBankMiddleware(this IAppBuilder app)
        {
            app.Use(typeof(BankMiddlewareImpl));

            return app;
        }

        public class BankMiddlewareImpl
        {
            private readonly OwinMiddleware _next;

            public BankMiddlewareImpl(OwinMiddleware next) => _next = next;

            public async Task Invoke(IDictionary<string, object> args) => await ServeResource(new OwinContext(args));
        }

        private static async Task ServeResource(OwinContext context)
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
