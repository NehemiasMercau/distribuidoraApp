using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(DistribuidoraAPI.Startup))]
namespace DistribuidoraAPI
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
