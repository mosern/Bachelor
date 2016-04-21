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

                c.CreateMap<People, PeopleViewModel>();
                c.CreateMap<PeopleViewModel, People>();

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
            using (var userLocRepo = new LocationRepository<UserLocation>())
            using (var locRepo = new LocationRepository<Location>())
            {
                uLocs = userLocRepo.List().Where(u => u.UserId == user.Id);

                List<LocationViewModel> locs = new List<LocationViewModel>();


                foreach (UserLocation uLoc in uLocs)
                {
                    Location temp = locRepo.Read(uLoc.LocationId);
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
                    Desc = location.Desc,
                    Hits = userLocation.Hits,
                    LocNr = location.LocNr,
                    Type = AutoMapConfig.configureMaping().Map<Models.EF.Type, TypeViewModel>(typeRepo.Read(location.TypeId)),
                    Coordinate = AutoMapConfig.configureMaping().Map<Coordinate, CoordinateViewModel>(coorRepo.Read(location.CoordinateId))
                };
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
                    Desc = source.Desc,
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
                    Desc = source.Desc,
                    Hits = source.Hits,
                    LocNr = source.LocNr,
                    Coordinate = AutoMapConfig.configureMaping().Map<Coordinate, CoordinateViewModel>(coorRepo.Read(source.CoordinateId)),
                    Type = AutoMapConfig.configureMaping().Map<Models.EF.Type, TypeViewModel>(typeRepo.Read(source.TypeId))
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