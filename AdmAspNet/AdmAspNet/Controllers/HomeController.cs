using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.IdentityModel.Protocols;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace AdmAspNet.Controllers
{
    
    public class HomeController : Controller
    {
        string apiBaseAddress = ConfigurationManager.AppSettings["apiBaseAddress"];

        public ActionResult Index()
        {
            return View();
        }

        [Authorize]
        public ActionResult About()
        {
            string test = Get();
            ViewBag.Message = "Your application description page." + test;

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
                var token = (User.Identity as ClaimsIdentity).FindFirst("access_token");
                if(token != null)
                {
                    client.SetBearerToken(token.Value);
                }


                client.BaseAddress = new Uri(apiBaseAddress);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = client.GetAsync("api/values").Result;
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