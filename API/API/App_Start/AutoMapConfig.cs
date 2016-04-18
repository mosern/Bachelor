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
    public class AutoMapConfig
    {
        public static IMapper configureMaping()
        {
            var config = new MapperConfiguration(c =>
            {
                c.CreateMap<User, UserViewModel>().ConvertUsing<UserTypeConverter>();
                c.CreateMap<User, User>();
                c.CreateMap<UserLocation, LocationViewModel>().ConvertUsing<UserLocationTypeConverter>();
                c.CreateMap<Models.EF.Type, TypeViewModel>();
                c.CreateMap<Models.EF.Type, Models.EF.Type>();
                c.CreateMap<People, PeopleViewModel>();
                c.CreateMap<People, People>().ConvertUsing<PeopleTypeFiller>();
                c.CreateMap<Location, LocationViewModel>();
                c.CreateMap<Location, Location>().ConvertUsing<LocationTypeFiller>();
                c.CreateMap<Coordinate, CoordinateViewModel>();
                c.CreateMap<Coordinate, Coordinate>();
                c.CreateMap<Accesspoint, AccesspointViewModel>();
                c.CreateMap<Accesspoint, Accesspoint>().ConvertUsing<AccesspointTypeFiller>();
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

    public class LocationTypeFiller : ITypeConverter<Location, Location>
    {
        public Location Convert(ResolutionContext context)
        {

            Location source = (Location)context.SourceValue;
            Coordinate cor = new LocationRepository<Coordinate>().Create(new Coordinate() { Lat = source.Coordinate.Lat, Lng = source.Coordinate.Lng, Alt = source.Coordinate.Alt });
            Location dest = new Location()
            {
                Name = source.Name,
                Hits = source.Hits,
                LocNr = source.LocNr,
                CoordinateId = cor.Id,
                TypeId = source.Type.Id,
                //Coordinate = new LocationRepository<Coordinate>().Read(source.Coordinate.Id),
                //Type = new LocationRepository<Models.EF.Type>().Read(source.Type.Id)
            };

            return dest;
        }
    }

    public class PeopleTypeFiller : ITypeConverter<People, People>
    {
        public People Convert(ResolutionContext context)
        {
            People source = (People)context.SourceValue;
            People dest = new People()
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

    public class AccesspointTypeFiller : ITypeConverter<Accesspoint, Accesspoint>
    {
        public Accesspoint Convert(ResolutionContext context)
        {
            Accesspoint source = (Accesspoint)context.SourceValue;
            Accesspoint dest = new Accesspoint()
            {
                Desc = source.Desc,
                MacAddress = source.MacAddress,
                CoordinateId = source.Coordinate.Id
            };

            return dest;
        }
    }
}