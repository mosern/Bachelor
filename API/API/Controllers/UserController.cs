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

namespace Api.Controllers
{
    [RoutePrefix("api")]
    public class UserController : ApiController
    {
        Repository<User> userRepo = new Repository<User>();
        LocationRepository<UserLocation> userLocRepo = new LocationRepository<UserLocation>();
        const int stdPageSize = 5;


        [Route("users", Name = "users")]
        public IHttpActionResult Get(string fields = null, string sort = "id", int page = 1, int pageSize = stdPageSize, bool asObject = false, string objPropName = "users")
        {
            var users = userRepo.List().ApplySort(sort).Skip(pageSize * (page - 1)).Take(pageSize);

            if (users != null)
            {
                IDictionary<string, object> routeValues = new Dictionary<string, object>();

                routeValues.Add("fields", fields);
                routeValues.Add("sort", fields);

                HttpContext.Current.Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(PaginationHeader.Get(page, pageSize, users.Count(), "users", routeValues)));

                if (fields != null)
                {
                    if (asObject)
                    {
                        var toReturn = UserInfo.ShapeList(users, fields.ToLower().Split(',').ToList());
                        return Ok(JsonHelper.listToObject(toReturn, objPropName));
                    }
                    else
                    {
                        return Ok(UserInfo.ShapeList(users, fields.ToLower().Split(',').ToList()));
                    }
                }
                else
                {
                    if (asObject)
                    {
                        return Ok(JsonHelper.listToObject(UserInfo.List(users), objPropName));
                    }
                    else
                    {
                        return Ok(UserInfo.List(users));
                    }
                }
            }
            else
            {
                return BadRequest();
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

        [Route("users/{id}/locations/", Name="userlocations")]
        public IHttpActionResult Get(int id, string fields = null, string sort = "id", int page = 1, int pageSize = stdPageSize, bool asObject = false, string objPropName = "userLocations")
        {
            var locations = userLocRepo.List().Where(u => u.UserId == id).ApplySort(sort).Skip(pageSize * (page - 1)).Take(pageSize);

            if (locations != null)
            {
                IDictionary<string, object> routeValues = new Dictionary<string, object>();

                if (fields != null)
                {
                    routeValues.Add("fields", fields);
                    routeValues.Add("sort", sort);

                    if (asObject)
                    {
                        var toReturn = UserLocationInfo.ShapeList(locations, fields.ToLower().Split(',').ToList());
                        return Ok(JsonHelper.listToObject(toReturn, objPropName));
                    }
                    else
                    {
                        return Ok(UserLocationInfo.ShapeList(locations, fields.ToLower().Split(',').ToList()));
                    }

                }
                else
                {
                    if (asObject)
                    {
                        routeValues.Add("sort", sort);

                        return Ok(JsonHelper.listToObject(UserLocationInfo.List(locations), objPropName));
                    }
                    else
                    {
                        routeValues.Add("sort", sort);

                        return Ok(UserLocationInfo.List(locations));
                    }
                }

                HttpContext.Current.Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(PaginationHeader.Get(page, pageSize, locations.Count(), "userlocations", routeValues)));
            }
            else
            {
                return BadRequest();
            }
        }

        [Route("users/{id}/locations/{locid}")]
        public IHttpActionResult Get(int id, int locId, string fields = null)
        {
            var userLocation = userLocRepo.Read(locId);

            if (userLocation != null)
            {
                if (fields != null)
                {
                    return Ok(UserLocationInfo.Shape(userLocation, fields.ToLower().Split(',').ToList()));
                }
                else
                {
                    return Ok(new UserLocationInfo(userLocation));
                }
            }
            else
            {
                return BadRequest();
            }
        }
    }
}
