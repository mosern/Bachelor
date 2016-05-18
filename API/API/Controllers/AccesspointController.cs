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
    /// <summary>
    /// Controller that handels crud for Accesspoints
    /// 
    /// Written by: Andreas Mosvoll
    /// </summary>
    [RoutePrefix("api")]
    public class AccesspointController : ApiController
    {
        const int stdPageSize = 5;

        /// <summary>
        /// Gets a list of accesspoints
        /// </summary>
        /// <param name="fields">Optional. The fields to include in returned object. Returns all fields if no field is specified</param>
        /// <param name="sort">Optional, sorts accending by id if not set. The fields to sort by, use "," to serparate fields. Use "-" in fornt of field name to sort decending. Sorts accesnding by id as default</param>
        /// <param name="page">Optional. The page you want</param>
        /// <param name="pageSize">Optional, standard value is 5. The size you want your pages to be</param>
        /// <param name="asObject">Optional, standard value is true. Spesify if you want a collection or a object with the collection as a property</param>
        /// <param name="objPropName">Optional, standard value is "accesspoints". Name of object if asObject is true</param>
        /// <returns>200 ok with processed list of all accesspoints in the database</returns>
        [Route("accesspoints", Name = "accesspoints")]
        [ResourceAuthorize("read", "accesspoint")]
        public IHttpActionResult Get(string fields = null, string sort = "id", int? page = null, int pageSize = stdPageSize, bool asObject = true, string objPropName = "accesspoints")
        {
            IQueryable<Accesspoint> accs;
            object toReturn;
            using (var repo = new LocationRepository<Accesspoint>())
            {
                accs = repo.List();

                toReturn = ControllerHelper.get<AccesspointViewModel>(accs, HttpContext.Current, Request, "accesspoints", asObject, objPropName, fields, sort, page, pageSize);
            }

            if (toReturn != null)
            {
                return Ok(toReturn);
            }
            else
            {
                return BadRequest("No accesspoints found");
            }
        }

        /// <summary>
        /// Gets a spesific accesspoint
        /// </summary>
        /// <param name="id"> id for the accesspoint you want to get</param>
        /// <param name="fields">Optional. The fields to include in returned object. Returns all fields if no field is specified</param>
        /// <returns>200 ok with processed accesspoint object</returns>
        [Route("accesspoints/{id}")]
        [ResourceAuthorize("read", "accesspoint")]
        public IHttpActionResult Get(int id, string fields = null)
        {
            Accesspoint acc;
            using (var repo = new LocationRepository<Accesspoint>())
            {
                acc = repo.Read(id);

                if (acc != null)
                {
                    return Ok(ControllerHelper.get<AccesspointViewModel>(acc, fields));

                }
                else
                {
                    return BadRequest("No accesspoint found");
                }
            }
                
        }

        /// <summary>
        /// Creates new accesspont
        /// </summary>
        /// <param name="acc">AccesspointViewModel with info about the accesspoint to create</param>
        /// <returns>201 created with the new object</returns>
        [Route("accesspoints")]
        [ResourceAuthorize("write", "accesspoint")]
        public IHttpActionResult Post(AccesspointViewModel acc)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            try
            {
                return Created("api/accesspoints", ControllerHelper.post<Accesspoint, AccesspointViewModel>(acc));
            }
            catch
            {
                return BadRequest();
            }
        }

        /// <summary>
        /// Full update of a accesspoint
        /// </summary>
        /// <param name="acc">All information about the accesspoint to be updated</param>
        /// <param name="id">Id of the accesspoint to be updated</param>
        /// <returns>200 ok</returns>
        [Route("accesspoints/{id}")]
        [ResourceAuthorize("edit", "accesspoint")]
        public IHttpActionResult Put(AccesspointViewModel acc, int id)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    acc.Id = id;
                    ControllerHelper.Put<Accesspoint>(acc);
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
        /// Partial update of a accesspoint
        /// </summary>
        /// <param name="acc">Information about the accesspoint to be updated</param>
        /// <param name="id">Id of the accesspoint to be updated</param>
        /// <returns>200 ok</returns>
        [Route("accesspoints/{id}")]
        [ResourceAuthorize("edit", "accesspoint")]
        public IHttpActionResult Patch(AccesspointViewModel acc, int id)
        {
            if (acc != null)
            {
                try
                {
                    acc.Id = id;
                    ControllerHelper.Patch<Accesspoint>(acc);
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
        /// Removes an accesspoint
        /// </summary>
        /// <param name="id">Id of the accespoint to remove</param>
        /// <returns>200 ok</returns>
        [Route("accesspoints/{id}")]
        [ResourceAuthorize("delete", "accesspoint")]
        public IHttpActionResult Delete(int id)
        {
            try
            {
                using (var repo = new LocationRepository<Accesspoint>())
                {
                    Accesspoint acc = repo.Read(id);

                    if (acc != null)
                    {
                        ControllerHelper.Delete<Accesspoint>(id);
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
