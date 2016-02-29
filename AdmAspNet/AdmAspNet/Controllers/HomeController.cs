using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.IdentityModel.Protocols;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace AdmAspNet.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private static string aadInstance = ConfigurationManager.AppSettings["AADInstance"];
        private static string tenant = ConfigurationManager.AppSettings["Tenant"];
        private static string clientId = ConfigurationManager.AppSettings["ClientId"];
        private static string clientSecret = ConfigurationManager.AppSettings["ClientSecret"];

        private static string authority = String.Format(aadInstance+"{0}", tenant);

        private static string apiResourceId = ConfigurationManager.AppSettings["ApiResourceId"];
        private static string apiBaseAddress = ConfigurationManager.AppSettings["ApiBaseAddress"];

        private AuthenticationContext authContext = null;
        private static AuthenticationResult authResult = null;

        public async Task<ActionResult> Index()
        {
            string debug = "";
            try
            {
                debug += "start - ";
                authContext = new AuthenticationContext(authority);
                debug += "authContext - ";
                authResult = await authContext.AcquireTokenAsync(apiResourceId, new ClientCredential(clientId, clientSecret));
                debug += "authResult - ";
                var test = Get();
                debug += "Get - ";
                ViewBag.response = debug + apiBaseAddress + test;
                debug += "ferdig";
            }
            catch(Exception e)
            {
                ViewBag.response = debug + e.Message + e.InnerException + e.StackTrace;
            }


            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        string Get()
        {



            using (var client = new HttpClient())
            {
                
                client.BaseAddress = new Uri(apiBaseAddress);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);

                HttpResponseMessage response = client.GetAsync("/values").Result;
                if (response.IsSuccessStatusCode)
                {
                    return response.Content.ReadAsStringAsync().Result;
                }
                var test = response.Content.ReadAsStringAsync().Result;
                return test;
             
            }
        }
    }
}