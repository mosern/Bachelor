using Api.Classes;
using Api.Factories;
using Api.Models.Api;
using Api.Models.EF;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace Api.Helpers
{
    public class ControllerHelper<X> where X : BaseViewModel
    {
        public static object get(object obj, string fields = null)
        {
            if (fields != null)
            {
                return ShapeFactory<X>.Shape(AutoMapConfig.configureMaping().Map<object, X>(obj), fields.ToLower().Split(',').ToList());
            }
            else
            {
                return AutoMapConfig.configureMaping().Map<object, X>(obj);
            }
        }

        public static object get(IQueryable<object> obj, HttpContext context, HttpRequestMessage request, string routeName, bool asObject, string objPropName, string fields = null, string sort = "id", int? page = null, int? pageSize = null)
        {


            if (obj != null)
            {
                IQueryable<object> temp;
                if (page != null)
                {
                    temp = obj.ApplySort(sort).Skip(pageSize.Value * (page.Value - 1)).Take(pageSize.Value);
                }
                else
                {
                    temp = obj.ApplySort(sort);
                }

                IDictionary<string, object> routeValues = new Dictionary<string, object>();

                if (fields != null)
                {
                    routeValues.Add("fields", fields);
                    routeValues.Add("sort", sort);

                    if (page != null)
                        context.Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(PaginationHeader.Get(page.Value, pageSize.Value, obj.Count(), routeName, routeValues, request)));

                    var toShape = AutoMapConfig.configureMaping().Map<IEnumerable<object>, IEnumerable<X>>(temp);
                    IQueryable<object> toReturn = ShapeFactory<X>.ShapeList(toShape, fields.ToLower().Split(',').ToList()).AsQueryable();

                    if (asObject)
                    {
                        return JsonHelper.listToObject(toReturn, objPropName);
                    }
                    else
                    {
                        return toReturn;
                    }

                }
                else
                {
                    routeValues.Add("sort", sort);

                    if (page != null)
                        context.Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(PaginationHeader.Get(page.Value, pageSize.Value, obj.Count(), "objects", routeValues, request)));

                    var toReturn = AutoMapConfig.configureMaping().Map<IEnumerable<object>, IEnumerable<X>>(temp).AsQueryable();

                    if (asObject)
                    {
                        return JsonHelper.listToObject(toReturn, objPropName);
                    }
                    else
                    {
                        return toReturn;
                    }
                }


            }
            else
            {
                return null;
            }
        }
    }
}