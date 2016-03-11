using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Services;
using IdSrv.Config;
using IdSrv.Services;
using Microsoft.Owin;
using Microsoft.Owin.Security.Google;
using Owin;
using Serilog;
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
                var factory = Database.Configure("Bachelor");

                var userService = new LocalUserService();

                factory.UserService = new Registration<IUserService>(resolver => userService);

                idsrvApp.UseIdentityServer(new IdentityServerOptions
                {
                    AuthenticationOptions = new AuthenticationOptions()
                    {
                        EnablePostSignOutAutoRedirect = true,
                        IdentityProviders = ConfigureIdentityProviders
                    },

                    SigningCertificate = LoadCertificate(),

                    Factory = factory,

                    

                //Factory = new IdentityServerServiceFactory()
                //    .UseInMemoryUsers(Users.Get())
                //   .UseInMemoryClients(Clients.Get())
                //    .UseInMemoryScopes(Scopes.Get()),

            });
            });

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Trace()
                .CreateLogger();

        }

        private void ConfigureIdentityProviders(IAppBuilder app, string signInAsType)
        {
            app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions
            {
                AuthenticationType = "Google",
                Caption = "Sign-in with Google",
                SignInAsAuthenticationType = signInAsType,

                ClientId = "112937482750-ql6c3ueds1f5h85bm1clqvlk9vqvcer0.apps.googleusercontent.com",
                ClientSecret = "k56_hupJKDoLWk58wqd0_eHF"
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
                    //return new X509Certificate2(certCollection[0], "idsrv3test");
                    return certCollection[0];
                }

                return null;
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        X509Certificate2 LoadCertificate2()
        {
            return new X509Certificate2(
                string.Format(@"{0}\Certificates\idsrv3test.pfx", AppDomain.CurrentDomain.BaseDirectory), "idsrv3test");
        }

    }
}