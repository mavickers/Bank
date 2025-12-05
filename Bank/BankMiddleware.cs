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

            context.Response.StatusCode = resource == null ? 404 : 200;
            context.Response.ContentType = resource == null ? context.Response.ContentType : resource.ContentType;

            if (resource == null) return;

            await (BankHelpers.IsTextType(resource) ? context.Response.WriteAsync(resource.Contents.AsString(resource.Variables)) : context.Response.WriteAsync(resource.Contents));
        }

        public static bool ConditionsMatch(IOwinContext context) => BankAssets.ContainsUrl(context.Request.Path.Value);
    }
}
