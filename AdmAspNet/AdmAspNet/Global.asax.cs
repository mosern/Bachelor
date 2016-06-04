using AdmAspNet.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace AdmAspNet
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            ServicePointManager.ServerCertificateValidationCallback += (sender,certificate,chain,sslPolicyErrors) =>
            {
                return true;
            };
            ModelBinders.Binders.Add(typeof(LocationBinder), new LocationBinder()); 
        }

        protected void Application_AcquireRequestState(object sender, EventArgs e)
        {
            var culture = new CultureInfo("nb-NO");

            // Modify current thread's cultures            
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(culture.Name);
        }

        protected void Session_Start(object sender, EventArgs e)
        {
            /// When using cookie-based session state, ASP.NET does not allocate storage for session data until the Session object is used. 
            /// As a result, a new session ID is generated for each page request until the session object is accessed. 
            /// If your application requires a static session ID for the entire session, 
            /// you can either implement the Session_Start method in the application's Global.asax file and store data in the Session object to fix the session ID, 
            /// or you can use code in another part of your application to explicitly store data in the Session object.
            base.Session["init"] = 0;
        }
    }
}
