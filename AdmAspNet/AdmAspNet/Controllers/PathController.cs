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
    /// A controller that handles CRUD functionality for pathpoints
    /// </summary>
    public class PathController : Controller
    {
        private string tokenString = null;
        private Api api;
        private MapperConfiguration mapConfig;

        public PathController()
        {
            mapConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<PathPoint, PathPointViewModel>();
                cfg.CreateMap<PathPointViewModel, PathPoint>();
                cfg.CreateMap<Coordinate, CoordinateViewModel>();
                cfg.CreateMap<CoordinateViewModel, Coordinate>();
                cfg.CreateMap<NeighbourViewModel, Neighbour>(); 
            });
        }
        /// <summary>
        /// Shows a list of all pathpoints 
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            List<PathPoint> list = api.GetAllPathPoints();
            var mapper = mapConfig.CreateMapper();
            List<PathPointViewModel> viewModel = mapper.Map<List<PathPointViewModel>>(list);
            return View(viewModel);
        }

        /// <summary>
        /// Create a new pathpoint (GET) 
        /// </summary>
        /// <returns>A view that allows the user to create a pathpoint</returns>
        public ActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Create a new pathpoint (POST) 
        /// </summary>
        /// <param name="input">The id of the pathpoint</param>
        /// <returns>A view that allows the user to create a pathpoint</returns>
        [HttpPost]
        public ActionResult Create([Bind(Include ="Lat,Lng,Alt")] CoordinateViewModel input)
        {
            if (!ModelState.IsValid)
            {
                return View(input); 
            }
            var mapper = mapConfig.CreateMapper();
            Coordinate coordinate = mapper.Map<Coordinate>(input);
            PathPoint pathPoint = new PathPoint();
            pathPoint.Coordinate = coordinate;
            if (api.PostPathPoint(pathPoint))
            {
                ViewBag.SuccessMessage = "Rutepunktet ble opprettet"; 
            } 
            else
            {
                ViewBag.ErrorMessage = "En feil oppstod, kontakt en systemadministrator"; 
            }
            return View(); 
        } 

        /// <summary>
        /// Edit a pathpoint (GET)
        /// </summary>
        /// <param name="id">The id of the pathpoint</param>
        /// <returns>A view that allows the user to edit the pathpoint</returns>
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                ViewBag.ErrorMessage = "Du må spesifisere en ID";
                return View("ErrorView"); 
            }
            PathPoint pathPoint; 
            if ((pathPoint = api.GetPathPointById(id.Value)) == null)
            {
                ViewBag.ErrorMessage = "Finner ikke spesifisert rutepunkt";
                return View("ErrorView"); 
            }
            var mapper = mapConfig.CreateMapper();
            PathPointViewModel pathPointViewModel = mapper.Map<PathPointViewModel>(pathPoint);
            return View(pathPointViewModel.Coordinate); 
        }

        /// <summary>
        /// Edit a pathpoint (POST)
        /// </summary>
        /// <param name="id">The id of the pathpoint</param>
        /// <param name="input">Coordinate with updated views</param>
        /// <returns>A view that allows the user to edit the pathpoint</returns>
        [HttpPost]
        public ActionResult Edit(int id, [Bind(Include ="Lat,Lng,Alt")] CoordinateViewModel input)
        {
            if (!ModelState.IsValid)
            {
                return View(input); 
            }
            PathPoint pathPoint;
            if ((pathPoint = api.GetPathPointById(id)) == null)
            {
                ViewBag.ErrorMessage = "Finner ikke spesifisert rutepunkt";
                return View("ErrorView");
            }
            var mapper = mapConfig.CreateMapper();
            Coordinate coordinate = mapper.Map<Coordinate>(input);
            coordinate.Id = 0; 
            pathPoint.Coordinate = coordinate; 
            if (api.UpdatePathPoint(id,pathPoint))
            {
                ViewBag.SuccessMessage = "Rutepunktet ble oppdatert";
            }
            else
            {
                ViewBag.ErrorMessage = "En feil oppstod, kontakt en systemadministrator";
            }
            return View(input);
        }

        /// <summary>
        /// Delete a pathpoint (GET)
        /// </summary>
        /// <param name="id">The id of the pathpoint</param>
        /// <returns>A view that allows the user to delete a pathpoint</returns>
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                ViewBag.ErrorMessage = "Du må spesifisere en ID";
                return View("ErrorView"); 
            }
            PathPoint pathPoint; 
            if ((pathPoint = api.GetPathPointById(id.Value)) == null)
            {
                ViewBag.ErrorMessage = "Finner ikke spesifisert rutepunkt";
                return View("ErrorView");
            }
            var mapper = mapConfig.CreateMapper();
            PathPointViewModel viewModel = mapper.Map<PathPointViewModel>(pathPoint); 
            return View(viewModel); 
        }

        /// <summary>
        /// Delete a pathpoint
        /// </summary>
        /// <param name="id">The id of the pathpoint</param>
        /// <returns>A view that allows the user to delete a pathpoint</returns>
        [HttpPost]
        public ActionResult Delete(int id)
        {
            PathPoint pathPoint;
            if ((pathPoint = api.GetPathPointById(id)) == null)
            {
                ViewBag.ErrorMessage = "Finner ikke spesifisert rutepunkt";
                return View("ErrorView");
            }
            var mapper = mapConfig.CreateMapper();
            PathPointViewModel viewModel = mapper.Map<PathPointViewModel>(pathPoint); 
            if (api.DeletePathPoint(id))
            {
                ViewBag.SuccessMessage = "Rutepunktet ble slettet"; 
            }
            else
            {
                ViewBag.ErrorMessage = "En feil oppstod, kontakt en systemadministrator"; 
            }
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