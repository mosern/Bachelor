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

                c.CreateMap<UserLocation, LocationViewModel>().ConvertUsing<UserLocationTypeConverter>();

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
            });

            return config.CreateMapper();
        }         
    }

    public class UserTypeConverter : ITypeConverter<User, UserViewModel>
    {
        public UserViewModel Convert(ResolutionContext context)
        {
            return ConversionFactory.UserToViewModel((User)context.SourceValue);
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
            return ConversionFactory.UserLocationToViewModel((UserLocation)context.SourceValue);
        }
    }

    public class IQUserLocationTypeConverter : ITypeConverter<IQueryable<UserLocation>, IQueryable<LocationViewModel>>
    {
        public IQueryable<LocationViewModel> Convert(ResolutionContext context)
        {
            return ConversionFactory.QueryUserLocationToViewModel((IQueryable<UserLocation>)context.SourceValue);
        }
    }

    public class ViewLocationTypeConverter : ITypeConverter<LocationViewModel, Location>
    {
        public Location Convert(ResolutionContext context)
        {

            LocationViewModel source = (LocationViewModel)context.SourceValue;
            Coordinate cor = new LocationRepository<Coordinate>().Create(new Coordinate() { Lat = source.Coordinate.Lat, Lng = source.Coordinate.Lng, Alt = source.Coordinate.Alt });
            Location dest = new Location()
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

            return dest;
        }
    }

    public class LocationViewTypeConverter : ITypeConverter<Location, LocationViewModel>
    {
        public LocationViewModel Convert(ResolutionContext context)
        {

            Location source = (Location)context.SourceValue;
            LocationViewModel dest = new LocationViewModel()
            {
                Name = source.Name,
                Hits = source.Hits,
                LocNr = source.LocNr,
                Coordinate = AutoMapConfig.configureMaping().Map<Coordinate, CoordinateViewModel>(new LocationRepository<Coordinate>().Read(source.CoordinateId)),
                Type = AutoMapConfig.configureMaping().Map<Models.EF.Type, TypeViewModel>(new LocationRepository<Models.EF.Type>().Read(source.TypeId))
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
                Name = source.Name,
                Email = source.Email,
                TlfMobile = source.TlfMobile,
                TlfOffice = source.TlfOffice,
                LocationId = source.Location.Id
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
            People dest = new People()
            {
                Name = source.Name,
                Email = source.Email,
                TlfMobile = source.TlfMobile,
                TlfOffice = source.TlfOffice,
                LocationId = source.LocationId,
                Location = new LocationRepository<Location>().Read(source.LocationId)          
            };

            return dest;
        }
    }

    public class AccesspointTypeCoverter : ITypeConverter<Accesspoint, AccesspointViewModel>
    {
        public AccesspointViewModel Convert(ResolutionContext context)
        {
            Accesspoint source = (Accesspoint)context.SourceValue;
            AccesspointViewModel dest = new AccesspointViewModel()
            {
                Desc = source.Desc,
                MacAddress = source.MacAddress,
                Coordinate = AutoMapConfig.configureMaping().Map<Coordinate, CoordinateViewModel>(new LocationRepository<Coordinate>().Read(source.Coordinate.Id))

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
                Desc = source.Desc,
                MacAddress = source.MacAddress,
                CoordinateId = source.Coordinate.Id.Value
            };

            return dest;
        }
    }
}