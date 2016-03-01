using IdentityServer3.Core.Configuration;
using IdSrv.Config;
using Microsoft.Owin;
using Owin;


[assembly: OwinStartup(typeof(ExpenseTracker.IdSrv.Startup))]

namespace ExpenseTracker.IdSrv
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.Map("/identity", idSrv =>
            {
                idSrv.UseIdentityServer(new IdentityServerOptions
                {
                    SiteName = "Bachelor IdentityServer",
                    IssuerUri = "BachelorIdSrv.azurewebsites.net",

                    Factory = new IdentityServerServiceFactory()
                        .UseInMemoryUsers(Users.Get())
                        .UseInMemoryClients(Clients.Get())
                        .UseInMemoryScopes(Scopes.Get())
                });
            });
        }
 
    }
}