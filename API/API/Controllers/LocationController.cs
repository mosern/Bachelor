using Api.Classes;
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
using System.Web.Http.Description;
using Thinktecture.IdentityModel.Mvc;

namespace Api.Controllers
{
    [RoutePrefix("api")]
    public class LocationController : ApiController
    {
        static LocationRepository<Location> LocRepo = new LocationRepository<Location>();
        const int stdPageSize = 5;

        [Route("locations", Name = "locations")]
        public IHttpActionResult Get(string fields = null, string sort = "id", int page = 1, int pageSize = stdPageSize, bool asObject = true, string objPropName = "locations", string search = null)
        {
            IQueryable<Location> locations;

            if (search != null)
            {
                locations = Search.Location(search).ApplySort(sort).Skip(pageSize * (page - 1)).Take(pageSize); ;
            }
            else
            {
                locations = LocRepo.List().ApplySort(sort).Skip(pageSize * (page - 1)).Take(pageSize); ;
            }

            if (locations.Any())
            {
                IDictionary<string, object> routeValues = new Dictionary<string, object>();

                if(fields != null)
                routeValues.Add("fields", fields);

                routeValues.Add("sort", sort);

                HttpContext.Current.Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(PaginationHeader.Get(page, pageSize, locations.Count(), "locations", routeValues, Request)));

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
                return BadRequest("No locations found");
            }
        }

        [ResourceAuthorize("Read", "location")]
        [Route("locations/{id}")]
        public IHttpActionResult Get(int id, string fields = null)
        {
            var location = LocRepo.Read(id);

            if (location != null)
            {
                if(fields != null)
                {
                    return Ok(LocationInfo.Shape(location, fields.ToLower().Split(',').ToList()));
                }
                else
                {
                    return Ok(new LocationInfo(location));
                }
                
            }
            else
            {
                return BadRequest("No location found");
            }
            
        }

        [Route("locations")]
        public IHttpActionResult Post(LocationInfo location)
        {
            //if(location.CoordinateId == 0)
            //{
            //    return Ok(location);
            //}
            if (!ModelState.IsValid)
                return BadRequest();

            try {
                LocRepo.Create(LocationInfo.toLocation(location));
            }
            catch
            {
                return BadRequest();
            }
            return Ok();
        }
    }
}
