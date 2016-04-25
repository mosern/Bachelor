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

namespace Api.Controllers
{
    /// <summary>
    /// Controller that handels crud for Coordinates
    /// </summary>
    [RoutePrefix("api")]
    public class CoordinateController : ApiController
    {
        const int stdPageSize = 5;

        /// <summary>
        /// Gets a list of coordinates
        /// </summary>
        /// <param name="fields">Optional. The fields to include in returned object. Returns all fields if no field is specified</param>
        /// <param name="sort">Optional, sorts accending by id if not set. The fields to sort by, use "," to serparate fields. Use "-" in fornt of field name to sort decending. Sorts accesnding by id as default</param>
        /// <param name="page">Optional. The page you want</param>
        /// <param name="pageSize">Optional, standard value is 5. The size you want your pages to be</param>
        /// <param name="asObject">Optional, standard value is true. Spesify if you want a collection or a object with the collection as a property</param>
        /// <param name="objPropName">Optional, standard value is "coordiantes". Name of object if asObject is true</param>
        /// <returns>200 ok with processed list of all coordinatess in the database</returns>
        [Route("coordinates", Name = "coordinates")]
        public IHttpActionResult Get(string fields = null, string sort = "id", int? page = null, int pageSize = stdPageSize, bool asObject = true, string objPropName = "coordinates")
        {
            IQueryable<Coordinate> coors;
            object toReturn;
            using (var repo = new LocationRepository<Coordinate>())
            {
                coors = repo.List();

                toReturn = ControllerHelper.get<CoordinateViewModel>(coors, HttpContext.Current, Request, "coordinates", asObject, objPropName, fields, sort, page, pageSize);
            }

            if (toReturn != null)
            {
                return Ok(toReturn);
            }
            else
            {
                return BadRequest("No coordinates found");
            }

        }

        /// <summary>
        /// Gets a spesific coordinates
        /// </summary>
        /// <param name="id"> id for the accesspoint you want to get</param>
        /// <param name="fields">Optional. The fields to include in returned object. Returns all fields if no field is specified</param>
        /// <returns>200 ok with processed coordinates object</returns>
        [Route("coordinates/{id}")]
        public IHttpActionResult Get(int id, string fields = null)
        {
            Coordinate coor;
            using (var repo = new LocationRepository<Coordinate>())
            {
                coor = repo.Read(id);

                if (coor != null)
                {
                    return Ok(ControllerHelper.get<CoordinateViewModel>(coor, fields));
                }
                else
                {
                    return BadRequest("No coordinate found");
                }
            }


        }

        /// <summary>
        /// Creates new coordinates
        /// </summary>
        /// <param name="cor">CoordinateViewModel with info about the coordnate to create</param>
        /// <returns>201 created with the new object</returns>
        [Route("coordinates")]
        public IHttpActionResult Post(CoordinateViewModel cor)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            try
            {
                return Created("api/locations", ControllerHelper.post<Coordinate, CoordinateViewModel>(cor));
            }
            catch
            {
                return BadRequest();
            }
        }

        /// <summary>
        /// Full update of a coordinate
        /// </summary>
        /// <param name="cor">All information about the coordinate to be updated</param>
        /// <param name="id">Id of the coordinate to be updated</param>
        /// <returns>200 ok</returns>
        [Route("coordinates/{id}")]
        public IHttpActionResult Put(CoordinateViewModel cor, int id)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    cor.Id = id;
                    ControllerHelper.Put<Coordinate>(cor);
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
        /// Partial update of a coordinate
        /// </summary>
        /// <param name="cor">Information about the coodinate to be updated</param>
        /// <param name="id">Id of the coordiante to be updated</param>
        /// <returns>200 ok</returns>
        [Route("coordinates/{id}")]
        public IHttpActionResult Patch(CoordinateViewModel cor, int id)
        {
            if (cor != null)
            {
                try
                {
                    cor.Id = id;
                    ControllerHelper.Patch<Coordinate>(cor);
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
        /// Removes an coordinate
        /// </summary>
        /// <param name="id">Id of the coordinate to remove</param>
        /// <returns>200 ok</returns>
        [Route("coordinates/{id}")]
        public IHttpActionResult Delete(int id)
        {
            try
            {
                using (var repo = new LocationRepository<Coordinate>())
                {
                    Coordinate cor = repo.Read(id);

                    if (cor != null)
                    {
                        ControllerHelper.Delete<Coordinate>(id);
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
