using Api.Classes;
using Api.Models.EF;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;

namespace Api.Models.Api
{
    public class AccesspointInfo
    {
        static LocationRepository<Coordinate> CoorRepo = new LocationRepository<Coordinate>();
        public AccesspointInfo(Accesspoint accesspoint)
        {
            Id = accesspoint.Id;
            Coordinate = CoorRepo.List().Where(c => c.Id == accesspoint.CoordinateId).FirstOrDefault();
            Desc = accesspoint.Desc;
            MacAddress = accesspoint.MacAddress;
        }

        public static object Shape(Accesspoint accesspoint, List<string> fields)
        {
            object acc;

            if (fields.Any(f =>
            f.Equals("coordinate", StringComparison.OrdinalIgnoreCase) ||
            f.Equals("ing", StringComparison.OrdinalIgnoreCase) ||
            f.Equals("lat", StringComparison.OrdinalIgnoreCase) ||
            f.Equals("alt", StringComparison.OrdinalIgnoreCase)
            ))
            {
                AccesspointInfo tempAccInf = new AccesspointInfo(accesspoint);

                ExpandoObject temp = new ExpandoObject();
                ((IDictionary<string, object>)temp).Add("id", tempAccInf.Id);
                ((IDictionary<string, object>)temp).Add("desc", tempAccInf.Desc);
                ((IDictionary<string, object>)temp).Add("coordinate", CoordinateInfo.Shape(tempAccInf.Coordinate, fields));
                ((IDictionary<string, object>)temp).Add("macaddress", tempAccInf.MacAddress);

                acc = temp;

                if (!fields.Any(f => f.Equals("coordinate", StringComparison.OrdinalIgnoreCase)))
                {
                    fields.Add("coordinate");
                }
            }
            else
            {
                ExpandoObject temp = new ExpandoObject();
                ((IDictionary<string, object>)temp).Add("id", accesspoint.Id);
                ((IDictionary<string, object>)temp).Add("desc", accesspoint.Desc);
                ((IDictionary<string, object>)temp).Add("macaddress", accesspoint.MacAddress);

                acc = temp;
            }

            ExpandoObject toReturn = new ExpandoObject();

            foreach (var field in fields)
            {
                try
                {
                    var value = ((IDictionary<string, object>)acc)[field.ToLower()];
                    ((IDictionary<string, object>)toReturn).Add(field, value);
                }
                catch (KeyNotFoundException)
                {

                }
            }

            return toReturn;
        }

        public static IEnumerable<AccesspointInfo> List(IEnumerable<Accesspoint> accessPoints)
        {
            List<AccesspointInfo> accInf = new List<AccesspointInfo>();

            foreach (Accesspoint accesspoint in accessPoints)
            {
                accInf.Add(new AccesspointInfo(accesspoint));
            }

            return accInf as IEnumerable<AccesspointInfo>;
        }

        public static IEnumerable<object> ShapeList(IEnumerable<Accesspoint> accessPoints, List<string> fields)
        {
            List<object> toReturn = new List<object>();

            foreach (Accesspoint accesspoint in accessPoints)
            {
                toReturn.Add(Shape(accesspoint, fields));
            }

            return toReturn;
        }

        public int Id { get; }
        public Coordinate Coordinate { get; }
        public string Desc { get; }
        public string MacAddress { get; }
    }
}