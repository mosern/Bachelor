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
    [RoutePrefix("api")]
    public class CoordinateController : ApiController
    {
        LocationRepository<Coordinate> coorRepo = new LocationRepository<Coordinate>();
        const int stdPageSize = 5;


        [Route("coordinates", Name = "coordinates")]
        public IHttpActionResult Get(string fields = null, string sort = "id", int? page = null, int pageSize = stdPageSize, bool asObject = true, string objPropName = "coordinates")
        {
            IQueryable<Coordinate> coors = coorRepo.List();
            object toReturn = ControllerHelper<CoordinateViewModel>.get(coors, HttpContext.Current, Request, "coordinates", asObject, objPropName, fields, sort, page, pageSize);

            if (toReturn != null)
            {
                return Ok(toReturn);
            }
            else
            {
                return BadRequest("No coordinates found");
            }

        }

        [Route("coordinates/{id}")]
        public IHttpActionResult Get(int id, string fields = null)
        {
            var coor = coorRepo.Read(id);
            if (coor != null)
            {
                return Ok(ControllerHelper<CoordinateViewModel>.get(coor, fields));

            }
            else
            {
                return BadRequest("No coordinates found");
            }
        }
    }
}
