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
    [ResourceAuthorize("Write", "Admin")]
    [HandleForbidden]
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
        /// <returns></returns>
        public ActionResult Create()
        {
            return View();
        }


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