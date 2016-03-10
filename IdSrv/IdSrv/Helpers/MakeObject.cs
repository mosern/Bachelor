using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using UserDB;

namespace IdSrv.Helpers
{
    public class MakeObject
    {
        public static IEnumerable<Claim> IEClaimFromListClaims(List<Claims> claims)
        {
            List<Claim> enClaims = new List<Claim>();

            foreach (Claims clame in claims)
            {
                enClaims.Add(new Claim(clame.Type, clame.Value));
            }

            return enClaims as IEnumerable<Claim>;
        }
    }
}