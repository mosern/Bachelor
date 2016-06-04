using AdmAspNet.Classes;
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
    [Helpers.ResourceAuthorize("Write", "Admin")]
    [HandleForbidden]
    /// <summary>
    /// A controller that handles CRUD functionality for neighbours
    /// </summary>
    public class NeighbourController : Controller
    {
        private string tokenString = null;
        private Api api;
        private MapperConfiguration mapConfig;

        public NeighbourController()
        {
            mapConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<PathPoint, PathPointViewModel>();
                cfg.CreateMap<PathPointViewModel, PathPoint>();
                cfg.CreateMap<NeighbourViewModel, Neighbour>();
                cfg.CreateMap<Neighbour, NeighbourViewModel>();
                cfg.CreateMap<Coordinate, CoordinateViewModel>();
                cfg.CreateMap<CoordinateViewModel, Coordinate>(); 
            });
        }

        /// <summary>
        /// Get a list of all neighbours
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            List<Neighbour> list = api.GetAllNeighbours();
            var mapper = mapConfig.CreateMapper();
            List<NeighbourViewModel> viewModel = mapper.Map<List<NeighbourViewModel>>(list); 
            return View(viewModel);
        }

        /// <summary>
        /// Create a neighbour (GET) 
        /// </summary>
        /// <returns>A view that allows the user to create a neighbour</returns>
        public ActionResult Create()
        {
            List<PathPoint> pathPoints = api.GetAllPathPoints();
            var mapper = mapConfig.CreateMapper();
            List<PathPointViewModel> list = mapper.Map<List<PathPointViewModel>>(pathPoints);
            if (list.Count < 2)
            {
                ViewBag.ErrorMessage = "Ikke nok punkter til å knytte sammen";
                return View("ErrorView");
            }
            NeighbourViewModel viewModel = new NeighbourViewModel();
            viewModel.Liste = new SelectList(list.Select(x => new SelectListItem { Text = "Id:" + x.Id.ToString() + " - Lat: " + x.Coordinate.Lat + " - Lng: " + x.Coordinate.Lng, Value = x.Id.ToString() }).ToList(), "Value", "Text");
            return View(viewModel);
        }

        /// <summary>
        /// Create a neighbour (POST)
        /// </summary>
        /// <param name="input">The neighbour to create</param>
        /// <returns>A view that allows the user to create a neighbour</returns>
        [HttpPost]
        public ActionResult Create(NeighbourViewModel input)
        {
            if (!ModelState.IsValid)
            {
                return View(); 
            }
            var mapper = mapConfig.CreateMapper();
            Neighbour neighbour = mapper.Map<Neighbour>(input);
            List<PathPoint> pathPoints = api.GetAllPathPoints();
            List<PathPointViewModel> list = mapper.Map<List<PathPointViewModel>>(pathPoints);
            if (list.Count < 2)
            {
                ViewBag.ErrorMessage = "Ikke nok punkter til å knytte sammen";
                return View("ErrorView");
            }
            if (api.PostNeighbour(neighbour))
            {
                ViewBag.SuccessMessage = "Naboen ble opprettet"; 
            }
            else
            {
                ViewBag.ErrorMessage = "En feil oppstod, kontakt en systemadministrator"; 
            }
            NeighbourViewModel viewModel = new NeighbourViewModel();
            viewModel.Liste = new SelectList(list.Select(x => new SelectListItem { Text = "Id:" + x.Id.ToString() + " - Lat: " + x.Coordinate.Lat + " - Lng: " + x.Coordinate.Lng, Value = x.Id.ToString() }).ToList(), "Value", "Text");
            return View(viewModel); 
        }

        /// <summary>
        /// Delete a specific neighbour (GET) 
        /// </summary>
        /// <param name="id">The id of the neighbour to delete</param>
        /// <returns>A view that allows the user to delete a neighbour</returns>
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                ViewBag.ErrorMessage = "Du må spesifisere en ID";
                return View("ErrorView"); 
            }
            Neighbour data; 
            if ((data = api.GetNeighbourById(id.Value)) == null)
            {
                ViewBag.ErrorMessage = "Kunne ikke finne naboen du forespurte";
                return View("ErrorView"); 
            }
            var mapper = mapConfig.CreateMapper();
            NeighbourViewModel viewModel = mapper.Map<NeighbourViewModel>(data);
            return View(viewModel); 
        }

        /// <summary>
        /// Edit a specific neighbour (GET) 
        /// </summary>
        /// <param name="id">The id of the neighbour to delete</param>
        /// <returns>A view that allows the user to edit the neighbour</returns>
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                ViewBag.ErrorMessage = "Du må spesifisere en ID";
                return View("ErrorView");
            }
            Neighbour data;
            if ((data = api.GetNeighbourById(id.Value)) == null)
            {
                ViewBag.ErrorMessage = "Kunne ikke finne naboen du forespurte";
                return View("ErrorView");
            }
            var mapper = mapConfig.CreateMapper();
            NeighbourViewModel viewModel = mapper.Map<NeighbourViewModel>(data);
            return View(viewModel); 
        }

        /// <summary>
        /// Edit a specific neighbour (POST) 
        /// </summary>
        /// <param name="id">The id of the neighbour to delete</param>
        /// <param name="input">Object with updated fields</param>
        /// <returns>A view that allows the user to edit the neighbour</returns>
        [HttpPost]
        public ActionResult Edit(int id, [Bind(Include ="Distance")] NeighbourViewModel input)
        {
            Neighbour data;
            if ((data = api.GetNeighbourById(id)) == null)
            {
                ViewBag.ErrorMessage = "Kunne ikke finne naboen du forespurte";
                return View("ErrorView");
            }
            data.Distance = input.Distance; 
            if (api.UpdateNeighbour(id,data))
            {
                ViewBag.SuccessMessage = "Naboen ble oppdatert"; 
            }
            else
            {
                ViewBag.ErrorMessage = "En feil oppstod, kontakt en systemadministrator"; 
            }
            return View(input); 
        }

        /// <summary>
        /// Delete a specific neighbour (POST) 
        /// </summary>
        /// <param name="id">The id of the neighbour to delete</param>
        /// <returns>A view that allows the user to delete a neighbour</returns>
        [HttpPost]
        public ActionResult Delete(int id)
        {
            Neighbour data;
            if ((data = api.GetNeighbourById(id)) == null)
            {
                ViewBag.ErrorMessage = "Kunne ikke finne naboen du forespurte";
                return View("ErrorView");
            }

            if (api.DeleteNeighbour(id))
            {
                ViewBag.SuccessMessage = "Naboen ble slettet"; 
            }
            else
            {
                ViewBag.ErrorMessage = "En feil oppstod, kontakt en systemadministrator"; 
            }
            var mapper = mapConfig.CreateMapper();
            NeighbourViewModel viewModel = mapper.Map<NeighbourViewModel>(data);
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