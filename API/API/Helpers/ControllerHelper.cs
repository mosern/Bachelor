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
using UserDB;

namespace Api.Helpers
{
    public class ControllerHelper
    {
        public static object get<X>(object obj, string fields = null) where X : BaseViewModel
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

        public static object get<X>(IQueryable<object> obj, HttpContext context, HttpRequestMessage request, string routeName, bool asObject, string objPropName, string fields = null, string sort = "id", int? page = null, int? pageSize = null) where X : BaseViewModel
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

        public static BaseModel post<X>(BaseModel model) where X : BaseModel
        {
            try
            {
                return new LocationRepository<X>().Create(AutoMapConfig.configureMaping().Map<BaseModel, X>(model));
            }
            catch
            {
                throw new Exception();
            }
        }

        public static BaseDbModel post<X>(BaseDbModel model) where X : BaseDbModel
        {
            try
            {
                return new Repository<X>().Create(AutoMapConfig.configureMaping().Map<BaseDbModel, X>(model));
            }
            catch
            {
                throw new Exception();
            }
        }
    }
}