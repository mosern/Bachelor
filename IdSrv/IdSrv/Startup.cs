using IdentityServer3.Core.Configuration;
using IdSrv.Config;
using Microsoft.Owin;
using Owin;
using System;
using System.Security.Cryptography.X509Certificates;

[assembly: OwinStartupAttribute(typeof(IdSrv.Startup))]

namespace IdSrv
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            
            var options = new IdentityServerOptions()
                {
                    SigningCertificate = LoadCertificate(),

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

        X509Certificate2 LoadCertificate()
        {
            try
            {
                return new X509Certificate2(string.Format(@"{0}\Certificates\idsrv3test.pfx", AppDomain.CurrentDomain.BaseDirectory), "idsrv3test");
            }
            catch(Exception e)
            {
                throw new Exception(e.Message + string.Format(@"{0}\Certificates\idsrv3test.pfx", AppDomain.CurrentDomain.BaseDirectory));
            }
        }

    }
}