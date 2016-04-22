using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Thinktecture.IdentityModel.Owin.ResourceAuthorization;

namespace AdmAspNet.Helpers
{
    //https://identityserver.github.io/Documentation/docsv2/overview/mvcGettingStarted.html
    public class AuthorizationManager : ResourceAuthorizationManager
    {
        public override Task<bool> CheckAccessAsync(ResourceAuthorizationContext context)
        {
            switch (context.Resource.First().Value)
            {
                case "ContactDetails" : return AuthorizeContactDetails(context);
                case "About" : return AuthorizeAbout(context);
                case "Admin": return AuthorizeAdmin(context);
                default : return Nok();
            }
        }

        private Task<bool> AuthorizeContactDetails(ResourceAuthorizationContext context)
        {
            switch (context.Action.First().Value)
            {
                case "Read":
                    return Eval(context.Principal.HasClaim("roles", "Administrator"));
                case "Write":
                    return Eval(context.Principal.HasClaim("roles", "Administrator"));
                default:
                    return Nok();
            }
        }

        private Task<bool> AuthorizeAbout(ResourceAuthorizationContext context)
        {
            switch (context.Action.First().Value)
            {
                case "Read":
                    return Eval(context.Principal.Identity.IsAuthenticated);
                case "Write":
                    return Eval(false);
                default:
                    return Nok();
            }
        }

        private Task<bool> AuthorizeAdmin(ResourceAuthorizationContext context)
        {
            switch (context.Action.First().Value)
            {
                case "Read":
                    return Eval(context.Principal.HasClaim("roles", "Administrator"));
                case "Write":
                    return Eval(context.Principal.HasClaim("roles","Administrator"));
                default:
                    return Nok(); 

            }
        }
    }
}