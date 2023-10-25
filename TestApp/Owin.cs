using LightPath.Bank;
using Microsoft.Owin;
using Microsoft.Owin.Extensions;
using Owin;

[assembly: OwinStartup("TestApp.Owin", typeof(TestApp.Owin), "Configuration")]

namespace TestApp
{
    public class Owin
    {
        public void Configuration(IAppBuilder app)
        {
            app.Use(typeof(BankMiddleware));
            app.UseStageMarker(PipelineStage.Authenticate);
        }
    }
}