using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using IdentityServer3.Core.Services.Default;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using UserDB;

namespace IdSrv.Services
{
    public class LocalUserService : UserServiceBase
    {
        public static Repository<User> Users = new Repository<User>();
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

        /*public override Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            // issue the claims for the user
            var user = Users.SingleOrDefault(x => x.Subject == context.Subject.GetSubjectId());
            if (user != null)
            {
                context.IssuedClaims = user.Claims.Where(x => context.RequestedClaimTypes.Contains(x.Type));
            }

            return Task.FromResult(0);*/
        }
    }
}