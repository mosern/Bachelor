using Api.Classes;
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
}