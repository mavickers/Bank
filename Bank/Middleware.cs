using LightPath.Bank;
using Microsoft.Owin;
using System;
using System.Threading.Tasks;

namespace Bank
{
    public class Middleware : OwinMiddleware
    {
        public Middleware(OwinMiddleware next) : base(next)
        {
        }

        public override Task Invoke(IOwinContext context)
        {
            throw new NotImplementedException();
        }

        public static bool Predicate(IOwinContext context) => Assets.ContainsUrl(context.Request.Path.Value);
    }
}
