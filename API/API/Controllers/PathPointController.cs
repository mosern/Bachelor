using Api.Classes;
using Api.Helpers;
using Api.Models.Api;
using Api.Models.EF;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace Api.Controllers
{
    [RoutePrefix("api")]
    public class PathPointController : ApiController
    {
        const int stdPageSize = 5;

        /// <summary>
        /// Gets a list of PathPoints
        /// </summary>
        /// <param name="fields">Optional. The fields to include in returned object. Returns all fields if no field is specified</param>
        /// <param name="sort">Optional, sorts accending by id if not set. The fields to sort by, use "," to serparate fields. Use "-" in fornt of field name to sort decending. Sorts accesnding by id as default</param>
        /// <param name="page">Optional. The page you want</param>
        /// <param name="pageSize">Optional, standard value is 5. The size you want your pages to be</param>
        /// <param name="asObject">Optional, standard value is true. Spesify if you want a collection or a object with the collection as a property</param>
        /// <param name="objPropName">Optional, standard value is "PathPoints". Name of object if asObject is true</param>
        /// <returns>200 ok with processed list of all pathPoints in the database</returns>
        [Route("pathPoints", Name = "pathPoints")]
        public IHttpActionResult Get(string fields = null, string sort = "id", int? page = null, int pageSize = stdPageSize, bool asObject = true, string objPropName = "pathPoints")
        {
            IQueryable<PathPoint> pathPoints;
            object toReturn;
            using (var repo = new LocationRepository<PathPoint>())
            {
                var temp = repo.List().ToList();
                pathPoints = repo.List().Include(p => p.Coordinate).AsQueryable();

                toReturn = ControllerHelper.get<PathPointNeighbourViewModel>(pathPoints, HttpContext.Current, Request, "pathPoints", asObject, objPropName, fields, sort, page, pageSize);
            }
            if (toReturn != null)
            {
                return Ok(toReturn);
            }
            else
            {
                return BadRequest("No pathPoints found");
            }

        }

        /// <summary>
        /// Gets a spesific pathPoint
        /// </summary>
        /// <param name="id"> id for the pathPoint you want to get</param>
        /// <param name="fields">Optional. The fields to include in returned object. Returns all fields if no field is specified</param>
        /// <returns>200 ok with processed pathPoint object</returns>
        [Route("pathPoints/{id}")]
        public IHttpActionResult Get(int id, string fields = null)
        {
            PathPoint pathPoint;
            using (var repo = new LocationRepository<PathPoint>())
            {
                pathPoint = repo.Read(id);

                if (pathPoint != null)
                {
                    return Ok(ControllerHelper.get<PathPointNeighbourViewModel>(pathPoint, fields));
                }
                else
                {
                    return BadRequest("No pathPoint found");
                }
            }

        }

        /// <summary>
        /// Creates new pathPoint
        /// </summary>
        /// <param name="pathPoint">pathPointViewModel with info about the pathPoint to create</param>
        /// <returns>201 created with the new object</returns>
        [Route("pathPoints")]
        public IHttpActionResult Post(PathPointViewModel pathPoint)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            try
            {
                return Created("api/pathPoints", ControllerHelper.post<PathPoint, PathPointViewModel>(pathPoint));
            }
            catch
            {
                return BadRequest();
            }
        }

        /// <summary>
        /// Full update of a pathPoint
        /// </summary>
        /// <param name="pathPoint">All information about the pathPoint to be updated</param>
        /// <param name="id">Id of the pathPoint to be updated</param>
        /// <returns>200 ok</returns>
        [Route("pathPoints/{id}")]
        public IHttpActionResult Put(PathPointViewModel pathPoint, int id)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    pathPoint.Id = id;
                    ControllerHelper.Put<PathPoint>(pathPoint);
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
        /// Partial update of a pathPoint
        /// </summary>
        /// <param name="pathPoint">Information about the pathPoint to be updated</param>
        /// <param name="id">Id of the pathPoint to be updated</param>
        /// <returns>200 ok</returns>
        [Route("pathPoints/{id}")]
        public IHttpActionResult Patch(PathPointViewModel pathPoint, int id)
        {
            if (pathPoint != null)
            {
                try
                {
                    pathPoint.Id = id;
                    ControllerHelper.Patch<PathPoint>(pathPoint);
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
        /// Removes an pathPoint
        /// </summary>
        /// <param name="id">Id of the pathPoint to remove</param>
        /// <returns>200 ok</returns>
        [Route("pathpoints/{id}")]
        public IHttpActionResult Delete(int id)
        {
            try
            {
                using (var repo = new LocationRepository<PathPoint>())
                {
                    PathPoint pathPoint = repo.Read(id);

                    if (pathPoint != null)
                    {
                        ControllerHelper.Delete<PathPoint>(id);
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
