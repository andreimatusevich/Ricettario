using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Ricettario.Startup))]
namespace Ricettario
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
