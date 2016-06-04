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
using Thinktecture.IdentityModel.Mvc;

namespace AdmAspNet.Controllers
{
    /*[ResourceAuthorize("Write", "Admin")]
    [HandleForbidden]*/
    public class AdminController : Controller
    {
        /// <summary>
        /// Shows the frontpage of the admin interface
        /// </summary>
        /// <returns>A view with information to the user</returns>
        public ActionResult Index()
        {
            return View();
        }
    }
}