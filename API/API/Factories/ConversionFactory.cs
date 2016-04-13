using Api.Classes;
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
        static LocationRepository<Coordinate> coorRepo = new LocationRepository<Coordinate>();
        static LocationRepository<Models.EF.Type> typeRepo = new LocationRepository<Models.EF.Type>();

        public static LocationViewModel LocationToViewModel(Location Location)
        {
            return new LocationViewModel()
                    {
                        Id = Location.Id,
                        Name = Location.Name,
                        Hits = Location.Hits,
                        LocNr = Location.LocNr,
                        Type = typeRepo.Read(Location.TypeId),
                        Coordinate = coorRepo.Read(Location.CoordinateId)
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

        public static IQueryable<object> searchToQuerry(SearchViewModel searchViewModel)
        {
            return new
        } 
    }
}