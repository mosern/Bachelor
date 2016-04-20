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

namespace Api.Controllers
{
    [RoutePrefix("api")]
    public class AccesspointController : ApiController
    {
        const int stdPageSize = 5;


        [Route("accesspoints", Name = "accesspoints")]
        public IHttpActionResult Get(string fields = null, string sort = "id", int? page = null, int pageSize = stdPageSize, bool asObject = true, string objPropName = "accesspoints")
        {
            IQueryable<Accesspoint> accs;
            using (var repo = new LocationRepository<Accesspoint>())
                accs = repo.List();

            object toReturn = ControllerHelper.get<AccesspointViewModel>(accs, HttpContext.Current, Request, "accesspoints", asObject, objPropName, fields, sort, page, pageSize);

            if (toReturn != null)
            {
                return Ok(toReturn);
            }
            else
            {
                return BadRequest("No accesspoints found");
            }

        }

        [Route("accesspoints/{id}")]
        public IHttpActionResult Get(int id, string fields = null)
        {
            Accesspoint acc;
            using (var repo = new LocationRepository<Accesspoint>())
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

        //[Route("accesspoints")]
        //public IHttpActionResult Post(AccesspointViewModel accesspoint)
        //{
        //    if (!ModelState.IsValid)
        //        return BadRequest();

        //    try
        //    {
        //        return Created("api/accesspoints", ControllerHelper.post(accesspoint));
        //    }
        //    catch
        //    {
        //        return BadRequest();
        //    }
        //}
    }
}
