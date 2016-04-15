using Api.Models.Api;
using Api.Models.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Api.Helpers
{
    public class CompareEF<X> : IEqualityComparer<X> where X : BaseViewModel
    {
        public bool Equals(X x, X y)
        {
            if(x.Id == y.Id)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public int GetHashCode(X obj)
        {
            return obj.GetHashCode();
        }
    }
}