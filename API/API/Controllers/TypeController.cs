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
    public class TypeController : ApiController
    {
        const int stdPageSize = 5;


        [Route("types", Name = "types")]
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

        [Route("types/{id}")]
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

        [Route("types")]
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

        [Route("types/{id}")]
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

        [Route("types/{id}")]
        public IHttpActionResult Patch(TypeViewModel type, int id)
        {
            if (type != null)
            {
                try
                {
                    type.Id = id;
                    ControllerHelper.Patch<Location>(type);
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

        [Route("types/{id}")]
        public IHttpActionResult Delete(int id)
        {
            try
            {
                using (var repo = new LocationRepository<Location>())
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