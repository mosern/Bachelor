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
        const int stdPageSize = 5;


        [Route("coordinates", Name = "coordinates")]
        public IHttpActionResult Get(string fields = null, string sort = "id", int? page = null, int pageSize = stdPageSize, bool asObject = true, string objPropName = "coordinates")
        {
            IQueryable<Coordinate> coors;
            using (var repo = new LocationRepository<Coordinate>())
                coors = repo.List();

            object toReturn = ControllerHelper.get<CoordinateViewModel>(coors, HttpContext.Current, Request, "coordinates", asObject, objPropName, fields, sort, page, pageSize);

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
            Coordinate coor;
            using (var repo = new LocationRepository<Coordinate>())
                coor = coorRepo.Read(id);

            if (coor != null)
            {
                return Ok(ControllerHelper.get<CoordinateViewModel>(coor, fields));
            }
            else
            {
                return BadRequest("No coordinate found");
            }
        }

        //[Route("coordinates")]
        //public IHttpActionResult Post(CoordinateViewModel coor)
        //{
        //    if (!ModelState.IsValid)
        //        return BadRequest("Modelstate is invalid");

        //    try
        //    {
        //        return Created("api/coordinates", ControllerHelper.post(coor));
        //    }
        //    catch(Exception e)
        //    {
        //        return BadRequest(e.Message);
        //    }
        //}
    }
}
