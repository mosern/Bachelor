using Api.Classes;
using Api.Helpers;
using Api.Models.Api;
using Api.Models.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Thinktecture.IdentityModel.WebApi;

namespace Api.Controllers
{
    [RoutePrefix("api")]
    public class PeopleController : ApiController
    {
        const int stdPageSize = 5;

        /// <summary>
        /// Gets a list of People
        /// </summary>
        /// <param name="fields">Optional. The fields to include in returned object. Returns all fields if no field is specified</param>
        /// <param name="sort">Optional, sorts accending by id if not set. The fields to sort by, use "," to serparate fields. Use "-" in fornt of field name to sort decending. Sorts accesnding by id as default</param>
        /// <param name="page">Optional. The page you want</param>
        /// <param name="pageSize">Optional, standard value is 5. The size you want your pages to be</param>
        /// <param name="asObject">Optional, standard value is true. Spesify if you want a collection or a object with the collection as a property</param>
        /// <param name="objPropName">Optional, standard value is "people". Name of object if asObject is true</param>
        /// <returns>200 ok with processed list of all people in the database</returns>
        [Route("people", Name = "people")]
        [ResourceAuthorize("read", "people")]
        public IHttpActionResult Get(string fields = null, string sort = "id", int? page = null, int pageSize = stdPageSize, bool asObject = true, string objPropName = "people")
        {
            IQueryable<People> people;
            object toReturn;
            using (var repo = new LocationRepository<People>())
            {
                people = repo.List();

                toReturn = ControllerHelper.get<PeopleViewModel>(people, HttpContext.Current, Request, "people", asObject, objPropName, fields, sort, page, pageSize);
            }

            if (toReturn != null)
            {
                return Ok(toReturn);
            }
            else
            {
                return BadRequest("No people found");
            }

        }

        /// <summary>
        /// Gets a spesific people
        /// </summary>
        /// <param name="id"> id for the neighbour you want to get</param>
        /// <param name="fields">Optional. The fields to include in returned object. Returns all fields if no field is specified</param>
        /// <returns>200 ok with processed people object</returns>
        [Route("people/{id}")]
        [ResourceAuthorize("read", "people")]
        public IHttpActionResult Get(int id, string fields = null)
        {
            People people;
            using (var repo = new LocationRepository<People>())
            {
                people = repo.Read(id);

                if (people != null)
                {
                    return Ok(ControllerHelper.get<PeopleViewModel>(people, fields));
                }
                else
                {
                    return BadRequest("No neighbour found");
                }
            }

        }

        /// <summary>
        /// Creates new people
        /// </summary>
        /// <param name="people">peopleViewModel with info about the people to create</param>
        /// <returns>201 created with the new object</returns>
        [Route("people")]
        [ResourceAuthorize("write", "people")]
        public IHttpActionResult Post(PeopleViewModel people)
        {
            if (people.Id == null)
                people.Id = 0;

            var peopleError = ModelState.Keys.Where(k => k.Contains("people.Location.")).ToList();
            
            foreach(string error in peopleError)
            {
                ModelState.Remove(error);
            }

            if (!ModelState.IsValid)
                return BadRequest();

            try
            {
                return Created("api/neighbours", ControllerHelper.post<People, PeopleViewModel>(people));
            }
            catch
            {
                return BadRequest();
            }
        }

        /// <summary>
        /// Full update of a people
        /// </summary>
        /// <param name="people">All information about the people to be updated</param>
        /// <param name="id">Id of the people to be updated</param>
        /// <returns>200 ok</returns>
        [Route("people/{id}")]
        [ResourceAuthorize("edit", "people")]
        public IHttpActionResult Put(PeopleViewModel people, int id)
        {
            if (people.Id == null)
                people.Id = 0;

            var peopleError = ModelState.Keys.Where(k => k.Contains("people.Location.")).ToList();

            foreach (string error in peopleError)
            {
                ModelState.Remove(error);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    people.Id = id;
                    ControllerHelper.Put<People>(people);
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
        /// Partial update of a people
        /// </summary>
        /// <param name="people">Information about the people to be updated</param>
        /// <param name="id">Id of the people to be updated</param>
        /// <returns>200 ok</returns>
        [Route("people/{id}")]
        [ResourceAuthorize("edit", "people")]
        public IHttpActionResult Patch(PeopleViewModel people, int id)
        {
            if (people.Id == null)
                people.Id = 0;

            if (people != null)
            {
                try
                {
                    people.Id = id;
                    ControllerHelper.Patch<People>(people);
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
        /// Removes an people
        /// </summary>
        /// <param name="id">Id of the people to remove</param>
        /// <returns>200 ok</returns>
        [Route("people/{id}")]
        [ResourceAuthorize("delete", "people")]
        public IHttpActionResult Delete(int id)
        {
            try
            {
                using (var repo = new LocationRepository<People>())
                {
                    People people = repo.Read(id);

                    if (people != null)
                    {
                        ControllerHelper.Delete<People>(id);
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
