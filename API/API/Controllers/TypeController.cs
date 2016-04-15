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
        LocationRepository<Models.EF.Type> typeRepo = new LocationRepository<Models.EF.Type>();
        const int stdPageSize = 5;


        [Route("types", Name = "types")]
        public IHttpActionResult Get(string fields = null, string sort = "id", int? page = null, int pageSize = stdPageSize, bool asObject = true, string objPropName = "types")
        {
            IQueryable<Models.EF.Type> types = typeRepo.List();
            object toReturn = ControllerHelper<TypeViewModel>.get(types, HttpContext.Current, Request, "types", asObject, objPropName, fields, sort, page, pageSize);

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
            var type = typeRepo.Read(id);
            if (type != null)
            {
                return Ok(ControllerHelper<TypeViewModel>.get(type, fields));

            }
            else
            {
                return BadRequest("No type found");
            }
        }
    }
}