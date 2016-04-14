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

namespace Api.Controllers
{
    [RoutePrefix("api")]
    public class CoordinateController : ApiController
    {
        //LocationRepository<Coordinate> coorRepo = new LocationRepository<Coordinate>();


        //[Route("coordinates")]
        //public IHttpActionResult Get(string fields = null, string sort = "id", int? page = null, int pageSize = stdPageSize, bool asObject = true, string objPropName = "users")
        //{
        //    IQueryable<Coordinate> coors;
        //    if (page != null)
        //    {
        //        coors = coorRepo.List().ApplySort(sort).Skip(pageSize * (page.Value - 1)).Take(pageSize);
        //    }
        //    else
        //    {
        //        coors = coorRepo.List().ApplySort(sort);
        //    }

        //    if (coors != null)
        //    {
        //        IDictionary<string, object> routeValues = new Dictionary<string, object>();

        //        if (fields != null)
        //            routeValues.Add("fields", fields);

        //        routeValues.Add("sort", fields);

        //        if (page != null)
        //            HttpContext.Current.Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(PaginationHeader.Get(page.Value, pageSize, coors.Count(), "users", routeValues, Request)));

        //        if (fields != null)
        //        {
        //            if (asObject)
        //            {
        //                var toReturn = ShapeFactory<CoordinateViewModel>.ShapeList(ConversionFactory.QueryCoordianteToViewModel(coors), fields.ToLower().Split(',').ToList());
        //                return Ok(JsonHelper.listToObject(toReturn, objPropName));
        //            }
        //            else
        //            {
        //                return Ok(ShapeFactory<UserViewModel>.ShapeList(ConversionFactory.QueryUserToViewModel(users), fields.ToLower().Split(',').ToList()));
        //            }
        //        }
        //        else
        //        {
        //            if (asObject)
        //            {
        //                return Ok(JsonHelper.listToObject(ConversionFactory.QueryUserToViewModel(users), objPropName));
        //            }
        //            else
        //            {
        //                return Ok(ConversionFactory.QueryUserToViewModel(users));
        //            }
        //        }
        //    }
        //    else
        //    {
        //        return BadRequest("No users found");
        //    }
        //}
    }
}
