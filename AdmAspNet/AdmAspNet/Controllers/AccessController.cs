using AdmAspNet.Classes;
using AdmAspNet.Helpers;
using AdmAspNet.Models.DataContracts;
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
    /// <summary>
    /// A controller that handles CRUD functionality for accesspoints
    /// </summary>
    public class AccessController : Controller
    {
        private string tokenString = null;
        private Api api;
        private MapperConfiguration mapConfig;

        public AccessController()
        {
            //Set up all mapper configurations 
            mapConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Coordinate, CoordinateViewModel>();
                cfg.CreateMap<AccessPoint, AccessPointViewModel>();
                cfg.CreateMap<AccessPointViewModel, AccessPoint>();
                cfg.CreateMap<CoordinateViewModel, Coordinate>(); 
            });
        }
        
        /// <summary>
        /// Gives a list of all the accesspoints
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            List<AccessPoint> accessPointList = api.GetAllAccessPoints();
            var mapper = mapConfig.CreateMapper(); 
            List<AccessPointViewModel> viewModel = mapper.Map<List<AccessPointViewModel>>(accessPointList); 
            return View(viewModel);
        }

        /// <summary>
        /// Create a new accesspoint 
        /// </summary>
        /// <returns>A view that allows the user to create an accesspoint</returns>
        public ActionResult Create()
        {
            return View(); 
        }

        /// <summary>
        /// Create a new accesspoint 
        /// </summary>
        /// <param name="input">The data for the object that should be created</param>
        /// <returns>A view that allows the user to create an accesspoint</returns>
        [HttpPost] 
        public ActionResult Create([ModelBinder(typeof(AccessPointBinder))] AccessPointViewModel input)
        {
            if (!ModelState.IsValid)
            {
                return View(input); 
            }
            var mapper = mapConfig.CreateMapper();
            AccessPoint data = mapper.Map<AccessPoint>(input); 
            if (api.PostAccessPoint(data))
            {
                ViewBag.SuccessMessage = "Aksesspunktet ble opprettet"; 
            }
            else
            {
                ViewBag.ErrorMessage = "En feil oppstod, kontakt en systemadministrator"; 
            }
            return View(); 
        }

        /// <summary>
        /// Returns the details about one spesific accesspoint
        /// </summary>
        /// <param name="id">The id of the access point</param>
        /// <returns></returns>
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                ViewBag.ErrorMessage = "Du må angi en ID";
                return View("ErrorView");
            }
            AccessPoint accessPoint;
            if ((accessPoint = api.GetAccessPointById(id.Value)) == null)
            {
                ViewBag.ErrorMessage = "Kan ikke finne aksesspunktet du forespurte";
                return View("ErrorView"); 
            }
            var mapper = mapConfig.CreateMapper();
            AccessPointViewModel viewModel = mapper.Map<AccessPointViewModel>(accessPoint);
            return View(viewModel); 
        }

        /// <summary>
        /// Allows the user to edit accesspoint (GET)
        /// </summary>
        /// <param name="id">The id of the access point</param>
        /// <returns>A view that allows the user to edit the accesspoint</returns>
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                ViewBag.ErrorMessage = "Du må angi en ID";
                return View("ErrorView");
            }
            AccessPoint accessPoint;
            if ((accessPoint = api.GetAccessPointById(id.Value)) == null)
            {
                ViewBag.ErrorMessage = "Kan ikke finne aksesspunktet du forespurte";
                return View("ErrorView");
            }
            var mapper = mapConfig.CreateMapper();
            AccessPointViewModel viewModel = mapper.Map<AccessPointViewModel>(accessPoint);
            return View(viewModel); 
        }

        /// <summary>
        /// Allows the user to edit accesspoint (POST)
        /// </summary>
        /// <param name="id">The id of the accesspoint</param>
        /// <param name="input">Object with updated fields</param>
        /// <returns>A view that allows the user to edit the accesspoint</returns>
        [HttpPost]
        public ActionResult Edit(int id, [ModelBinder(typeof(AccessPointBinder))] AccessPointViewModel input)
        {
            AccessPoint accessPoint; 
            if ((accessPoint = api.GetAccessPointById(id)) == null)
            {
                ViewBag.ErrorMessage = "Kan ikke finne aksesspunktet du prøver å redigere";
                return View("ErrorView"); 
            }
            if (!ModelState.IsValid)
            {
                return View(input); 
            }
            bool latEquals = input.Coordinate.Lat.Equals(accessPoint.Coordinate.Lat);
            bool lngEquals = input.Coordinate.Lng.Equals(accessPoint.Coordinate.Lng);
            bool altEquals = input.Coordinate.Alt.Equals(accessPoint.Coordinate.Alt);
            if (latEquals && lngEquals && altEquals)
            {
                input.Coordinate.Id = accessPoint.Coordinate.Id; 
            }
            var mapper = mapConfig.CreateMapper();
            AccessPoint data = mapper.Map<AccessPoint>(input); 
            if (api.UpdateAccessPoint(id,data))
            {
                ViewBag.SuccessMessage = "Aksesspunktet ble endret"; 
            }
            else
            {
                ViewBag.ErrorMessage = "En feil oppstod, kontakt en systemadministrator"; 
            }
            return View(input); 
        }

        /// <summary>
        /// Allows the user to delete accesspoint (GET)
        /// </summary>
        /// <param name="id">The id of the accesspoint</param>
        /// <returns>A view that allows the user to delete an accesspoint</returns>
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                ViewBag.ErrorMessage = "Du må spesifisere en ID";
                return View("ErrorView"); 
            }
            AccessPoint accessPoint; 
            if ((accessPoint = api.GetAccessPointById(id.Value)) == null)
            {
                ViewBag.ErrorMessage = "Kan ikke finne aksesspunktet du spurte etter";
                return View("ErrorView"); 
            }
            var mapper = mapConfig.CreateMapper();
            AccessPointViewModel viewModel = mapper.Map<AccessPointViewModel>(accessPoint);
            return View(viewModel); 
        }

        /// <summary>
        /// Allows the user to delete accesspoint (POST)
        /// </summary>
        /// <param name="id">The id of the accesspoint</param>
        /// <returns>A view that allows the user to delete an accesspoint</returns>
        [HttpPost]
        public ActionResult Delete(int id)
        {
            AccessPoint accessPoint;
            if ((accessPoint = api.GetAccessPointById(id)) == null)
            {
                ViewBag.ErrorMessage = "Kan ikke finne aksesspunktet du spurte etter";
                return View("ErrorView");
            }

            if (api.DeleteAccessPoint(id))
            {
                ViewBag.SuccessMessage = "Aksesspunktet ble slettet";
            }
            else
            {
                ViewBag.ErrorMessage = "En feil oppstod, kontakt en systemadministrator"; 
            }
            var mapper = mapConfig.CreateMapper();
            AccessPointViewModel viewModel = mapper.Map<AccessPointViewModel>(accessPoint);
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