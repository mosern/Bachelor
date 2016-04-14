using Api.Classes;
using Api.Dto;
using Api.Models.Api;
using Api.Models.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using UserDB;

namespace Api.Factories
{
    public class ConversionFactory
    {
        public static UserViewModel UserToViewModel(User user)
        {
            LocationRepository<UserLocation> uLocRepo = new LocationRepository<UserLocation>();
            LocationRepository<Location> locRepo = new LocationRepository<Location>();

            var uLocs = uLocRepo.List().Where(u => u.UserId == user.Id);
            List<LocationViewModel> locs = new List<LocationViewModel>();

            foreach(UserLocation uLoc in uLocs)
            {
                Location temp = locRepo.Read(uLoc.LocationId);
                temp.Hits = uLoc.Hits;
                locs.Add(LocationToViewModel(temp));
            }

            return new UserViewModel()
            {
                Id = user.Id,
                Username = user.Username,
                Locations = locs.AsQueryable()
            };
        }

        public static LocationViewModel UserLocationToViewModel(UserLocation userLocation)
        {
            LocationRepository<Location> locRepo = new LocationRepository<Location>();
            LocationRepository<Models.EF.Type> typeRepo = new LocationRepository<Models.EF.Type>();
            LocationRepository<Coordinate> coorRepo = new LocationRepository<Coordinate>();

            Location location = locRepo.Read(userLocation.Id);

            return new LocationViewModel()
            {
                Id = location.Id,
                Name = location.Name,
                Hits = userLocation.Hits,
                LocNr = location.LocNr,
                Type = TypeToViewModel(typeRepo.Read(location.TypeId)),
                Coordinate = CoordinateToViewModel(coorRepo.Read(location.CoordinateId))
            };
        }

        public static LocationViewModel LocationToViewModel(Location location)
        {
            LocationRepository<Models.EF.Type> typeRepo = new LocationRepository<Models.EF.Type>();
            LocationRepository<Coordinate> coorRepo = new LocationRepository<Coordinate>();

            return new LocationViewModel()
                    {
                        Id = location.Id,
                        Name = location.Name,
                        Hits = location.Hits,
                        LocNr = location.LocNr,
                        Type = TypeToViewModel(typeRepo.Read(location.TypeId)),
                        Coordinate = CoordinateToViewModel(coorRepo.Read(location.CoordinateId))
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

        public static IQueryable<UserViewModel> QueryUserToViewModel(IQueryable<User> users)
        {
            List<UserViewModel> view = new List<UserViewModel>();

            foreach(User user in users)
            {
                view.Add(UserToViewModel(user));
            }

            return view.AsQueryable();
        }

        public static IQueryable<LocationViewModel> QueryUserLocationToViewModel(IQueryable<UserLocation> userLocations)
        {
            List<LocationViewModel> view = new List<LocationViewModel>();

            foreach (UserLocation userLocation in userLocations)
            {
                view.Add(UserLocationToViewModel(userLocation));
            }

            return view.AsQueryable();
        }

        public static IQueryable<LocationViewModel> QueryLocationToViewModel(IQueryable<Location> locations)
        {
            List<LocationViewModel> view = new List<LocationViewModel>();

            foreach(Location location in locations)
            {
                view.Add(LocationToViewModel(location));
            }

            return view.AsQueryable();
        }

        public static IQueryable<PeopleViewModel> QueryPeopleToViewModel(IQueryable<People> people)
        {
            List<PeopleViewModel> view = new List<PeopleViewModel>();

            foreach (People peo in people)
            {
                view.Add(PeopleToViewModel(peo));
            }

            return view.AsQueryable();
        }

        public static IQueryable<object> SearchToQuerry(SearchViewModel searchViewModel)
        {
            return new List<object>() { searchViewModel.LocationViewModel, searchViewModel.PeopleViewModel }.AsQueryable();
        } 
    }
}