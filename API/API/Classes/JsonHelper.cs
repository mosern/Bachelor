using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;

namespace Api.Classes
{
    public class JsonHelper
    {
        public static object listToObject(IEnumerable<object> objects, string type)
        {
            ExpandoObject obj = new ExpandoObject();
            ((IDictionary<string, object>)obj).Add(type, objects);
            return obj;
        }
    }
}