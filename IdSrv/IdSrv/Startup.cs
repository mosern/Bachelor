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
            app.Map("/identity", idsrvApp =>
            {
                idsrvApp.UseIdentityServer(new IdentityServerOptions
                {
                    //SigningCertificate = LoadCertificate(),

                    Factory = new IdentityServerServiceFactory()
                        .UseInMemoryUsers(Users.Get())
                        .UseInMemoryClients(Clients.Get())
                        .UseInMemoryScopes(Scopes.Get()),

                });
            });
           
        }

        X509Certificate2 LoadCertificate()
        {
            try
            {
                X509Store certStore = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                certStore.Open(OpenFlags.ReadOnly);
                X509Certificate2Collection certCollection = certStore.Certificates.Find(X509FindType.FindByThumbprint, "6B7ACC520305BFDB4F7252DAEB2177CC091FAAE1", false);
                // Get the first cert with the thumbprint
                if (certCollection.Count > 0)
                {
                    return certCollection[0];
                }

                return null;
            }
            catch(Exception e)
            {
                throw e;
            }
        }

    }
}