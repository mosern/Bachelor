using Api.Classes;
using Api.Dto;
using Api.Models.Api;
using Api.Models.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Api.Factories
{
    public class ConversionFactory
    {

        public static LocationViewModel LocationToViewModel(Location Location)
        {
            LocationRepository<Models.EF.Type> typeRepo = new LocationRepository<Models.EF.Type>();
            LocationRepository<Coordinate> coorRepo = new LocationRepository<Coordinate>();

            return new LocationViewModel()
                    {
                        Id = Location.Id,
                        Name = Location.Name,
                        Hits = Location.Hits,
                        LocNr = Location.LocNr,
                        Type = TypeToViewModel(typeRepo.Read(Location.TypeId)),
                        Coordinate = CoordinateToViewModel(coorRepo.Read(Location.CoordinateId))
                    };
        }

        public static Location DTOLocationToDatabase(DTOLocation dtoLocation)
        {
            LocationRepository<Models.EF.Type> typeRepo = new LocationRepository<Models.EF.Type>();
            LocationRepository<Coordinate> coorRepo = new LocationRepository<Coordinate>();

            return new Location()
            {
                Id = dtoLocation.Id,
                Name = dtoLocation.Name,
                Hits = dtoLocation.Hits,
                LocNr = dtoLocation.LocNr,
                Coordinate = coorRepo.Read(dtoLocation.CoordinateId),
                Type = typeRepo.Read(dtoLocation.TypeId)
            };
        }

        public static CoordinateViewModel CoordinateToViewModel(Coordinate coordinate)
        {
            return new CoordinateViewModel()
                    {
                        Id = coordinate.Id,
                        Lat = coordinate.Lat,
                        Lng = coordinate.Lng,
                        Alt = coordinate.Alt
                    };
        }

        public static TypeViewModel TypeToViewModel(Models.EF.Type type)
        {
            return new TypeViewModel()
                    {
                        Id = type.Id,
                        Name = type.Name
                    };
        }

        public static PeopleViewModel PeopleToViewModel(People people)
        {
            return new PeopleViewModel()
                    {
                        Id = people.Id,
                        Name = people.Name,
                        Email = people.Email,
                        TlfMobile = people.TlfMobile,
                        TlfOffice = people.TlfOffice,
                        LocationId = people.LocationId
                    };
        }

        public static IQueryable<LocationViewModel> queryLocationToViewModel(IQueryable<Location> locations)
        {
            List<LocationViewModel> view = new List<LocationViewModel>();

            foreach(Location location in locations)
            {
                view.Add(LocationToViewModel(location));
            }

            return view.AsQueryable();
        }

        public static IQueryable<PeopleViewModel> queryPeopleToViewModel(IQueryable<People> people)
        {
            List<PeopleViewModel> view = new List<PeopleViewModel>();

            foreach (People peo in people)
            {
                view.Add(PeopleToViewModel(peo));
            }

            return view.AsQueryable();
        }

        public static IQueryable<object> searchToQuerry(SearchViewModel searchViewModel)
        {
            return new List<object>() { searchViewModel.LocationViewModel, searchViewModel.PeopleViewModel }.AsQueryable();
        } 
    }
}