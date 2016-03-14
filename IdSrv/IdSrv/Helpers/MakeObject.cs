using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using UserDB;

namespace IdSrv.Helpers
{
    /// <summary>
    /// Helper metodes to convert my db models to system classes
    /// </summary>
    public class MakeObject
    {
        /// <summary>
        /// Converts a list of my db model claims to IEnumerable of System.Security.Claims.Claim
        /// </summary>
        /// <param name="claims"></param>
        /// <returns> IEnumerable<Claim> </returns>
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