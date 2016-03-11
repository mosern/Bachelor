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
    public class UserService : UserServiceBase
    {
        public static Repository<User> Users = new Repository<User>();
        public static Repository<UserProvider> UserProviders = new Repository<UserProvider>();
        public static Repository<Claims> Claims = new Repository<Claims>();
        public static List<User> UsersL = Users.List();

        public override Task AuthenticateLocalAsync(LocalAuthenticationContext context)
        {
            var user = UsersL.SingleOrDefault(x => x.Username == context.UserName && x.Password == context.Password.Sha512());
            if (user != null)
            {
                context.AuthenticateResult = new AuthenticateResult(user.Id.ToString(), user.Username);
            }


            return Task.FromResult(0);
        }

        public override Task AuthenticateExternalAsync(ExternalAuthenticationContext context)
        {
            var userProvider = UserProviders.List().FirstOrDefault(u => u.Identifier == context.ExternalIdentity.ProviderId);
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
                newUser.Password = RandomString(10).Sha512();

                User createdUser = Users.Create(newUser);

                context.AuthenticateResult = new AuthenticateResult(createdUser.Id.ToString(), createdUser.Username);
            }
            return Task.FromResult(0);
        }

        public override Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            // issue the claims for the user
            var user = UsersL.SingleOrDefault(u => u.Username.Equals(context.Subject.Identity.Name));
            var claims = Claims.List().Where(c => c.UserId == user.Id).ToList();

            if (user != null)
            {
                context.IssuedClaims = MakeObject.IEClaimFromListClaims(claims);
            }

            return Task.FromResult(0);
        }

        //http://stackoverflow.com/questions/1344221/how-can-i-generate-random-alphanumeric-strings-in-c
        //
        //written by dtd
        public string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}