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
        private static string aadInstance = ConfigurationManager.AppSettings["ida:AADInstance"];
        private static string tenant = ConfigurationManager.AppSettings["ida:Tenant"];
        private static string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        Uri redirectUri = new Uri(ConfigurationManager.AppSettings["ida:RedirectUri"]);

        private static string authority = String.Format(aadInstance+"{0}", tenant);

        private static string apiResourceId = ConfigurationManager.AppSettings["ApiResourceId"];
        private static string apiBaseAddress = ConfigurationManager.AppSettings["ApiBaseAddress"];

        private AuthenticationContext authContext = null;
        private static AuthenticationResult authResult = null;

        public ActionResult Index()
        {
            authContext = new AuthenticationContext(authority);
            authResult = authContext.AcquireToken(apiResourceId, clientId, redirectUri);
            var test = GetAsync().Result;
            ViewBag.response = test;

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

        static async Task<HttpContent> GetAsync()
        {



            using (var client = new HttpClient())
            {

                client.BaseAddress = new Uri("https://bachelorapi.azurewebsites.net/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);


                HttpResponseMessage response = await client.GetAsync("api/values");
                if (response.IsSuccessStatusCode)
                {
                    return response.Content;
                }

                return null;
             
            }
        }
    }
}