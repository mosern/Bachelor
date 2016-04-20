using Api.Classes;
using Api.Dto;
using Api.Factories;
using Api.Models.Api;
using Api.Models.EF;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using UserDB;

namespace Api
{
    //TODO check if all converters is setting id
    public class AutoMapConfig
    {
        public static IMapper configureMaping()
        {
            var config = new MapperConfiguration(c =>
            {
                c.CreateMap<User, UserViewModel>().ConvertUsing<UserTypeConverter>();
                c.CreateMap<UserViewModel, User>();

                c.CreateMap<Models.EF.Type, TypeViewModel>();
                c.CreateMap<TypeViewModel, Models.EF.Type>();

                c.CreateMap<People, PeopleViewModel>().ConvertUsing<PeopleTypeConverter>();
                c.CreateMap<PeopleViewModel, People>().ConvertUsing<PeopleViewTypeConverter>();

                c.CreateMap<Location, LocationViewModel>().ConvertUsing<LocationViewTypeConverter>();
                c.CreateMap<LocationViewModel, Location>().ConvertUsing<ViewLocationTypeConverter>();

                c.CreateMap<Coordinate, CoordinateViewModel>();
                c.CreateMap<CoordinateViewModel, Coordinate>();

                c.CreateMap<Accesspoint, AccesspointViewModel>().ConvertUsing<AccesspointTypeCoverter>();
                c.CreateMap<AccesspointViewModel, Accesspoint>().ConvertUsing<AccesspointViewTypeConverter>();

                c.CreateMap<UserLocation, LocationViewModel>().ConvertUsing<UserLocationTypeConverter>();

                c.CreateMap<SearchViewModel, IEnumerable<object>>().ConvertUsing<SearchTypeConverter>();
            });

            return config.CreateMapper();
        }         
    }

    public class UserTypeConverter : ITypeConverter<User, UserViewModel>
    {
        public UserViewModel Convert(ResolutionContext context)
        {
            User user = (User)context.SourceValue;

            IQueryable<UserLocation> uLocs;
            using (var repo = new LocationRepository<UserLocation>())
                uLocs = repo.List().Where(u => u.UserId == user.Id);

            List<LocationViewModel> locs = new List<LocationViewModel>();

            using (var repo = new LocationRepository<Location>())
                foreach (UserLocation uLoc in uLocs)
                {
                    Location temp = repo.Read(uLoc.LocationId);
                    temp.Hits = uLoc.Hits;
                    locs.Add(AutoMapConfig.configureMaping().Map<Location, LocationViewModel>(temp));
                }

            return new UserViewModel()
            {
                Id = user.Id,
                Username = user.Username,
                Locations = locs.AsQueryable()
            };
        }
    }

    //TODO unnecessary?
    public class UserViewTypeConverter : ITypeConverter<UserViewModel, User>
    {
        public User Convert(ResolutionContext context)
        {
            UserViewModel source = (UserViewModel)context.SourceValue;
            User dest = new User()
            {
                Id = source.Id.Value,
                Username = source.Username
            };

            return dest;
        }
    }

    public class UserLocationTypeConverter : ITypeConverter<UserLocation, LocationViewModel>
    {
        public LocationViewModel Convert(ResolutionContext context)
        {
            UserLocation userLocation = (UserLocation)context.SourceValue;

            Location location;
            using (var repo = new LocationRepository<Location>())
                location = repo.Read(userLocation.Id);

            using (var typeRepo = new LocationRepository<Models.EF.Type>())
            using (var coorRepo = new LocationRepository<Coordinate>())
                return new LocationViewModel()
                {
                    Id = location.Id,
                    Name = location.Name,
                    Hits = userLocation.Hits,
                    LocNr = location.LocNr,
                    Type = AutoMapConfig.configureMaping().Map<Models.EF.Type, TypeViewModel>(typeRepo.Read(location.TypeId)),
                    Coordinate = AutoMapConfig.configureMaping().Map<Coordinate, CoordinateViewModel>(coorRepo.Read(location.CoordinateId))
                };
        }
    }

    public class IQUserLocationTypeConverter : ITypeConverter<IQueryable<UserLocation>, IQueryable<LocationViewModel>>
    {
        public IQueryable<LocationViewModel> Convert(ResolutionContext context)
        {
            IQueryable<UserLocation> userLocations = (IQueryable<UserLocation>)context.SourceValue;
            List<LocationViewModel> view = new List<LocationViewModel>();

            foreach (UserLocation userLocation in userLocations)
            {
                view.Add(AutoMapConfig.configureMaping().Map<UserLocation, LocationViewModel>(userLocation));
            }

            return view.AsQueryable();
        }
    }

