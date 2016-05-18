using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Thinktecture.IdentityModel.Owin.ResourceAuthorization;

namespace Api.Classes
{


    /// <summary>
    /// Implementation of Identityservers resourceAutorization
    /// Inspired by https://identityserver.github.io/Documentation/docsv2/overview/mvcGettingStarted.html
    /// We have not utilized the potential here, due to limited time everything regarding user administration was a low priority.
    /// 
    /// Written by: Andreas Mosvoll
    /// </summary>
    public class AuthorizationManager : ResourceAuthorizationManager
    {
        public override Task<bool> CheckAccessAsync(ResourceAuthorizationContext context)
        {
            switch (context.Resource.First().Value)
            {
                case "location" : return AuthorizeLocation(context);
                case "accesspoint" : return AuthorizeAccesspoint(context);
                case "coordinate" : return AuthorizeCoordinate(context);
                case "neighbour" : return AuthorizeNeighbour(context);
                case "pathPoint" : return AuthorizePathPoint(context);
                case "people" : return AuthorizePeople(context);
                case "type" : return AuthorizeType(context);
                case "user" : return AuthorizeUser(context);
                default : return Nok();
            }

            //Uncomment this and comment out switch statement to disable autorization
            //return Eval(true);
        }

        private Task<bool> AuthorizeUser(ResourceAuthorizationContext context)
        {
            switch (context.Action.First().Value)
            {
                case "read": return Eval(context.Principal.HasClaim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Administrator"));
                case "write": return Eval(context.Principal.HasClaim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Administrator"));
                case "edit": return Eval(context.Principal.HasClaim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Administrator"));
                case "delete": return Eval(context.Principal.HasClaim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Administrator"));
                default: return Nok();
            }
        }

        private Task<bool> AuthorizeType(ResourceAuthorizationContext context)
        {
            switch (context.Action.First().Value)
            {
                case "read": return Eval(true);
                case "write": return Eval(context.Principal.HasClaim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Administrator"));
                case "edit": return Eval(context.Principal.HasClaim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Administrator"));
                case "delete": return Eval(context.Principal.HasClaim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Administrator"));
                default: return Nok();
            }
        }

        private Task<bool> AuthorizePeople(ResourceAuthorizationContext context)
        {
            switch (context.Action.First().Value)
            {
                case "read": return Eval(true);
                case "write": return Eval(context.Principal.HasClaim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Administrator"));
                case "edit": return Eval(context.Principal.HasClaim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Administrator"));
                case "delete": return Eval(context.Principal.HasClaim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Administrator"));
                default: return Nok();
            }
        }

        private Task<bool> AuthorizePathPoint(ResourceAuthorizationContext context)
        {
            switch (context.Action.First().Value)
            {
                case "read": return Eval(true);
                case "write": return Eval(context.Principal.HasClaim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Administrator"));
                case "edit": return Eval(context.Principal.HasClaim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Administrator"));
                case "delete": return Eval(context.Principal.HasClaim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Administrator"));
                default: return Nok();
            }
        }

        private Task<bool> AuthorizeNeighbour(ResourceAuthorizationContext context)
        {
            switch (context.Action.First().Value)
            {
                case "read": return Eval(true);
                case "write": return Eval(context.Principal.HasClaim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Administrator"));
                case "edit": return Eval(context.Principal.HasClaim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Administrator"));
                case "delete": return Eval(context.Principal.HasClaim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Administrator"));
                default: return Nok();
            }
        }

        private Task<bool> AuthorizeCoordinate(ResourceAuthorizationContext context)
        {
            switch (context.Action.First().Value)
            {
                case "read": return Eval(true);
                case "write": return Eval(context.Principal.HasClaim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Administrator"));
                case "edit": return Eval(context.Principal.HasClaim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Administrator"));
                case "delete": return Eval(context.Principal.HasClaim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Administrator"));
                default: return Nok();
            }
        }

        private Task<bool> AuthorizeAccesspoint(ResourceAuthorizationContext context)
        {
            switch (context.Action.First().Value)
            {
                case "read": return Eval(true);
                case "write": return Eval(context.Principal.HasClaim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Administrator"));
                case "edit": return Eval(context.Principal.HasClaim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Administrator"));
                case "delete": return Eval(context.Principal.HasClaim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Administrator"));
                default: return Nok();
            }
        }

        private Task<bool> AuthorizeLocation(ResourceAuthorizationContext context)
        {
            switch (context.Action.First().Value)
            {
                case "read" : return Eval(true);
                case "write" : return Eval(context.Principal.HasClaim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Administrator"));
                case "edit" : return Eval(context.Principal.HasClaim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Administrator"));
                case "delete" : return Eval(context.Principal.HasClaim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Administrator"));
                default : return Nok();
            }
        }

    }
}