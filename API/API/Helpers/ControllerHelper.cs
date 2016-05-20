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
    /// <summary>
    /// Basic controller functionality implementet as generic methodes. This makes implementing a new controller easy.
    /// 
    /// Written by: Andreas Mosvoll
    /// </summary>
    public class ControllerHelper
    {
        /// <summary>
        /// Converts object to X where X is of type BaseViewModel and optionally applies field filtering.
        /// </summary>
        /// <typeparam name="X">Type of ViewModel, used for converting object to a viewmodel</typeparam>
        /// <param name="obj">The object to process</param>
        /// <param name="fields">The fields to include in returned object. Returns all fields if no field is specified</param>
        /// <returns>Processed object</returns>
        public static object get<X>(object obj, string fields = null) where X : BaseViewModel
        {
            if (fields != null)
            {
                return ShapeFactory<X>.Shape(AutoMapConfig.getMapper().Map<object, X>(obj), fields.ToLower().Split(',').ToList());
            }
            else
            {
                return AutoMapConfig.getMapper().Map<object, X>(obj);
            }
        }

        /// <summary>
        /// Converts collection of object to X where X is of type BaseViewModel and optionally applies field filtering, sorting and paging with support for custom pagesize.
        /// </summary>
        /// <typeparam name="X">Type of ViewModel, used for converting object to a viewmodel</typeparam>
        /// <param name="obj">The object to process</param>
        /// <param name="context">HttpContext used to add "x-pagination" header</param>
        /// <param name="request">Used by PaginationHeader class to initiate UrlHelper that is used to generate links for next and previous page</param>
        /// <param name="routeName">Used in pagination header</param>
        /// <param name="asObject">Spesify if you want a collection or a object with the collection as a property</param>
        /// <param name="objPropName">Name of object if asObject is true</param>
        /// <param name="fields">The fields to include in returned object. Returns all fields if no field is specified</param>
        /// <param name="sort">The fields to sort by, user "," to serparate fields. Use "-" in fornt of field name to sort decending. Sorts accesnding by id as default</param>
        /// <param name="page">The page you want</param>
        /// <param name="pageSize">The size you want your pages to be</param>
        /// <returns>Processed object</returns>
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
                    temp = obj;
                }

                IDictionary<string, object> routeValues = new Dictionary<string, object>();

                if (fields != null)
                {
                    routeValues.Add("fields", fields);
                    routeValues.Add("sort", sort);

                    if (page != null)
                        context.Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(PaginationHeader.Get(page.Value, pageSize.Value, obj.Count(), routeName, routeValues, request)));

                    var toShape = AutoMapConfig.getMapper().Map<IEnumerable<object>, IEnumerable<X>>(temp);
                    IQueryable<object> toReturn = ShapeFactory<X>.ShapeList(toShape, fields.ToLower().Split(',').ToList()).AsQueryable().ApplySort(sort);

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
                        context.Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(PaginationHeader.Get(page.Value, pageSize.Value, obj.Count(), routeName, routeValues, request)));

                    var toReturn = AutoMapConfig.getMapper().Map<IEnumerable<object>, IEnumerable<X>>(temp).AsQueryable().ApplySort(sort);

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

        /// <summary>
        /// Creates new object in database from viewmodel
        /// </summary>
        /// <typeparam name="X">Type of database model, must inherit BaseModel</typeparam>
        /// <typeparam name="Y">Type of ViewModel, must inherit BaseViewModel</typeparam>
        /// <param name="viewModel">The view model containing data to create new database object from</param>
        /// <returns>ViewModel created from the new database object</returns>
        public static object post<X, Y>(Y viewModel) where X : BaseModel where Y : BaseViewModel
        {
            try
            {
                X model = AutoMapConfig.getMapper().Map<Y, X>(viewModel);

                using(var repo = new LocationRepository<X>())
                model = repo.Create(model);

                return AutoMapConfig.getMapper().Map<X, Y>(model);
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        //TODO Find a solution that works for users and BaseModel
        public static BaseDbModel post(User user)
        {
            try
            {
                return new Repository<User>().Create(AutoMapConfig.getMapper().Map<User, User>(user));
            }
            catch
            {
                throw new Exception();
            }
        }

        /// <summary>
        /// Full update of object in database from viewmodel
        /// </summary>
        /// <typeparam name="X">Type of database model, must inherit BaseModel</typeparam>
        /// <param name="viewModel">The view model containing data to be set to existing database object</param>
        public static void Put<X>(object viewModel) where X : BaseModel
        {
            try
            {
                X model = AutoMapConfig.getMapper().Map<object, X>(viewModel);

                using (var repo = new LocationRepository<X>())
                    repo.Update(model);
            }
            catch(Exception e)
            {
                throw e;
            }
            
        }

        //TODO Find a solution that works for users and BaseModel
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

        /// <summary>
        /// Partial update of object in database from viewmodel
        /// </summary>
        /// <typeparam name="X">Type of database model, must inherit BaseModel</typeparam>
        /// <param name="viewModel">The view model containing data to be set to existing database object</param>
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

        /// <summary>
        /// Removes a object from the database
        /// </summary>
        /// <typeparam name="X"></typeparam>
        /// <param name="id"></param>
        public static void Delete<X>(int id) where X : BaseModel
        {
            try
            {
                using (var repo = new LocationRepository<X>())
                {
                    X model = repo.Read(id);
                    repo.Delete(model);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        //TODO Find a solution that works for users and BaseModel
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