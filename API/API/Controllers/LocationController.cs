using Api.Classes;
using Api.Dto;
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
using System.Web.Http.Description;
using System.Web.Http.ModelBinding;
using Thinktecture.IdentityModel.WebApi;
namespace Api.Controllers
{
    [RoutePrefix("api")]
    public class LocationController : ApiController
    {
        const int stdPageSize = 5;

        [Route("locations", Name = "locations")]
        public IHttpActionResult Get(string fields = null, string sort = "id", int? page = null, int pageSize = stdPageSize, bool asObject = true, string objPropName = "locations", string search = null)
        {
            string name = objPropName;
            SearchViewModel result = new SearchViewModel();

            if (search != null)
            {
                name = "results";
                result = Search.Location(search);
                if (page != null)
                {
                    result.LocationViewModel = result.LocationViewModel.ApplySort(sort).Skip(pageSize * (page.Value - 1)).Take(pageSize);
                    result.PeopleViewModel = result.PeopleViewModel.ApplySort(sort).Skip(pageSize * (page.Value - 1)).Take(pageSize);
                }
                else
                {
                    result.LocationViewModel = result.LocationViewModel.ApplySort(sort);
                    result.PeopleViewModel = result.PeopleViewModel.ApplySort(sort);
                }
            }
            else
            {
                if (page != null)
                {
                    using (var repo = new LocationRepository<Location>())
                        result.LocationViewModel = ConversionFactory.QueryLocationToViewModel(repo.List().ApplySort(sort).Skip(pageSize * (page.Value - 1)).Take(pageSize));
                    result.PeopleViewModel = new List<PeopleViewModel>().AsQueryable();
                }
                else
                {
                    using (var repo = new LocationRepository<Location>())
                        result.LocationViewModel = ConversionFactory.QueryLocationToViewModel(repo.List().ApplySort(sort));
                    result.PeopleViewModel = new List<PeopleViewModel>().AsQueryable();
                }
            }

            if (result.LocationViewModel.Any() || result.PeopleViewModel.Any())
            {
                IDictionary<string, object> routeValues = new Dictionary<string, object>();

                if(fields != null)
                routeValues.Add("fields", fields);

                routeValues.Add("sort", sort);

                if(page != null)
                HttpContext.Current.Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(PaginationHeader.Get(page.Value, pageSize, result.LocationViewModel.Count() + result.PeopleViewModel.Count(), "locations", routeValues, Request)));

                if(search == null)
                {
                    if (fields != null)
                    {
                        IEnumerable<object> loc = new List<object>();

                        if (asObject)
                        {
                            loc = ShapeFactory<LocationViewModel>.ShapeList(result.LocationViewModel, fields.ToLower().Split(',').ToList());

                            return Ok(JsonHelper.listToObject(loc, objPropName));
                        }
                        else
                        {
                            loc = ShapeFactory<LocationViewModel>.ShapeList(result.LocationViewModel, fields.ToLower().Split(',').ToList());

                            return Ok(loc);
                        }
                    }
                    else
                    {
                        if (asObject)
                        {
                            return Ok(JsonHelper.listToObject(result.LocationViewModel, objPropName));
                        }
                        else
                        {
                            return Ok(result.LocationViewModel);
                        }
                    }
                }
                else
                {
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
                            return Ok(JsonHelper.listToObject(ConversionFactory.SearchToQuerry(result), objPropName));
                        }
                        else
                        {
                            return Ok(ConversionFactory.SearchToQuerry(result));
                        }
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
            Location location;

            using (var repo = new LocationRepository<Location>())
                location = repo.Read(id);

            if (location != null)
            {
                if(fields != null)
                {
                    return Ok(ShapeFactory<LocationViewModel>.Shape(ConversionFactory.LocationToViewModel(location), fields.ToLower().Split(',').ToList()));
                }
                else
                {
                    return Ok(ConversionFactory.LocationToViewModel(location));
                }
                
            }
            else
            {
                return BadRequest("No location found");
            }
            
        }

        [Route("locations")]
        public IHttpActionResult Post(LocationViewModel location)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            try
            {
                return Created("api/locations", ControllerHelper.post<Location, LocationViewModel>(location));
            }
            catch
            {
                return BadRequest();
            }
        }

        [Route("locations/{id}")]
        public IHttpActionResult Put(LocationViewModel loc, int id)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    loc.Id = id;
                    ControllerHelper.Put<Location>(loc);
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

        [Route("locations/{id}")]
        public IHttpActionResult Patch(LocationViewModel  loc, int id)
        {
            if (loc != null)
            {
                try
                {
                    loc.Id = id;
                    ControllerHelper.Patch<Location>(loc);
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

        [Route("users/{id}")]
        public IHttpActionResult Delete(int id)
        {
            if (id != 0)
            {
                return Ok();
            }
            else
            {
                return InternalServerError();
            }
        }
    }
}
