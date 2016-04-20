using Api.Classes;
using Api.Models.Api;
using Api.Models.EF;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using UserDB;
using Api.Factories;
using Api.Helpers;

namespace Api.Controllers
{
    [RoutePrefix("api")]
    public class UserController : ApiController
    {
        const int stdPageSize = 5;


        [Route("users", Name = "users")]
        public IHttpActionResult Get(string fields = null, string sort = "id", int? page = null, int pageSize = stdPageSize, bool asObject = true, string objPropName = "users")
        {

            IQueryable<User> users;
            object toReturn;
            using (var repo = new Repository<User>())
            {
                users = repo.List();

                toReturn = ControllerHelper.get<UserViewModel>(users, HttpContext.Current, Request, "userLocations", asObject, objPropName, fields, sort, page, pageSize);
            }


            if(toReturn != null)
            {
                return Ok(toReturn);
            }
            else
            {
                return BadRequest("No users found");
            }

        }

        [Route("users/{id}")]
        public IHttpActionResult Get(int id, string fields = null)
        {
            User user;
            using (var repo = new Repository<User>())
            {
                user = repo.Read(id);

                if (user != null)
                {
                    return Ok(ControllerHelper.get<UserViewModel>(user, fields));

                }
                else
                {
                    return BadRequest("No user found");
                }
            }
                
        }

        [Route("users/{id}/locations/", Name="userlocations")]
        public IHttpActionResult Get(int id, string fields = null, string sort = "id", int? page = null, int pageSize = stdPageSize, bool asObject = true, string objPropName = "userLocations")
        {
            IQueryable<UserLocation> locations;
            object toReturn;
            using (var repo = new LocationRepository<UserLocation>())
            {
                locations = repo.List().Where(u => u.UserId == id);

                toReturn = ControllerHelper.get<LocationViewModel>(locations, HttpContext.Current, Request, "userLocations", asObject, objPropName, fields, sort, page, pageSize);
            }

            if(toReturn != null)
            {
                return Ok(toReturn);
            }
            else
            {
                return BadRequest();
            }
        }

        [Route("users/{id}/locations/{locid}")]
        public IHttpActionResult Get(int id, int locId, string fields = null)
        {
            UserLocation userLocation;
            using (var repo = new LocationRepository<UserLocation>())
            {
                userLocation = repo.Read(locId);

                if (userLocation != null)
                {
                    return Ok(ControllerHelper.get<LocationViewModel>(userLocation, fields));
                }
                else
                {
                    return BadRequest();
                }
            }            
        }

        [Route("users")]
        public IHttpActionResult Post(User user)
        {
            if (!ModelState.IsValid)
                return BadRequest("Modelstate is invalid");

            try
            {
                return Created("api/users", ControllerHelper.post(user));
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [Route("users/{id}")]
        public IHttpActionResult Put(User user, int id)
        {
            if(user.Username != null && user.Password != null)
            {
                try
                {
                    user.Id = id;
                    ControllerHelper.Put(user);
                    return Ok();
                }
                catch
                {
                    return InternalServerError();
                }
            }
            else
            {
                return BadRequest();
            }
        }

        [Route("users/{id}")]
        public IHttpActionResult Patch(User user, int id)
        {
            if (user != null)
            {
                try
                {
                    user.Id = id;
                    ControllerHelper.Put(user);
                    return Ok();
                }
                catch
                {
                    return InternalServerError();
                }
            }
            else
            {
                return BadRequest();
            }
        }

        [Route("users/{id}")]
        public IHttpActionResult Delete(int id)
        {
            try
            {
                User user = new Repository<User>().Read(id);

                if(user != null)
                {
                    ControllerHelper.Delete(user);
                    return Ok(); 
                }
                else
                {
                    return BadRequest();
                }


            }
            catch
            {
                return InternalServerError();
            }
        }
    }
}
