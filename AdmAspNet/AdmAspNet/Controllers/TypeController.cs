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
using Thinktecture.IdentityModel.Mvc;

namespace AdmAspNet.Controllers
{
    [ResourceAuthorize("Write", "Admin")]
    [HandleForbidden]
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

        [HttpPost]
        public ActionResult Create([Bind(Include ="Name")] TypeViewModel input)
        {
            if (!ModelState.IsValid)
            {
                return View(input); 
            }
            var mapper = mapConfig.CreateMapper();
            Models.DataContracts.Type data = mapper.Map<Models.DataContracts.Type>(input);
           if (api.PostType(data))
            {
                ViewBag.SuccessMessage = "Typen ble opprettet";
            }
           else
            {
                ViewBag.ErrorMessage = "En feil oppstod, kontakt en systemadministrator"; 
            }
            return View(); 
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                ViewBag.ErrorMessage = "Du må spesifisere en ID";
                return View("ErrorView"); 
            }
            Models.DataContracts.Type data; 
            if ((data = api.GetTypeById(id.Value)) == null)
            {
                ViewBag.ErrorMessage = "Kan ikke finne typen du forespurte";
                return View("ErrorView"); 
            }
            var mapper = mapConfig.CreateMapper();
            TypeViewModel viewModel = mapper.Map<TypeViewModel>(data); 
            return View(viewModel); 
        }

        [HttpPost]
        public ActionResult Edit(int id, [Bind(Include ="Name")] TypeViewModel input)
        {
            if (!ModelState.IsValid)
            {
                return View(input);
            }
            var mapper = mapConfig.CreateMapper();
            Models.DataContracts.Type type = mapper.Map<Models.DataContracts.Type>(input); 
            if (api.UpdateType(id,type))
            {
                ViewBag.SuccessMessage = "Typen ble oppdatert"; 
            }
            else
            {
                ViewBag.ErrorMessage = "En feil oppstod, kontakt en systemadministrator"; 
            }
            return View(input); 

        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                ViewBag.ErrorMessage = "Du må spesifisere en id";
                return View("ErrorView"); 
            }
            Models.DataContracts.Type data; 
            if ((data = api.GetTypeById(id.Value)) == null)
            {
                ViewBag.ErrorMessage = "Typen du forespurte finnes ikke";
                return View("ErrorView"); 
            }
            var mapper = mapConfig.CreateMapper();
            TypeViewModel viewModel = mapper.Map<TypeViewModel>(data);
            return View(viewModel); 
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            Models.DataContracts.Type data; 
            if ((data = api.GetTypeById(id)) == null)
            {
                ViewBag.ErrorMessage = "Typen du forespurte finnes ikke";
                return View("ErrorView"); 
            }
            
            if (api.DeleteType(id))
            {
                ViewBag.SuccessMessage = "Typen ble slettet"; 
            }
            else
            {
                ViewBag.ErrorMessage = "En feil oppstod, kontakt systemadministrator"; 
            }
            var mapper = mapConfig.CreateMapper();
            TypeViewModel viewModel = mapper.Map<TypeViewModel>(data);
            return View(viewModel); 
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