using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens;
using System.Linq;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.ActiveDirectory;
using Owin;
using System.Web.Http;
using IdentityServer3.AccessTokenValidation;

namespace Api
{
    public partial class Startup
    {
        
        public void ConfigureAuth(IAppBuilder app)
        {
            app.UseIdentityServerBearerTokenAuthentication(new IdentityServerBearerTokenAuthenticationOptions
            {
                Authority = "https://bacheloridsrv3.azurewebsites.net/identity/",
                RequiredScopes = new[] { "api" }
            });

        }
    }
}
