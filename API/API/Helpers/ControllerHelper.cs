using Api.Classes;
using Api.Factories;
using Api.Models.Api;
using Api.Models.EF;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
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

        public static object post<X, Y>(Y viewModel) where X : BaseModel where Y : BaseViewModel
        {
            try
            {
                X model = AutoMapConfig.configureMaping().Map<Y, X>(viewModel);

                using(var repo = new LocationRepository<X>())
                model = repo.Create(model);

                return AutoMapConfig.configureMaping().Map<X, Y>(model);
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        public static BaseDbModel post(User user)
        {
            try
            {
                return new Repository<User>().Create(AutoMapConfig.configureMaping().Map<User, User>(user));
            }
            catch
            {
                throw new Exception();
            }
        }

        public static void Put<X>(object viewModel) where X : BaseModel
        {
            try
            {
                X model = AutoMapConfig.configureMaping().Map<object, X>(viewModel);

                using (var repo = new LocationRepository<X>())
                    repo.Update(model);
            }
            catch(Exception e)
            {
                throw e;
            }
            
        }

        public static void Put(User model)
        {
            try
            {
                new Repository<User>().Update(model);
            }
            catch (Exception e)
            {
                throw e;
            }

        }

        public static void Patch<X>(object viewModel) where X : BaseModel
        {
            X src;
            using (var repo = new LocationRepository<X>())
                src = repo.Read(((BaseViewModel)viewModel).Id.Value);

            var srcProp = src.GetType().GetProperties();

            var destProp = viewModel.GetType().GetProperties();

            foreach(PropertyInfo prop in destProp)
            {
                var property = src.GetType().GetProperty(prop.Name);

                if (property != null && prop.GetValue(viewModel) != null)
                {
                    property.SetValue(src,prop.GetValue(viewModel));
                }
            }

            using (var repo = new LocationRepository<X>())
                repo.Update(src);
        }

        public static void Delete<X>(int id) where X : BaseModel
        {
            try
            {
                X model = (X)new BaseModel() { Id = id };

                using (var repo = new LocationRepository<X>())
                    repo.Delete(model);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static void Delete(User user)
        {
            try
            {
                new Repository<User>().Delete(user);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}