using IdentityServer3.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IdSrv.Config
{
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
                        "https://bacheloradm.azurewebsites.net/"
                    },

                    AllowAccessToAllScopes = true,                    

                }
            };
        }
    }
}