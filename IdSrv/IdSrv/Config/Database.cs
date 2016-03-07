using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Models;
using IdentityServer3.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IdSrv.Config
{

    public class Database
    {
        /// <summary>
        /// Inspired by sample from IdentityServer3 developers
        /// https://github.com/IdentityServer/IdentityServer3.Samples/blob/master/source/EntityFramework/SelfHost/Config/Factory.cs
        /// </summary>
        /// <param name="connectStr"></param>
        /// <returns></returns>
        public static IdentityServerServiceFactory Configure(string connectStr)
        {
            var efConfig = new EntityFrameworkServiceOptions
            {
                ConnectionString = connectStr,
            };

            ConfigureClients( Clients.Get(), efConfig);
            ConfigureScopes(Scopes.Get(), efConfig);

            var factory = new IdentityServerServiceFactory();

            factory.RegisterConfigurationServices(efConfig);
            factory.RegisterOperationalServices(efConfig);

            factory.UseInMemoryUsers(Users.Get());

            return factory;


        }

        /// <summary>
        /// Copied from IdentityServer3 sample made by developers behind IdentityServer3
        /// https://github.com/IdentityServer/IdentityServer3.Samples/blob/master/source/EntityFramework/SelfHost/Config/Factory.cs
        /// </summary>
        /// <param name="clients"></param>
        /// <param name="options"></param>
        public static void ConfigureClients(IEnumerable<Client> clients, EntityFrameworkServiceOptions options)
        {
            using (var db = new ClientConfigurationDbContext(options.ConnectionString, options.Schema))
            {
                if (!db.Clients.Any())
                {
                    foreach (var c in clients)
                    {
                        var e = c.ToEntity();
                        db.Clients.Add(e);
                    }
                    db.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Copied from IdentityServer3 sample made by developers behind IdentityServer3
        /// https://github.com/IdentityServer/IdentityServer3.Samples/blob/master/source/EntityFramework/SelfHost/Config/Factory.cs
        /// </summary>
        /// <param name="scopes"></param>
        /// <param name="options"></param>
        public static void ConfigureScopes(IEnumerable<Scope> scopes, EntityFrameworkServiceOptions options)
        {
            using (var db = new ScopeConfigurationDbContext(options.ConnectionString, options.Schema))
            {
                if (!db.Scopes.Any())
                {
                    foreach (var s in scopes)
                    {
                        var e = s.ToEntity();
                        db.Scopes.Add(e);
                    }
                    db.SaveChanges();
                }
            }
        }
    }
}