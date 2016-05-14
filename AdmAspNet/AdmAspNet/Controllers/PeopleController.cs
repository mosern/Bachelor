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

        public ActionResult Create()
        {
            PeopleViewModel model = new PeopleViewModel(); 
            List<Location> listLocation = api.GetAllLocations(); 
            if (listLocation.Count == 0)
            {
                model.DropDown = new SelectList(listLocation); 
            }
            else
            {
                var ordered = listLocation.OrderBy(x => x.LocNr); 
                model.DropDown = new SelectList(ordered, "Id", "LocNr", ordered.First().Id); 
            }
            return View(model); 
        }

        [HttpPost]
        public ActionResult Create(PeopleViewModel input)
        {
            List<Location> listLocation = api.GetAllLocations();
            if (listLocation.Count == 0)
            {
                input.DropDown = new SelectList(listLocation);

            }
            else
            {
                var ordered = listLocation.OrderBy(x => x.LocNr); 
                input.DropDown = new SelectList(ordered, "Id", "LocNr", ordered.First().Id);
            }
            if (!ModelState.IsValid)
            {
                return View(input); 
            }
            var mapper = mapConfig.CreateMapper();
            People people = mapper.Map<People>(input); 
            if (api.PostPeople(people))
            {
                ViewBag.SuccessMessage = "Personen ble opprettet"; 
            }
            else
            {
                ViewBag.ErrorMessage = "En feil oppstod, kontakt en systemadministrator"; 
            }
            PeopleViewModel baseModel = new PeopleViewModel();
            baseModel.DropDown = input.DropDown;
            return View(baseModel); 
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                ViewBag.ErrorMessage = "Du må spesifisere en ID";
                return View("ErrorView"); 
            }
            People data; 
            if ((data = api.GetPeopleById(id.Value)) == null)
            {
                ViewBag.ErrorMessage = "Kan ikke finne personen du forespurte";
                return View("ErrorView"); 
            }
            var mapper = mapConfig.CreateMapper();
            PeopleViewModel viewModel = mapper.Map<PeopleViewModel>(data);
            List<Location> listLocation = api.GetAllLocations();
            if (listLocation.Count == 0)
            {
                viewModel.DropDown = new SelectList(listLocation);

            }
            else
            {
                var ordered = listLocation.OrderBy(x => x.LocNr);
                viewModel.DropDown = new SelectList(ordered, "Id", "LocNr", ordered.Where(p => p.Id == viewModel.Location.Id).First().Id);
            }
            return View(viewModel);  
        }

        [HttpPost]
        public ActionResult Edit(int id, PeopleViewModel data)
        {
            People people; 
            if ((people = api.GetPeopleById(id)) == null)
            {
                ViewBag.ErrorMessage = "Kan ikke finne personen du forespurte";
                return View("ErrorView"); 
            }
            var mapper = mapConfig.CreateMapper();
            People edit = mapper.Map<People>(data); 
            if (api.UpdatePeople(id,edit))
            {
                ViewBag.SuccessMessage = "Personen ble oppdatert"; 
            }
            else
            {
                ViewBag.ErrorMessage = "En feil oppstod, kontakt en systemadministrator"; 
            }
            List<Location> listLocation = api.GetAllLocations(); 
            if (listLocation.Count == 0)
            {
                data.DropDown = new SelectList(listLocation); 
            }
            else
            {
                var ordered = listLocation.OrderBy(x => x.LocNr);
                data.DropDown = new SelectList(ordered, "Id", "LocNr", ordered.Where(p => p.Id == data.Location.Id).First().Id); 
            }
            return View(data); 
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