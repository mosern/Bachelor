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
        public IHttpActionResult Get(string fields = null, string sort = "id")
        {
            var users = userRepo.List().ApplySort(sort);

            if (fields != null)
            {
                return Ok(UserInfo.ShapeList(users, fields.ToLower().Split(',').ToList()));
            }
            else
            {

                return Ok(UserInfo.List(users));
            }

        }

        [Route("users/{id}")]
        public IHttpActionResult Get(int id, string fields = null)
        {
            var user = userRepo.Read(id);
            if (user != null)
            {
                if (fields != null)
                {
                    return Ok(UserInfo.Shape(user, fields.ToLower().Split(',').ToList()));
                }
                else
                {

                    return Ok(new UserInfo(user));
                }
            }
            else
            {
                return BadRequest();
            }
        }

        [Route("users/{id}/locations/")]
        public IHttpActionResult get(int id, string fields = null, string sort = "id")
        {
            var locations = userLocRepo.List().Where(u => u.UserId == id).ApplySort(sort);

            if (locations != null)
            {
                return Ok(UserLocationInfo.ShapeList(locations, fields.ToLower().Split(',').ToList()));
            }
            else
            {
                return BadRequest();
            }
        }

        [Route("users/{id}/locations/{locid}")]
        public IHttpActionResult get(int id, int locId, string fields = null)
        {
            var userLocation = userLocRepo.List().Where(u => u.UserId == id && u.Id == locId).FirstOrDefault();

            if (userLocation != null)
            {
                    return Ok(UserLocationInfo.Shape(userLocation, fields.ToLower().Split(',').ToList()));
            }
            else
            {
                return BadRequest();
            }
        }
    }
}
