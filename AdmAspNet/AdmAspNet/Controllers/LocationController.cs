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
using Thinktecture.IdentityModel.Mvc;

namespace AdmAspNet.Controllers
{
    [ResourceAuthorize("Write","Admin")]
    [HandleForbidden]
    /*[ResourceAuthorize("Write","Admin")]
    [HandleForbidden]*/
    /// <summary>
    /// A controller that handles CRUD functionality for location
    /// </summary>
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
        /// Shows a list of all the locations
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            List<Location> locationList = api.GetAllLocations();
            //If there is no location, create an empty one
            if (locationList == null)
            {
                locationList = new List<Location>(); 
            }
            var mapper = mapConfig.CreateMapper();
            List<LocationViewModel> listViewModdel = mapper.Map<List<LocationViewModel>>(locationList);
            return View(listViewModdel);
        }

        /// <summary>
        /// Allow a user to create a location
        /// </summary>
        /// <returns>A view that allows the user to create a location</returns>
        public ActionResult Create()
        {
            //Get all types 
            List<Models.DataContracts.Type> list = api.GetAllTypes();
            var mapper = mapConfig.CreateMapper();
            //Map it to a viewmodel
            List<TypeViewModel> listViewModel = mapper.Map<List<TypeViewModel>>(list);
            //Create an empty location view model 
            LocationViewModel locationViewModel = new LocationViewModel();
            //Populate dropdown
            if (listViewModel.Count == 0)
            {
                locationViewModel.DropDown = new SelectList(new List<Models.DataContracts.Type>()); 
            }
            else
            {
                locationViewModel.DropDown = new SelectList(listViewModel, "Id", "Name", listViewModel.First().Id); 
            }
            return View(locationViewModel);
        }

        /// <summary>
        /// Allow the user to create a location (POST)
        /// </summary>
        /// <param name="input">The object to create</param>
        /// <returns>A view that allows the user to create a location</returns>
        [HttpPost]
        public ActionResult Create([ModelBinder(typeof(LocationBinder))]LocationViewModel input)
        {
            List<Models.DataContracts.Type> list = api.GetAllTypes();
            var mapper = mapConfig.CreateMapper();
            List<TypeViewModel> listViewModel = mapper.Map<List<TypeViewModel>>(list);
            Location locationObject = mapper.Map<Location>(input);
            if (!ModelState.IsValid)
            {
                if (listViewModel.Count == 0)
                {
                    input.DropDown = new SelectList(listViewModel); 
                }
                else
                {
                    input.DropDown = new SelectList(listViewModel, "Id", "Name", listViewModel.First().Id); 
                }
                return View(input); 
            }
            LocationViewModel baseViewModel = new LocationViewModel();
            if (listViewModel.Count == 0)
            {
                baseViewModel.DropDown = new SelectList(listViewModel); 
            }
            else
            {
                baseViewModel.DropDown = new SelectList(listViewModel, "Id", "Name", listViewModel.First().Id); 
            }
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

        /// <summary>
        /// Show the details of a given location
        /// </summary>
        /// <param name="id">The id of the location</param>
        /// <returns>A view that shows the details of the location</returns>
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

        /// <summary>
        /// Delete a given location (GET)
        /// </summary>
        /// <param name="id">The id of the location to delete</param>
        /// <returns>A view that allows the user to delete a location</returns>
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
                ViewBag.ErrorMessage = "Kan ikke finne lokasjonen du prøver å slette";
                return View("ErrorView"); 
            }
            var mapper = mapConfig.CreateMapper();
            LocationViewModel locationViewModel = mapper.Map<LocationViewModel>(locationObject);
            return View(locationViewModel); 
        }

        /// <summary>
        /// Delete a given location (POST)
        /// </summary>
        /// <param name="id">The id of the location to delete</param>
        /// <returns>A view that allows the user to delete a location</returns>
        [HttpPost]
        public ActionResult Delete(int id)
        {
            Location locationObject; 
            if ((locationObject = api.GetLocationById(id)) == null)
            {
                ViewBag.ErrorMessage = "Kan ikke finne lokasjonen du prøver å slette";
                return View("ErrorView"); 
            }
            if (api.DeleteLocation(id))
            {
                ViewBag.SuccessMessage = "Lokasjonen ble slettet";
            }
            else
            {
                ViewBag.ErrorMessage = "En feil oppstod, kontakt en systemadministrator"; 
            }
            var mapper = mapConfig.CreateMapper();
            LocationViewModel viewModel = mapper.Map<LocationViewModel>(locationObject); 
            return View(viewModel); 
        }

        /// <summary>
        /// Edit a location (GET) 
        /// </summary>
        /// <param name="id">The id of the location to edit</param>
        /// <returns>A view that allows the user to edit the location</returns>
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
            List<Models.DataContracts.Type> typeList = api.GetAllTypes();
            List<TypeViewModel> typeViewModel = mapper.Map<List<TypeViewModel>>(typeList); 
            LocationViewModel locationViewModel = mapper.Map<LocationViewModel>(locationObject);
            if (typeViewModel.Count == 0)
            {
                locationViewModel.DropDown = new SelectList(typeViewModel); 
            }
            else
            {
                locationViewModel.DropDown = new SelectList(typeViewModel, "Id", "Name", typeViewModel.First().Id); 
            }
            return View(locationViewModel); 
        }

        /// <summary>
        /// Edit a location (POST)
        /// </summary>
        /// <param name="id">The id of the location to edit</param>
        /// <param name="input">Object with updated fields</param>
        /// <returns>A view that allows the user to edit the location</returns>
        [HttpPost]
        public ActionResult Edit(int id,[ModelBinder(typeof(LocationBinder))] LocationViewModel input)
        {
            var mapper = mapConfig.CreateMapper();
            Location locationTmp; 
            List<Models.DataContracts.Type> typeList = api.GetAllTypes();
            List<TypeViewModel> typeViewModel = mapper.Map<List<TypeViewModel>>(typeList);
            if (typeViewModel.Count == 0)
            {
                input.DropDown = new SelectList(typeViewModel); 
            }
            else
            {
                input.DropDown = new SelectList(typeViewModel, "Id", "Name", typeViewModel.First().Id); 
            }
            if (!ModelState.IsValid)
            {
                return View(input); 
            }
            if ((locationTmp =api.GetLocationById(id)) == null)
            {
                ViewBag.ErrorMessage = "Kan ikke finne lokasjonen du forsøkte å redigere";
                return View("ErrorView");
            }
            Location locationObject = mapper.Map<Location>(input);
            bool latEquals = locationObject.Coordinate.Lat.Equals(locationTmp.Coordinate.Lat);
            bool lngEquals = locationObject.Coordinate.Lng.Equals(locationTmp.Coordinate.Lng);
            bool altEquals = locationObject.Coordinate.Alt.Equals(locationTmp.Coordinate.Alt); 
            if (latEquals && lngEquals && altEquals)
            {
                locationObject.Coordinate.Id = locationTmp.Coordinate.Id; 
            }
            if (api.UpdateLocation(id,locationObject))
            {
                ViewBag.SuccessMessage = "Lokasjonen ble oppdatert"; 
            }
            else
            {
                ViewBag.ErrorMessage = "En feil oppstod, kontakt en systemadministrator"; 
            }
            return View(input); 
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