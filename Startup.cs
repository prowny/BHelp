[assembly: OwinStartupAttribute(typeof(BHelp.Startup))]
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