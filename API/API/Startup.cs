using Api.Classes;
using Microsoft.Owin;
using Owin;
using System.Web.Http;

namespace Api
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseResourceAuthorization(new AuthorizationManager());

            ConfigureAuth(app);

            app.UseWebApi(WebApiConfig.Register());
        }
    }
}
