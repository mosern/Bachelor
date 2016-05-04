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

namespace AdmAspNet.Controllers
{
    public class PeopleController : Controller
    {
        private string tokenString = null;
        private Api api;
        private MapperConfiguration mapConfig;

        public PeopleController()
        {
            mapConfig = new MapperConfiguration(cfg => {
                cfg.CreateMap<PeopleViewModel, People>();
                cfg.CreateMap<People, PeopleViewModel>();
                cfg.CreateMap<Location, LocationViewModel>();
                cfg.CreateMap<LocationViewModel, Location>();
                cfg.CreateMap<Models.DataContracts.Type, TypeViewModel>();
                cfg.CreateMap<TypeViewModel, Models.DataContracts.Type>();
                cfg.CreateMap<CoordinateViewModel, Coordinate>();
                cfg.CreateMap<Coordinate, CoordinateViewModel>(); 
            }); 
        }
        // GET: Person
        public ActionResult Index()
        {
            List<People> peopleList = api.GetAllPeople();
            var mapper = mapConfig.CreateMapper();
            List<PeopleViewModel> viewModel = mapper.Map<List<PeopleViewModel>>(peopleList); 
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