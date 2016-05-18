using Api.Classes;
using Api.Factories;
using Api.Helpers;
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
using System.Web.Http.Description;
using System.Web.Http.ModelBinding;
using Thinktecture.IdentityModel.WebApi;
namespace Api.Controllers
{
    [RoutePrefix("api")]
    public class LocationController : ApiController
    {
        const int stdPageSize = 5;

        /// <summary>
        /// Gets a list of locations or search for location and/or people by string
        /// </summary>
        /// <param name="fields">Optional. The fields to include in returned object. Returns all fields if no field is specified</param>
        /// <param name="sort">Optional, sorts accending by id if not set. The fields to sort by, use "," to serparate fields. Use "-" in fornt of field name to sort decending. Sorts accesnding by id as default</param>
        /// <param name="page">Optional. The page you want</param>
        /// <param name="pageSize">Optional, standard value is 5. The size you want your pages to be</param>
        /// <param name="asObject">Optional, standard value is true. Spesify if you want a collection or a object with the collection as a property</param>
        /// <param name="objPropName">Optional, standard value is "locations". Name of object if asObject is true</param>
        /// <param name="search">Optional. search for location and/or people with a string</param>
        /// <returns>200 ok with processed list of all locations in the database</returns>
        [Route("locations", Name = "locations")]
        [ResourceAuthorize("read", "location")]
        public IHttpActionResult Get(string fields = null, string sort = "id", int? page = null, int pageSize = stdPageSize, bool asObject = true, string objPropName = "locations", string search = null)
        {
            var mapper = AutoMapConfig.getMapper();
            string name = objPropName;
            SearchViewModel result = new SearchViewModel();
            IQueryable<Location> locations;
            IQueryable<People> people;

            if(search != null)
            {
                name = "results";
                result = Search.Location(search, User);
                locations = mapper.Map<IEnumerable<LocationViewModel>, IEnumerable<Location>>(result.LocationViewModel.AsEnumerable()).AsQueryable();
                people = mapper.Map<IEnumerable<PeopleViewModel>, IEnumerable<People>>(result.PeopleViewModel.AsEnumerable()).AsQueryable();
            }
            else
            {
                using (var repo = new LocationRepository<Location>())
                    locations = repo.List().ToList().AsQueryable();

                people = new List<People>().AsQueryable();
            }

            if (locations.Any() || people.Any())
            {
                if (search != null)
                {
                    List<object> toReturn = new List<object>();

                    toReturn.Add(ControllerHelper.get<LocationViewModel>(locations, HttpContext.Current, Request, "locations", false, name, fields, sort, page, pageSize));
                    toReturn.Add(ControllerHelper.get<PeopleViewModel>(people, HttpContext.Current, Request, "people", false, name, fields, sort, page, pageSize));

                    if (asObject)
                    {
                        return Ok(JsonHelper.listToObject(toReturn, name));
                    }
                    else
                    {
                        return Ok(toReturn);
                    }
                }
                else
                {
                    return Ok(ControllerHelper.get<LocationViewModel>(locations, HttpContext.Current, Request, "locations", asObject, name, fields, sort, page, pageSize));
                }
                
            }
            else
            {
                return BadRequest("No locations found");
            }
        }

        /// <summary>
        /// Gets a spesific location
        /// </summary>
        /// <param name="id"> id for the location you want to get</param>
        /// <param name="fields">Optional. The fields to include in returned object. Returns all fields if no field is specified</param>
        /// <returns>200 ok with processed location object</returns>
        [Route("locations/{id}")]
        [ResourceAuthorize("read", "location")]
        public IHttpActionResult Get(int id, string fields = null)
        {
            Location location;

            using (var repo = new LocationRepository<Location>())
            {
                location = repo.Read(id);

                if (location != null)
                {
                    return Ok(ControllerHelper.get<LocationViewModel>(location, fields));
                }
                else
                {
                    return BadRequest("No location found");
                }
            }
        }

        /// <summary>
        /// Creates new location
        /// </summary>
        /// <param name="location">locationViewModel with info about the location to create</param>
        /// <returns>201 created with the new object</returns>
        [Route("locations")]
        [ResourceAuthorize("write", "location")]
        public IHttpActionResult Post(LocationViewModel location)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            try
            {
                return Created("api/locations", ControllerHelper.post<Location, LocationViewModel>(location));
            }
            catch
            {
                return BadRequest();
            }
        }

        /// <summary>
        /// Full update of a location
        /// </summary>
        /// <param name="loc">All information about the location to be updated</param>
        /// <param name="id">Id of the location to be updated</param>
        /// <returns>200 ok</returns>
        [Route("locations/{id}")]
        [ResourceAuthorize("edit", "location")]
        public IHttpActionResult Put(LocationViewModel loc, int id)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    loc.Id = id;
                    ControllerHelper.Put<Location>(loc);
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
        /// Partial update of a location
        /// </summary>
        /// <param name="loc">Information about the location to be updated</param>
        /// <param name="id">Id of the location to be updated</param>
        /// <returns>200 ok</returns>
        [Route("locations/{id}")]
        [ResourceAuthorize("edit", "location")]
        public IHttpActionResult Patch(LocationViewModel  loc, int id)
        {
            if (loc != null)
            {
                try
                {
                    loc.Id = id;
                    ControllerHelper.Patch<Location>(loc);
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
        /// Removes an location
        /// </summary>
        /// <param name="id">Id of the location to remove</param>
        /// <returns>200 ok</returns>
        [Route("locations/{id}")]
        [ResourceAuthorize("delete", "location")]
        public IHttpActionResult Delete(int id)
        {
            try
            {             
                using(var repo = new LocationRepository<Location>())
                {
                    Location location = repo.Read(id);

                    if (location != null)
                    {
                        ControllerHelper.Delete<Location>(id);
                        return Ok();
                    }
                    else
                    {
                        return BadRequest();
                    }
                }
            }
            catch
            {
                return InternalServerError();
            }
        }
    }
}
