using IdentityServer3.Core.Configuration;
using IdSrv.Config;
using Microsoft.Owin;
using Owin;


[assembly: OwinStartupAttribute(typeof(IdSrv.Startup))]

namespace IdSrv
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            
            var options = new IdentityServerOptions()
                {
                    Factory = new IdentityServerServiceFactory()
                        .UseInMemoryUsers(Users.Get())
                        .UseInMemoryClients(Clients.Get())
                        .UseInMemoryScopes(Scopes.Get())
                };

            app.Map("/identity", idsrv =>
                {
                    idsrv.UseIdentityServer(options);
                });

        }
 
    }
}