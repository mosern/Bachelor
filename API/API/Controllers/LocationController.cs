using Api.Classes;
using Api.Factories;
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
using Thinktecture.IdentityModel.WebApi;
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
            string name = objPropName;
            SearchViewModel result = new SearchViewModel();

            if (search != null)
            {
                name = "results";
                result = Search.Location(search);
                result.LocationViewModel = result.LocationViewModel.ApplySort(sort).Skip(pageSize * (page - 1)).Take(pageSize);
                result.PeopleViewModel = result.PeopleViewModel.ApplySort(sort).Skip(pageSize * (page - 1)).Take(pageSize);
            }
            else
            {
                result.LocationViewModel = ConversionFactory.queryLocationToViewModel(LocRepo.List().ApplySort(sort).Skip(pageSize * (page - 1)).Take(pageSize));
                result.PeopleViewModel = new List<PeopleViewModel>().AsQueryable();
            }

            if (result.LocationViewModel.Any() || result.PeopleViewModel.Any())
            {
                IDictionary<string, object> routeValues = new Dictionary<string, object>();

                if(fields != null)
                routeValues.Add("fields", fields);

                routeValues.Add("sort", sort);

                HttpContext.Current.Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(PaginationHeader.Get(page, pageSize, result.LocationViewModel.Count() + result.PeopleViewModel.Count(), "locations", routeValues, Request)));

                if (fields != null)
                {
                    IEnumerable<object> loc = new List<object>();
                    IEnumerable<object> peo = new List<object>();

                    if (asObject)
                    {
                        loc = ShapeFactory<LocationViewModel>.ShapeList(result.LocationViewModel, fields.ToLower().Split(',').ToList());
                        peo = ShapeFactory<PeopleViewModel>.ShapeList(result.PeopleViewModel, fields.ToLower().Split(',').ToList());

                        List<object> toReturn = new List<object>();

                        if (peo.Any())
                            toReturn.Add(peo);

                        toReturn.Add(loc);

                        return Ok(JsonHelper.listToObject(toReturn, objPropName));
                    }
                    else
                    {
                        loc = ShapeFactory<LocationViewModel>.ShapeList(result.LocationViewModel, fields.ToLower().Split(',').ToList());
                        peo = ShapeFactory<LocationViewModel>.ShapeList(result.LocationViewModel, fields.ToLower().Split(',').ToList());

                        List<object> toReturn = new List<object>();

                        if (peo.Any())
                            toReturn.Add(peo);

                        toReturn.Add(loc);

                        return Ok(toReturn);
                    }
                }
                else
                {
                    if (asObject)
                    {
                        return Ok(JsonHelper.listToObject(ConversionFactory.searchToQuerry(result), objPropName));
                    }
                    else
                    {
                        return Ok(ConversionFactory.searchToQuerry(result));
                    }
                }
            }
            else
            {
                return BadRequest("No locations found");
            }
        }

        
        [Route("locations/{id}")]
        [ResourceAuthorize("Read", "location")]
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
