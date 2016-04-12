using IdentityServer3.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IdSrv.Config
{
    /// <summary>
    /// Client config
    /// 
    /// Written by: Andreas Mosvoll
    /// </summary>
    public static class Clients
    {
        public static IEnumerable<Client> Get()
        {
            return new[]
            {
                new Client
                {
                    Enabled = true,
                    ClientName = "Adm",
                    ClientId = "adm",
                    Flow = Flows.Hybrid,
                    RequireConsent = true,

                    RedirectUris = new List<string>
                    {
                        "https://localhost:44300",
                        "https://bacheloradm2.azurewebsites.net/",
                    },

                    PostLogoutRedirectUris = new List<string>
                    {
                        "https://bacheloradm2.azurewebsites.net/",
                    },

                    AllowedScopes = new List<string>
                    {
                        StandardScopes.OpenId.Name,
                        StandardScopes.Profile.Name,
                        "api",
                        "roles",
                    },                   
                },

                new Client
                {
                    Enabled = true,
                    ClientName = "Android",
                    ClientId = "and",
                    Flow = Flows.Implicit,

                    RedirectUris = new List<string>
                    {
                        "http://localhost:123",
                    },              

                    ClientSecrets = new List<Secret>
                    {
                        new Secret("801Hd9ZEq0")
                    },

                    AllowedScopes = new List<string>
                    {
                        StandardScopes.OpenId.Name,
                        StandardScopes.Profile.Name,
                        "api",
                        "roles",
                    },
                }
            };
        }
    }
}