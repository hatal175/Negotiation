using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Negotiation.Startup))]
namespace Negotiation
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
