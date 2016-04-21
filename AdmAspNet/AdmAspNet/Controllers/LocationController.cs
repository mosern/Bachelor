using AdmAspNet.Classes;
using AdmAspNet.Models.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using AdmAspNet.Models.ViewModels;
using System.Web.Routing;
using AdmAspNet.Helpers;

namespace AdmAspNet.Controllers
{
    public class LocationController : Controller
    {
        private string tokenString = null;
        private Api api;
        private MapperConfiguration mapConfig;
        public LocationController()
        {
            mapConfig = new MapperConfiguration(cfg => {
                cfg.CreateMap<Location, LocationViewModel>();
                cfg.CreateMap<Coordinate, CoordinateViewModel>();
                cfg.CreateMap<LocationViewModel, Location>();
                cfg.CreateMap<CoordinateViewModel, Coordinate>();
                cfg.CreateMap<Models.DataContracts.Type, TypeViewModel>();
                cfg.CreateMap<TypeViewModel, Models.DataContracts.Type>();  
            });
        }

        /// <summary>
        /// Viser en liste over alle de forskjellige lokasjonene 
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            List<Location> locationList = api.GetAllLocations();
            var mapper = mapConfig.CreateMapper();
            List<LocationViewModel> listViewModdel = mapper.Map<List<LocationViewModel>>(locationList);
            return View(listViewModdel);
        }

        /// <summary>
        /// Lar deg opprette en lokasjon
        /// </summary>
        /// <returns></returns>
        public ActionResult Create()
        {
            List<Models.DataContracts.Type> list = api.GetAllTypes();
            var mapper = mapConfig.CreateMapper();
            List<TypeViewModel> listViewModel = mapper.Map<List<TypeViewModel>>(list);
            LocationViewModel locationViewModel = new LocationViewModel();
            locationViewModel.Types = listViewModel;
            return View(locationViewModel);
        }

        /// <summary>
        /// Lar deg opprette en lokasjon
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Create([ModelBinder(typeof(LocationBinder))]LocationViewModel input)
        {
            List<Models.DataContracts.Type> list = api.GetAllTypes();
            var mapper = mapConfig.CreateMapper();
            List<TypeViewModel> listViewModel = mapper.Map<List<TypeViewModel>>(list);
            Location locationObject = mapper.Map<Location>(input);
            if (!ModelState.IsValid)
            {
                input.Types = listViewModel;
                return View(input); 
            }
            LocationViewModel baseViewModel = new LocationViewModel();
            baseViewModel.Types = listViewModel; 
            if (api.PostLocation(locationObject))
            {
                ViewBag.SuccessMessage = "Lokasjonen ble opprettet"; 
            } 
            else
            {
                ViewBag.ErrorMessage = "En feil oppstod, kontakt en systemadministrator"; 
            }
            return View(baseViewModel); 
        }


        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                ViewBag.ErrorMessage = "Du må spesifisere en ID";
                return View("ErrorView"); 
            }
            Location locationObject; 
            if ((locationObject = api.GetLocationById(id.Value)) == null) {
                ViewBag.ErrorMessage = "Kan ikke finne lokasjonen du forespurte";
                return View("ErrorView"); 
            }
            var mapper = mapConfig.CreateMapper();
            LocationViewModel locationViewModel = mapper.Map<LocationViewModel>(locationObject); 
            return View(locationViewModel); 
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                ViewBag.ErrorMessage = "Du må spesifisere en ID";
                return View("ErrorView");
            }
            Location locationObject; 
            if ((locationObject = api.GetLocationById(id.Value)) == null)
            {
                ViewBag.ErrorMessage = "Kan ikke finne lokasjonen du forespurte";
                return View("ErrorView"); 
            }
            var mapper = mapConfig.CreateMapper();
            LocationViewModel locationViewModel = mapper.Map<LocationViewModel>(locationObject);
            return View(locationViewModel); 
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                ViewBag.ErrorMessage = "Du må spesifisere en ID";
                return View("ErrorView"); 
            }
            Location locationObject;
            if ((locationObject = api.GetLocationById(id.Value)) == null)
            {
                ViewBag.ErrorMessage = "Kan ikke finne lokasjonen du forespurte";
                return View("ErrorView"); 
            }
            var mapper = mapConfig.CreateMapper();
            LocationViewModel locationViewModel = mapper.Map<LocationViewModel>(locationObject);
            return View(locationViewModel); 
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