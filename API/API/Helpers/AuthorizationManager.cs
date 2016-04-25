using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Thinktecture.IdentityModel.Owin.ResourceAuthorization;

namespace Api.Classes
{
    //Implementation of Identityservers resourceAutorization
    //https://identityserver.github.io/Documentation/docsv2/overview/mvcGettingStarted.html
    public class AuthorizationManager : ResourceAuthorizationManager
    {
        public override Task<bool> CheckAccessAsync(ResourceAuthorizationContext context)
        {
            switch (context.Resource.First().Value)
            {
                case "location": return AuthorizeLocation(context);
                default: return Nok();
            }
        }

        private Task<bool> AuthorizeLocation(ResourceAuthorizationContext context)
        {
            switch (context.Action.First().Value)
            {
                case "Read":
                    return Eval(context.Principal.Identity.IsAuthenticated);
                case "Write":
                    return Eval(context.Principal.HasClaim("roles", "Administrator"));
                default:
                    return Nok();
            }
        }

    }
}