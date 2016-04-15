using Api.Models.Api;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace Api.Factories
{
    public class ShapeFactory<X> where X : BaseViewModel
    {
        public static object Shape(X obj, List<string> fields)
        {
            ExpandoObject toReturn = new ExpandoObject();

            var properties = obj.GetType().GetProperties();

            foreach(var property in properties)
            {
                if (!property.PropertyType.IsPrimitive 
                    && !property.PropertyType.IsEnum
                    && !property.PropertyType.Equals(typeof(string))
                    && !property.PropertyType.Equals(typeof(decimal)))
                {

                    object o = property.GetValue(obj);
                    var value = ShapeFactory<BaseViewModel>.Shape((BaseViewModel)o, fields);

                    ((IDictionary<string, object>)toReturn).Add(property.Name, value);

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
    }
}