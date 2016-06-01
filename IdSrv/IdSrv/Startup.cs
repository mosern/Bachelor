﻿using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Services;
using IdSrv.Config;
using IdSrv.Services;
using Microsoft.Owin;
using Microsoft.Owin.Security.Facebook;
using Microsoft.Owin.Security.Google;
using Microsoft.Owin.Security.MicrosoftAccount;
using Owin;
using Serilog;
using System;
using System.Security.Cryptography.X509Certificates;

[assembly: OwinStartupAttribute(typeof(IdSrv.Startup))]

namespace IdSrv
{
    /// <summary>
    /// Written by: Andreas Mosvoll
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Startup config, configures identity server.
        /// 
        /// </summary>
        /// <param name="app"></param>
        public void Configuration(IAppBuilder app)
        {
            app.Map("/identity", idsrvApp =>
            {
                var factory = Database.Configure("Bachelor");

                var userService = new UserService();

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

                });
            });

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Trace()
                .CreateLogger();

        }

        /// <summary>
        /// Configures additional identity providers
        /// </summary>
        /// <param name="app"></param>
        /// <param name="signInAsType"></param>
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

            app.UseFacebookAuthentication(new FacebookAuthenticationOptions
            {
                AuthenticationType = "Facebook",
                Caption = "Sign-in with Facebook",
                SignInAsAuthenticationType = signInAsType,
                
                AppId = "1776415305922386",
                AppSecret = "3853d13cd3cb1a946eafe8773812a5db"
            });

            app.UseMicrosoftAccountAuthentication(new MicrosoftAccountAuthenticationOptions
            {
                AuthenticationType = "Microsoft",
                Caption = "Sign-in with Microsoft",
                SignInAsAuthenticationType = signInAsType,
                
                ClientId = "000000004818CC2B",
                ClientSecret = "piRovC-cZ4mPkdBLcXqJBC7QrTjMZFV8"
            });
        }


        /// <summary>
        /// Get certificate from azure to sign tokens
        /// </summary>
        /// <returns>X509Certificate2</returns>
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

        /// <summary>
        /// Get certificate from local folder to sign tokens, used for debuging 
        /// </summary>
        /// <returns>X509Certificate2</returns>
        X509Certificate2 LoadCertificate2()
        {
            return new X509Certificate2(
                string.Format(@"{0}Certificates\idsrv3test.pfx", AppDomain.CurrentDomain.BaseDirectory), "idsrv3test");
        }

    }
}