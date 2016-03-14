using IdentityServer3.Core;
using IdentityServer3.Core.Services.InMemory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;

namespace IdSrv.Config
{
    /// <summary>
    /// InMemmoryUsers, to be removed!
    /// 
    /// written by: Amdreas Mosvoll
    /// </summary>
    public static class Users
    {
        public static List<InMemoryUser> Get()
        {
            return new List<InMemoryUser>() {

               new InMemoryUser
            {
                Username = "Andreas",
                Password = "secret",
                Subject = "1",

                Claims = new[]
                {
                    new Claim(Constants.ClaimTypes.GivenName, "Andreas"),
                    new Claim(Constants.ClaimTypes.FamilyName, "Mosvoll"),
               }
            }
            ,
            new InMemoryUser
            {
                Username = "Henning",
                Password = "secret",
                Subject = "2",

                Claims = new[]
                {
                    new Claim(Constants.ClaimTypes.GivenName, "Henning"),
                    new Claim(Constants.ClaimTypes.FamilyName, "Fredriksen"),
               }
            },

            new InMemoryUser
            {
                Username = "Evgeniia",
                Password = "secret",
                Subject = "3",

                Claims = new[]
                {
                    new Claim(Constants.ClaimTypes.GivenName, "Evgeniia"),
                    new Claim(Constants.ClaimTypes.FamilyName, "Gladkova"),
               }
            },

            new InMemoryUser
            {
                Username = "Simon",
                Password = "secret",
                Subject = "4",

                Claims = new[]
                {
                    new Claim(Constants.ClaimTypes.GivenName, "Simon"),
                    new Claim(Constants.ClaimTypes.FamilyName, "Ingebrigtsen"),
               }
            }

           };
        }
    }
}