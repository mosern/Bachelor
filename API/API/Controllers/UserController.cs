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
        Repository<User> userRepo = new Repository<User>();
        LocationRepository<UserLocation> userLocRepo = new LocationRepository<UserLocation>();
        const int stdPageSize = 5;


        [Route("users", Name = "users")]
        public IHttpActionResult Get(string fields = null, string sort = "id", int? page = null, int pageSize = stdPageSize, bool asObject = true, string objPropName = "users")
        {
            IQueryable<User> users = userRepo.List();
            object toReturn = ControllerHelper.get<UserViewModel>(users, HttpContext.Current, Request, "userLocations", asObject, objPropName, fields, sort, page, pageSize);

            if(toReturn != null)
            {
                return Ok(toReturn);
            //if (page != null)
            //{
            //    users = userRepo.List().ApplySort(sort).Skip(pageSize * (page.Value - 1)).Take(pageSize);
            //}
            //else
            //{
            //    users = userRepo.List().ApplySort(sort);
            //}

            //if (users != null)
            //{
            //    IDictionary<string, object> routeValues = new Dictionary<string, object>();

            //    if (fields != null)
            //        routeValues.Add("fields", fields);

            //    routeValues.Add("sort", fields);

            //    if(page != null)
            //    HttpContext.Current.Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(PaginationHeader.Get(page.Value, pageSize, users.Count(), "users", routeValues, Request)));

            //    if (fields != null)
            //    {
            //        if (asObject)
            //        {
            //            var toReturn = ShapeFactory<UserViewModel>.ShapeList(ConversionFactory.QueryUserToViewModel(users), fields.ToLower().Split(',').ToList());
            //            return Ok(JsonHelper.listToObject(toReturn, objPropName));
            //        }
            //        else
            //        {
            //            return Ok(ShapeFactory<UserViewModel>.ShapeList(ConversionFactory.QueryUserToViewModel(users), fields.ToLower().Split(',').ToList()));
            //        }
            //    }
            //    else
            //    {
            //        if (asObject)
            //        {
            //            return Ok(JsonHelper.listToObject(ConversionFactory.QueryUserToViewModel(users), objPropName));
            //        }
            //        else
            //        {
            //            return Ok(ConversionFactory.QueryUserToViewModel(users));
            //        }
            //    }
            }
            else
            {
                return BadRequest("No users found");
            }

        }

        [Route("users/{id}")]
        public IHttpActionResult Get(int id, string fields = null)
        {
            var user = userRepo.Read(id);
            if (user != null)
            {
                //if (fields != null)
                //{
                //    return Ok(ShapeFactory<UserViewModel>.Shape(AutoMapConfig.configureMaping().Map<User, UserViewModel>(user), fields.ToLower().Split(',').ToList()));
                //}
                //else
                //{

                //    return Ok(ConversionFactory.UserToViewModel(user));
                //}

                return Ok(ControllerHelper.get<UserViewModel>(user, fields));

            }
            else
            {
                return BadRequest("No user found");
            }
        }

        [Route("users/{id}/locations/", Name="userlocations")]
        public IHttpActionResult Get(int id, string fields = null, string sort = "id", int? page = null, int pageSize = stdPageSize, bool asObject = true, string objPropName = "userLocations")
        {

            IQueryable<UserLocation> locations = userLocRepo.List().Where(u => u.UserId == id);
            object toReturn = ControllerHelper.get<LocationViewModel>(locations, HttpContext.Current, Request, "userLocations", asObject, objPropName, fields, sort, page, pageSize );

            if(toReturn != null)
            {
                return Ok(toReturn);
            
            //IQueryable<UserLocation> locations;
            //if (page != null)
            //{
            //    locations = userLocRepo.List().Where(u => u.UserId == id).ApplySort(sort).Skip(pageSize * (page.Value - 1)).Take(pageSize);
            //}
            //else
            //{
            //    locations = userLocRepo.List().Where(u => u.UserId == id).ApplySort(sort);
            //}

            //if (locations != null)
            //{
            //    IDictionary<string, object> routeValues = new Dictionary<string, object>();

            //    if (fields != null)
            //    {
            //        routeValues.Add("fields", fields);
            //        routeValues.Add("sort", sort);

            //        if (page != null)
            //            HttpContext.Current.Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(PaginationHeader.Get(page.Value, pageSize, locations.Count(), "userlocations", routeValues, Request)));

            //        if (asObject)
            //        {
            //            var toReturn = ShapeFactory<LocationViewModel>.ShapeList(ConversionFactory.QueryUserLocationToViewModel(locations), fields.ToLower().Split(',').ToList());
            //            return Ok(JsonHelper.listToObject(toReturn, objPropName));
            //        }
            //        else
            //        {
            //            return Ok(ShapeFactory<LocationViewModel>.ShapeList(ConversionFactory.QueryUserLocationToViewModel(locations), fields.ToLower().Split(',').ToList()));
            //        }

            //    }
            //    else
            //    {
            //        routeValues.Add("sort", sort);

            //        if(page != null)
            //        HttpContext.Current.Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(PaginationHeader.Get(page.Value, pageSize, locations.Count(), "userlocations", routeValues, Request)));

            //        if (asObject)
            //        {
            //            return Ok(JsonHelper.listToObject(ConversionFactory.QueryUserLocationToViewModel(locations), objPropName));
            //        }
            //        else
            //        {
            //            return Ok(ConversionFactory.QueryUserLocationToViewModel(locations));
            //        }
            //    }


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
                return Ok(ControllerHelper.get<LocationViewModel>(userLocation, fields));
            }
            else
            {
                return BadRequest();
            }
        }

        [Route("users")]
        public IHttpActionResult Post(User user)
        {
            if (!ModelState.IsValid)
                return BadRequest("Modelstate is invalid");

            try
            {
                return Created("api/users", (User)ControllerHelper.post<User>(user));
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        public IHttpActionResult Patch(User user)
        {
            return InternalServerError();
        }
    }
}
