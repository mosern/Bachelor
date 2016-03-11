using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdmAspNet.Helpers
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class ResourceAuthorizeAttribute : Thinktecture.IdentityModel.Mvc.ResourceAuthorizeAttribute
    {

        public ResourceAuthorizeAttribute() : base()
        {

        }
        public ResourceAuthorizeAttribute(string action, params string[] resources)
            : base(action, resources)
        {

        }

        protected override void HandleUnauthorizedRequest(System.Web.Mvc.AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.Request.IsAuthenticated)
            {
                filterContext.Result = new System.Web.Mvc.HttpStatusCodeResult((int)System.Net.HttpStatusCode.Forbidden);
            }
            else
            {
                base.HandleUnauthorizedRequest(filterContext);
            }
        }
    }
}