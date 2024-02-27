using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(BHelp.Startup))]
namespace BHelp
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}