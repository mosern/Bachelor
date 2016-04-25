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
    /// <summary>
    /// Controller that handels crud for Users
    /// </summary>
    [RoutePrefix("api")]
    public class UserController : ApiController
    {
        const int stdPageSize = 5;

        /// <summary>
        /// Gets a list of users
        /// </summary>
        /// <param name="fields">Optional. The fields to include in returned object. Returns all fields if no field is specified</param>
        /// <param name="sort">Optional, sorts accending by id if not set. The fields to sort by, use "," to serparate fields. Use "-" in fornt of field name to sort decending. Sorts accesnding by id as default</param>
        /// <param name="page">Optional. The page you want</param>
        /// <param name="pageSize">Optional, standard value is 5. The size you want your pages to be</param>
        /// <param name="asObject">Optional, standard value is true. Spesify if you want a collection or a object with the collection as a property</param>
        /// <param name="objPropName">Optional, standard value is "users". Name of object if asObject is true</param>
        /// <returns>200 ok with processed list of all users in the database</returns>
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

        /// <summary>
        /// Gets a spesific user
        /// </summary>
        /// <param name="id"> id for the accesspoint you want to get</param>
        /// <param name="fields">Optional. The fields to include in returned object. Returns all fields if no field is specified</param>
        /// <returns>200 ok with processed user object</returns>
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

        /// <summary>
        /// Gets a list of locations, the spesified user has visited
        /// </summary>
        /// /// <param name="id"> id for the user</param>
        /// <param name="fields">Optional. The fields to include in returned object. Returns all fields if no field is specified</param>
        /// <param name="sort">Optional, sorts accending by id if not set. The fields to sort by, use "," to serparate fields. Use "-" in fornt of field name to sort decending. Sorts accesnding by id as default</param>
        /// <param name="page">Optional. The page you want</param>
        /// <param name="pageSize">Optional, standard value is 5. The size you want your pages to be</param>
        /// <param name="asObject">Optional, standard value is true. Spesify if you want a collection or a object with the collection as a property</param>
        /// <param name="objPropName">Optional, standard value is "userLocations". Name of object if asObject is true</param>
        /// <returns>200 ok with processed list of all locations in the database, that the spesified user has visited. Hits field in these location objects is spesific to the user</returns>
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

        /// <summary>
        /// Gets a spesific location, the spesified user has visited
        /// </summary>
        /// <param name="id"> id for the user</param>
        /// /// <param name="locId"> id for the location you want to get</param>
        /// <param name="fields">Optional. The fields to include in returned object. Returns all fields if no field is specified</param>
        /// <returns>200 ok with processed location object</returns>
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

        /// <summary>
        /// Creates new user
        /// </summary>
        /// <param name="user">UserViewModel with info about the user to create</param>
        /// <returns>201 created with the new object</returns>
        [Route("users")]
        public IHttpActionResult Post(User user)
        {
            //TODO update user to use viewmodel
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

        /// <summary>
        /// Full update of a user
        /// </summary>
        /// <param name="user">All information about the user to be updated</param>
        /// <param name="id">Id of the user to be updated</param>
        /// <returns>200 ok</returns>
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

        /// <summary>
        /// Partial update of a user
        /// </summary>
        /// <param name="user">Information about the user to be updated</param>
        /// <param name="id">Id of the user to be updated</param>
        /// <returns>200 ok</returns>
        [Route("users/{id}")]
        public IHttpActionResult Patch(User user, int id)
        {
            if (user != null)
            {
                try
                {
                    //TODO Patch for Users has to be implemented
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
        /// <summary>
        /// Removes an user
        /// </summary>
        /// <param name="id">Id of the user to remove</param>
        /// <returns>200 ok</returns>
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
