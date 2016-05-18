using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;

namespace Api.Classes
{

    /// <summary>
    /// Helper class for making objects. 
    /// Needed because Android differentiates JsonArray and JsonObject.
    /// Therefor having the returned data as JsonObjects only, makes it easier for the Android develpers.
    /// 
    /// Written by: Andreas Mosvoll
    /// </summary>
    public class JsonHelper
    {
        public static object listToObject(IEnumerable<object> objects, string type)
        {
            ExpandoObject obj = new ExpandoObject();
            ((IDictionary<string, object>)obj).Add(type, objects);
            return obj;
        }

        public static object variableToObject(object value, string type)
        {
            ExpandoObject obj = new ExpandoObject();
            ((IDictionary<string, object>)obj).Add(type, value);
            return obj;
        }
    }
}