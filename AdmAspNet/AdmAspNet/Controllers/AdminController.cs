using AdmAspNet.Classes;
using AdmAspNet.Models.DataContracts;
using AdmAspNet.Models.ViewModels;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace AdmAspNet.Controllers
{
    public class AdminController : Controller
    {
        private static string apiBaseAddress = ConfigurationManager.AppSettings["apiBaseAddress"]; 
        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Location()
        {
            string tokenString = null;
            var token = (User.Identity as ClaimsIdentity).FindFirst("access_token");
            if (token != null)
            {
                tokenString = token.Value;
            }
            Api api = new Api(tokenString);
            List<Location> listLocation = api.GetAllLocations();

            var config = new MapperConfiguration(cfg => cfg.CreateMap<Location, LocationViewModel>());

            var mapper = config.CreateMapper();

            List<LocationViewModel> listViewModel = mapper.Map<List<LocationViewModel>>(listLocation);
            return View(listViewModel); 
        }
    }
}