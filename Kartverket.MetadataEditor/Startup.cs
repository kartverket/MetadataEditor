using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Kartverket.MetadataEditor.Startup))]
namespace Kartverket.MetadataEditor
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