    public class ViewLocationTypeConverter : ITypeConverter<LocationViewModel, Location>
    {
        public Location Convert(ResolutionContext context)
        {

            LocationViewModel source = (LocationViewModel)context.SourceValue;

            Coordinate cor;
            Location dest;
            using (var repo = new LocationRepository<Coordinate>())
            {
                cor = repo.Create(new Coordinate() { Lat = source.Coordinate.Lat, Lng = source.Coordinate.Lng, Alt = source.Coordinate.Alt });

                dest = new Location()
                {
                    Id = source.Id.Value,
                    Name = source.Name,
                    Hits = source.Hits,
                    LocNr = source.LocNr,
                    CoordinateId = cor.Id,
                    TypeId = source.Type.Id.Value,
                    //Coordinate = new LocationRepository<Coordinate>().Read(source.Coordinate.Id),
                    //Type = new LocationRepository<Models.EF.Type>().Read(source.Type.Id)
                };
            }
                

            return dest;
        }
    }

    public class LocationViewTypeConverter : ITypeConverter<Location, LocationViewModel>
    {
        public LocationViewModel Convert(ResolutionContext context)
        {

            Location source = (Location)context.SourceValue;
            LocationViewModel dest;
            using (var coorRepo = new LocationRepository<Coordinate>())
            using (var typeRepo = new LocationRepository<Models.EF.Type>())
                dest = new LocationViewModel()
                {
                    Id = source.Id,
                    Name = source.Name,
                    Hits = source.Hits,
                    LocNr = source.LocNr,
                    Coordinate = AutoMapConfig.configureMaping().Map<Coordinate, CoordinateViewModel>(coorRepo.Read(source.CoordinateId)),
                    Type = AutoMapConfig.configureMaping().Map<Models.EF.Type, TypeViewModel>(typeRepo.Read(source.TypeId))
                };

            return dest;
        }
    }

    public class PeopleTypeConverter : ITypeConverter<People, PeopleViewModel>
    {
        public PeopleViewModel Convert(ResolutionContext context)
        {
            People source = (People)context.SourceValue;
            PeopleViewModel dest = new PeopleViewModel()
            {
                Id = source.Id,
                Name = source.Name,
                Email = source.Email,
                TlfMobile = source.TlfMobile,
                TlfOffice = source.TlfOffice,
                Location = source.Location
            };

            return dest;
        }
    }

    //TODO unnecessary?
    public class PeopleViewTypeConverter : ITypeConverter<PeopleViewModel, People>
    {
        public People Convert(ResolutionContext context)
        {
            PeopleViewModel source = (PeopleViewModel)context.SourceValue;

            People dest;
            using (var repo = new LocationRepository<Location>())
                dest = new People()
            {
                Id = source.Id.Value,
                Name = source.Name,
                Email = source.Email,
                TlfMobile = source.TlfMobile,
                TlfOffice = source.TlfOffice,
                LocationId = source.Location.Id,
                Location = source.Location         
            };

            return dest;
        }
    }

    public class AccesspointTypeCoverter : ITypeConverter<Accesspoint, AccesspointViewModel>
    {
        public AccesspointViewModel Convert(ResolutionContext context)
        {
            Accesspoint source = (Accesspoint)context.SourceValue;

            AccesspointViewModel dest;
            using (var repo = new LocationRepository<Coordinate>())
                dest = new AccesspointViewModel()
                {
                    Id = source.Id,
                    Desc = source.Desc,
                    MacAddress = source.MacAddress,
                    Coordinate = AutoMapConfig.configureMaping().Map<Coordinate, CoordinateViewModel>(repo.Read(source.Coordinate.Id))

                };

            return dest;
        }
    }

    public class AccesspointViewTypeConverter : ITypeConverter<AccesspointViewModel, Accesspoint>
    {
        public Accesspoint Convert(ResolutionContext context)
        {
            AccesspointViewModel source = (AccesspointViewModel)context.SourceValue;
            Accesspoint dest = new Accesspoint()
            {
                Id = source.Id.Value,
                Desc = source.Desc,
                MacAddress = source.MacAddress,
                CoordinateId = source.Coordinate.Id.Value
            };

            return dest;
        }
    }

    public class SearchTypeConverter : ITypeConverter<SearchViewModel, IEnumerable<object>>
    {
        public IEnumerable<object> Convert(ResolutionContext context)
        {
            SearchViewModel searchViewModel = (SearchViewModel)context.SourceValue;
            return new List<object>() { searchViewModel.LocationViewModel, searchViewModel.PeopleViewModel }.AsQueryable();
        }
    }
}