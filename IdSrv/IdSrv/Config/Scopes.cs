using IdentityServer3.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IdSrv.Config
{
    public static class Scopes
    {
        public static IEnumerable<Scope> Get()
        {
            var scopes = new List<Scope>
                {
 
                    // identity scopes

                    StandardScopes.OpenId,
                    StandardScopes.Profile,

                    new Scope
                    {
                        Name = "api",
                        DisplayName = "API",
                        Description = "Access to the api",
                        Type = ScopeType.Resource,
                        Emphasize = false,
                        Enabled = true
                    },

                    new Scope
                    {
                        Enabled = true,
                        Name = "roles",
                        Type = ScopeType.Identity,
                        Claims = new List<ScopeClaim>
                        {
                            new ScopeClaim("role")
                        }
                    }

                 };

            return scopes;
        }

    }
}