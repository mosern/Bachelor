using Api.Classes;
using Api.Models.Api;
using Api.Models.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using UserDB;

namespace Api.Controllers
{
    [RoutePrefix("api")]
    public class UserController : ApiController
    {
        Repository<User> userRepo = new Repository<User>();
        LocationRepository<UserLocation> userLocRepo = new LocationRepository<UserLocation>();

        [Route("users")]
        public IHttpActionResult Get(string fields, string sort)
        {
            var users = userRepo.List();

            if(fields != null)
            {      
                return Ok(UserInfo.ShapeList(users, fields.ToLower().Split(',').ToList()));
            }
            else
            {

                return Ok(UserInfo.List(users));
            }
            
        }

        [Route("users")]
        public IHttpActionResult Get()
        {
            var users = userRepo.List();
            return Ok(UserInfo.List(users));
        }
    }
}
