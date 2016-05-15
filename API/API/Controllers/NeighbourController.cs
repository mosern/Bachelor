using Api.Classes;
using Api.Helpers;
using Api.Models.Api;
using Api.Models.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace Api.Controllers
{
    [RoutePrefix("api")]
    public class NeighbourController : ApiController
    {
        const int stdPageSize = 5;

        /// <summary>
        /// Gets a list of Neighbours
        /// </summary>
        /// <param name="fields">Optional. The fields to include in returned object. Returns all fields if no field is specified</param>
        /// <param name="sort">Optional, sorts accending by id if not set. The fields to sort by, use "," to serparate fields. Use "-" in fornt of field name to sort decending. Sorts accesnding by id as default</param>
        /// <param name="page">Optional. The page you want</param>
        /// <param name="pageSize">Optional, standard value is 5. The size you want your pages to be</param>
        /// <param name="asObject">Optional, standard value is true. Spesify if you want a collection or a object with the collection as a property</param>
        /// <param name="objPropName">Optional, standard value is "neighbours". Name of object if asObject is true</param>
        /// <returns>200 ok with processed list of all neighbours in the database</returns>
        [Route("neighbours", Name = "neighbour")]
        public IHttpActionResult Get(string fields = null, string sort = "id", int? page = null, int pageSize = stdPageSize, bool asObject = true, string objPropName = "neighbours")
        {
            IQueryable<PathNeighbour> pathPoints;
            object toReturn;
            using (var repo = new LocationRepository<PathNeighbour>())
            {
                pathPoints = repo.List();

                toReturn = ControllerHelper.get<NeighbourViewModel>(pathPoints, HttpContext.Current, Request, "neighbours", asObject, objPropName, fields, sort, page, pageSize);
            }

            if (toReturn != null)
            {
                return Ok(toReturn);
            }
            else
            {
                return BadRequest("No neighbours found");
            }

        }

        /// <summary>
        /// Gets a spesific neighbour
        /// </summary>
        /// <param name="id"> id for the neighbour you want to get</param>
        /// <param name="fields">Optional. The fields to include in returned object. Returns all fields if no field is specified</param>
        /// <returns>200 ok with processed neighbour object</returns>
        [Route("neighbours/{id}")]
        public IHttpActionResult Get(int id, string fields = null)
        {
            PathNeighbour pathPoint;
            using (var repo = new LocationRepository<PathNeighbour>())
            {
                pathPoint = repo.Read(id);

                if (pathPoint != null)
                {
                    return Ok(ControllerHelper.get<NeighbourViewModel>(pathPoint, fields));
                }
                else
                {
                    return BadRequest("No neighbour found");
                }
            }

        }

        /// <summary>
        /// Creates new neighbour
        /// </summary>
        /// <param name="neighbour">neighbourViewModel with info about the neighbour to create</param>
        /// <returns>201 created with the new object</returns>
        [Route("neighbours")]
        public IHttpActionResult Post(NeighbourViewModel neighbour)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            try
            {
                var created = ControllerHelper.post<PathNeighbour, NeighbourViewModel>(neighbour);

                using (var repo = new LocationRepository<PathPoint>())
                {
                    var point1 = repo.Read(neighbour.pathPoint1.Id.Value);
                    var point2 = repo.Read(neighbour.pathPoint2.Id.Value);

                    point1.NeighbourCount += 1;
                    point2.NeighbourCount += 1;

                    repo.Update(point1);
                    repo.Update(point2);
                }

                return Created("api/neighbours", created);
            }
            catch
            {
                return BadRequest();
            }
        }

        /// <summary>
        /// Full update of a neighbour
        /// </summary>
        /// <param name="neighbour">All information about the neighbour to be updated</param>
        /// <param name="id">Id of the neighbour to be updated</param>
        /// <returns>200 ok</returns>
        [Route("neighbours/{id}")]
        public IHttpActionResult Put(NeighbourViewModel neighbour, int id)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    neighbour.Id = id;
                    ControllerHelper.Put<PathNeighbour>(neighbour);
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
        /// Partial update of a neighbour
        /// </summary>
        /// <param name="neighbour">Information about the neighbour to be updated</param>
        /// <param name="id">Id of the neighbour to be updated</param>
        /// <returns>200 ok</returns>
        [Route("neighbours/{id}")]
        public IHttpActionResult Patch(NeighbourViewModel neighbour, int id)
        {
            if (neighbour != null)
            {
                try
                {
                    neighbour.Id = id;
                    ControllerHelper.Patch<PathNeighbour>(neighbour);
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
        /// Removes an neighbour
        /// </summary>
        /// <param name="id">Id of the neighbour to remove</param>
        /// <returns>200 ok</returns>
        [Route("neighbours/{id}")]
        public IHttpActionResult Delete(int id)
        {
            try
            {
                using (var repo = new LocationRepository<PathNeighbour>())
                {
                    PathNeighbour pathPoint = repo.Read(id);

                    if (pathPoint != null)
                    {
                        ControllerHelper.Delete<PathNeighbour>(id);

                        using (var pointRepo = new LocationRepository<PathPoint>())
                        {
                            var point1 = pointRepo.Read(pathPoint.PathPoint1.Id);
                            var point2 = pointRepo.Read(pathPoint.PathPoint2.Id);

                            point1.NeighbourCount -= 1;
                            point2.NeighbourCount -= 1;

                            pointRepo.Update(point1);
                            pointRepo.Update(point2);
                        }

                        return Ok();
                    }
                    else
                    {
                        return BadRequest();
                    }
                }
            }
            catch(Exception e)
            {
                return InternalServerError(e);
            }
        }
    }
}