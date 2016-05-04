using AdmAspNet.Classes;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AdmAspNet.Controllers
{
    public class PersonController : Controller
    {
        private string tokenString = null;
        private Api api;
        private MapperConfiguration mapConfig;
        // GET: Person
        public ActionResult Index()
        {
            return View();
        }


    }
}