using Api.Models.Api;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace Api.Factories
{
    /// <summary>
    /// Generic class that shapes an object, based on the spesified fields. Has methodes for both individual objects and lists of objects
    /// </summary>
    /// <typeparam name="X">a ViewModel</typeparam>
    public class ShapeFactory<X> where X : BaseViewModel
    {
        //TODO Using generic type?
        public static object Shape(X obj, List<string> rawFields)
        {
            List<string> fields = new List<string>();

            foreach(string field in rawFields)
            {
                fields.Add(field.ToLower());
            }

            ExpandoObject toReturn = new ExpandoObject();

            var properties = obj.GetType().GetProperties();

            foreach(var property in properties)
            {
                if (!property.PropertyType.IsPrimitive 
                    && !property.PropertyType.IsEnum
                    && !property.PropertyType.Equals(typeof(string))
                    && !property.PropertyType.Equals(typeof(decimal))
                    && !property.PropertyType.Equals(typeof(int?)))
                {

                    var o = property.GetValue(obj);

                    object value;
                    if (new BaseViewModel().GetType().IsInstanceOfType(o))
                    {
                        
                        if (fields.Contains(property.Name.ToLower()))
                        {
                            value = o;
                        }
                        else
                        {
                            value = ShapeFactory<BaseViewModel>.Shape((BaseViewModel)o, fields);
                        }
                        
                    }
                    else
                    {
                        if (o != null)
                        {
                            value = ShapeFactory<BaseViewModel>.ShapeList((IEnumerable<BaseViewModel>)o, fields);
                        }
                        else
                        {
                            value = null;
                        }
                    }

                    if (fields.Contains(property.Name.ToLower()))
                    {
                        ((IDictionary<string, object>)toReturn).Add(property.Name.ToLower(), value);
                        fields.Remove(property.Name.ToLower());
                    }
                    else if(ContainsField(property, fields))
                    {
                        ((IDictionary<string, object>)toReturn).Add(property.Name.ToLower(), value);
                    }

                }
            }

            foreach (var field in fields)
            {
                try
                {
                    var value = obj.GetType().GetProperty(field, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance).GetValue(obj);
                    ((IDictionary<string, object>)toReturn).Add(field, value);
                }
                catch (NullReferenceException)
                {

                }
            }

            return toReturn;
        }

        public static IEnumerable<object> ShapeList(IEnumerable<X> inn, List<string> fields)
        {
            List<object> objs = new List<object>();

            foreach (X o in inn)
            {
                objs.Add(Shape(o, fields));
            }

            return objs.AsEnumerable();
        }

        private static bool ContainsField(PropertyInfo property, List<string> fields)
        {
            var properties = property.PropertyType.GetProperties().Select(p => p.Name);

            foreach(var prop in properties)
            {
                if (fields.Contains(prop.ToLower()))
                    return true;
            }

            return false;
        }
    }
}