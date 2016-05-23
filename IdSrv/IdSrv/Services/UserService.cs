using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using IdentityServer3.Core.Services.Default;
using IdSrv.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using UserDB;

namespace IdSrv.Services
{
    /// <summary>
    /// User authentication logic.
    /// UserService used by IdentityServer3
    /// 
    /// Written by: Andreas Mosvoll
    /// </summary>
    public class UserService : UserServiceBase
    {
        public static Repository<User> Users = new Repository<User>();
        public static Repository<UserProvider> UserProviders = new Repository<UserProvider>();
        public static Repository<Claims> Claims = new Repository<Claims>();
        public static List<User> UsersL = Users.List().ToList();

        /// <summary>
        /// Autentication local user
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task AuthenticateLocalAsync(LocalAuthenticationContext context)
        {
            //TODO Password salt
            var user = UsersL.SingleOrDefault(x => x.Username == context.UserName && x.Password == (context.Password + x.Salt).Sha512());
            if (user != null)
            {
                context.AuthenticateResult = new AuthenticateResult(user.Id.ToString(), user.Username);
            }


            return Task.FromResult(0);
        }

        /// <summary>
        /// Autenticates local user connected to the verrified external identity. If no user is connectet to the external identity, a new user i created and autenticated.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task AuthenticateExternalAsync(ExternalAuthenticationContext context)
        {
            //TODO Make user registration (and hash Providerid and local userid?)
            var userProvider = UserProviders.List().FirstOrDefault(u => u.Identifier == context.ExternalIdentity.ProviderId.Sha512());
            User user = null;

            if (userProvider != null)
            user = UsersL.SingleOrDefault(x => x.Id == userProvider.UserId);

            if (user != null)
            {
                context.AuthenticateResult = new AuthenticateResult(user.Id.ToString(), user.Username);
            }
            else
            {
                User newUser = new User();
                newUser.Username = RandomString(10);
                newUser.Salt = RandomString(10);
                newUser.Password = (RandomString(10) + newUser.Salt).Sha512();
                

                User createdUser = Users.Create(newUser);

                Provider provider = new Repository<Provider>().List().SingleOrDefault(p => p.Name == context.ExternalIdentity.Provider);

                UserProvider newUserProvider = new UserProvider();
                newUserProvider.UserId = createdUser.Id;
                newUserProvider.ProviderId = provider.Id;
                newUserProvider.Identifier = context.ExternalIdentity.ProviderId.Sha512();

                UserProviders.Create(newUserProvider);

                Claims.Create(new Claims() { Type = "username", Value = createdUser.Username, UserId = createdUser.Id });

                UsersL = Users.List().ToList();
                context.AuthenticateResult = new AuthenticateResult(createdUser.Id.ToString(), createdUser.Username);
            }
            return Task.FromResult(0);
        }

        /// <summary>
        /// Gets users IssuedClaims and assigns it to context
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task GetProfileDataAsync(ProfileDataRequestContext context)
        {

            var user = UsersL.SingleOrDefault(u => u.Username.Equals(context.Subject.Identity.Name));

            if (user != null)
            {
                var claims = Claims.List().Where(c => c.UserId == user.Id).ToList();
                context.IssuedClaims = MakeObject.IEClaimFromListClaims(claims);
            }

            return Task.FromResult(0);
        }

        /// <summary>
        /// Copied from stackowerflow
        /// http://stackoverflow.com/questions/1344221/how-can-i-generate-random-alphanumeric-strings-in-c
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}