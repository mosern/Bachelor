using Api.Classes;
using Api.Helpers;
using Api.Models.Api;
using Api.Models.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Thinktecture.IdentityModel.WebApi;

namespace Api.Controllers
{
    /// <summary>
    /// Controller that handels crud for Types
    /// 
    /// Written by: Andreas Mosvoll
    /// </summary>
    [RoutePrefix("api")]
    public class TypeController : ApiController
    {
        const int stdPageSize = 5;

        /// <summary>
        /// Gets a list of location
        /// </summary>
        /// <param name="fields">Optional. The fields to include in returned object. Returns all fields if no field is specified</param>
        /// <param name="sort">Optional, sorts accending by id if not set. The fields to sort by, use "," to serparate fields. Use "-" in fornt of field name to sort decending. Sorts accesnding by id as default</param>
        /// <param name="page">Optional. The page you want</param>
        /// <param name="pageSize">Optional, standard value is 5. The size you want your pages to be</param>
        /// <param name="asObject">Optional, standard value is true. Spesify if you want a collection or a object with the collection as a property</param>
        /// <param name="objPropName">Optional, standard value is "Types". Name of object if asObject is true</param>
        /// <returns>200 ok with processed list of all types in the database</returns>
        [Route("types", Name = "types")]
        [ResourceAuthorize("read", "type")]
        public IHttpActionResult Get(string fields = null, string sort = "id", int? page = null, int pageSize = stdPageSize, bool asObject = true, string objPropName = "types")
        {
            IQueryable<Models.EF.Type> types;
            object toReturn;
            using (var repo = new LocationRepository<Models.EF.Type>())
            {
                types = repo.List();

                toReturn = ControllerHelper.get<TypeViewModel>(types, HttpContext.Current, Request, "types", asObject, objPropName, fields, sort, page, pageSize);
            }
                

            if (toReturn != null)
            {
                return Ok(toReturn);
            }
            else
            {
                return BadRequest("No types found");
            }

        }

        /// <summary>
        /// Gets a spesific type
        /// </summary>
        /// <param name="id"> id for the type you want to get</param>
        /// <param name="fields">Optional. The fields to include in returned object. Returns all fields if no field is specified</param>
        /// <returns>200 ok with processed type object</returns>
        [Route("types/{id}")]
        [ResourceAuthorize("read", "type")]
        public IHttpActionResult Get(int id, string fields = null)
        {
            Models.EF.Type type;
            using (var repo = new LocationRepository<Models.EF.Type>())
            {
                type = repo.Read(id);

                if (type != null)
                {
                    return Ok(ControllerHelper.get<TypeViewModel>(type, fields));
                }
                else
                {
                    return BadRequest("No type found");
                }
            }
                
        }

        /// <summary>
        /// Creates new type
        /// </summary>
        /// <param name="type">TypeViewModel with info about the type to create</param>
        /// <returns>201 created with the new object</returns>
        [Route("types")]
        [ResourceAuthorize("write", "type")]
        public IHttpActionResult Post(TypeViewModel type)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            try
            {
                return Created("api/types", ControllerHelper.post<Models.EF.Type, TypeViewModel>(type));
            }
            catch
            {
                return BadRequest();
            }
        }

        /// <summary>
        /// Full update of a type
        /// </summary>
        /// <param name="type">All information about the type to be updated</param>
        /// <param name="id">Id of the type to be updated</param>
        /// <returns>200 ok</returns>
        [Route("types/{id}")]
        [ResourceAuthorize("edit", "type")]
        public IHttpActionResult Put(TypeViewModel type, int id)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    type.Id = id;
                    ControllerHelper.Put<Models.EF.Type>(type);
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
        /// Partial update of a type
        /// </summary>
        /// <param name="type">Information about the type to be updated</param>
        /// <param name="id">Id of the type to be updated</param>
        /// <returns>200 ok</returns>
        [Route("types/{id}")]
        [ResourceAuthorize("edit", "type")]
        public IHttpActionResult Patch(TypeViewModel type, int id)
        {
            if (type != null)
            {
                try
                {
                    type.Id = id;
                    ControllerHelper.Patch<Models.EF.Type>(type);
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
        /// Removes an type
        /// </summary>
        /// <param name="id">Id of the type to remove</param>
        /// <returns>200 ok</returns>
        [Route("types/{id}")]
        [ResourceAuthorize("delete", "type")]
        public IHttpActionResult Delete(int id)
        {
            try
            {
                using (var repo = new LocationRepository<Models.EF.Type>())
                {
                    Models.EF.Type type = repo.Read(id);

                    if (type != null)
                    {
                        ControllerHelper.Delete<Models.EF.Type>(id);
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