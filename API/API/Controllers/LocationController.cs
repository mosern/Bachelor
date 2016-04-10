using Api.Classes;
using Api.Models.Api;
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
    public class LocationController : ApiController
    {
        const int stdPageSize = 5;

        [Route("locations", Name = "locations")]
        public IHttpActionResult Get(string fields = null, string sort = "id", int page = 1, int pageSize = stdPageSize, bool asObject = true, string objPropName = "locations", string search = null)
        {
            if(search == null)
            {
                return BadRequest();
            }

            var locations = Search.Location(search);

            if (locations != null)
            {
                IDictionary<string, object> routeValues = new Dictionary<string, object>();

                routeValues.Add("fields", fields);
                routeValues.Add("sort", fields);

                HttpContext.Current.Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(PaginationHeader.Get(page, pageSize, locations.Count(), "locations", routeValues)));

                if (fields != null)
                {
                    if (asObject)
                    {
                        var toReturn = LocationInfo.ShapeList(locations, fields.ToLower().Split(',').ToList());
                        return Ok(JsonHelper.listToObject(toReturn, objPropName));
                    }
                    else
                    {
                        return Ok(LocationInfo.ShapeList(locations, fields.ToLower().Split(',').ToList()));
                    }
                }
                else
                {
                    if (asObject)
                    {
                        return Ok(JsonHelper.listToObject(LocationInfo.List(locations), objPropName));
                    }
                    else
                    {
                        return Ok(LocationInfo.List(locations));
                    }
                }
            }
            else
            {
                return Ok("No locations found");
            }
        }
    }
}
