using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(DN.App.Startup))]
namespace DN.App
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {            
        }
    }
}
