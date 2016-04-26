using AdmAspNet.Classes;
using AdmAspNet.Models.ViewModels;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace AdmAspNet.Controllers
{
    public class TypeController : Controller
    {
        private string tokenString = null;
        private Api api;
        private MapperConfiguration mapConfig;

        public TypeController()
        {
            mapConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<TypeViewModel, Models.DataContracts.Type>();
                cfg.CreateMap<Models.DataContracts.Type, TypeViewModel>(); 
            });
        }

        // GET: Type
        public ActionResult Index()
        {
            List<Models.DataContracts.Type> typeList = api.GetAllTypes();
            var mapper = mapConfig.CreateMapper();
            List<TypeViewModel> viewModel = mapper.Map<List<TypeViewModel>>(typeList); 
            return View(viewModel);
        }

        public ActionResult Create()
        {
            return View(); 
        }

        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);
            var token = (User.Identity as ClaimsIdentity).FindFirst("access_token");
            if (token != null)
            {
                tokenString = token.Value;
            }
            api = new Api(tokenString);
        }
    }
}