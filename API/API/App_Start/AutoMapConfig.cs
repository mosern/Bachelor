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
                c.CreateMap<UserLocation, LocationViewModel>().ConvertUsing<UserLocationTypeConverter>();
                //c.CreateMap<IQueryable<UserLocation>, IQueryable<LocationViewModel>>().ConvertUsing<IQUserLocationTypeConverter>();
                c.CreateMap<Models.EF.Type, TypeViewModel>();
                c.CreateMap<People, PeopleViewModel>();
                c.CreateMap<Location, LocationViewModel>();
                c.CreateMap<IQueryable<Location>, IQueryable<LocationViewModel>>();
                c.CreateMap<Coordinate, CoordinateViewModel>();
                c.CreateMap<Location, DTOLocation>();
                c.CreateMap<People, DTOPeople>();
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
}