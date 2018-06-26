using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(TenderSearch.Web.Startup))]
namespace TenderSearch.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
