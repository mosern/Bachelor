using Api.Classes;
using Api.Models.EF;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace Api.Models.Api
{
    public class UserLocationInfo
    {
        static LocationRepository<Coordinate> CoorRepo = new LocationRepository<Coordinate>();
        static LocationRepository<Location>LocRepo = new LocationRepository<Location>();
        public UserLocationInfo(UserLocation userlocation)
        {
            Location loc = LocRepo.List().Where(l => l.Id == userlocation.LocationId).FirstOrDefault();

            Id = userlocation.Id;
            Coordinate = CoorRepo.List().Where(c => c.Id == loc.CoordinateId).FirstOrDefault();
            Name = loc.Name;
            LocNr = loc.LocNr;
            Hits = userlocation.Hits;
        }

        public static object Shape(UserLocation userlocation, List<string> fields)
        {
            object usrLoc;

            if (fields.Any(f =>
            f.Equals("coordinate", StringComparison.OrdinalIgnoreCase) ||
            f.Equals("ing", StringComparison.OrdinalIgnoreCase) ||
            f.Equals("lat", StringComparison.OrdinalIgnoreCase) ||
            f.Equals("alt", StringComparison.OrdinalIgnoreCase)
            ))
            {
                UserLocationInfo tempUsrLoc = new UserLocationInfo(userlocation);

                ExpandoObject temp = new ExpandoObject();
                ((IDictionary<string, object>)temp).Add("id", tempUsrLoc.Id);
                ((IDictionary<string, object>)temp).Add("name", tempUsrLoc.Name);
                ((IDictionary<string, object>)temp).Add("coordinate", CoordinateInfo.Shape(tempUsrLoc.Coordinate, fields));
                ((IDictionary<string, object>)temp).Add("locnr", tempUsrLoc.LocNr);
                ((IDictionary<string, object>)temp).Add("hits", tempUsrLoc.Hits);

                usrLoc = temp;

                if (!fields.Any(f => f.Equals("coordinate", StringComparison.OrdinalIgnoreCase)))
                {
                    fields.Add("coordinate");
                }
            }
            else
            {
                Location loc = LocRepo.List().Where(l => l.Id == userlocation.LocationId).FirstOrDefault();

                ExpandoObject temp = new ExpandoObject();
                ((IDictionary<string, object>)temp).Add("id", userlocation.Id);
                ((IDictionary<string, object>)temp).Add("name", loc.Name);
                ((IDictionary<string, object>)temp).Add("locnr", loc.LocNr);
                ((IDictionary<string, object>)temp).Add("hits", userlocation.Hits);

                usrLoc = temp;
            }

            ExpandoObject toReturn = new ExpandoObject();

            foreach (var field in fields)
            {
                try
                {
                    var value = ((IDictionary<string, object>)usrLoc)[field.ToLower()];
                    ((IDictionary<string, object>)toReturn).Add(field, value);
                }
                catch (KeyNotFoundException)
                {
                    
                }
            }

            return toReturn;
        }

        public static IEnumerable<UserLocationInfo> List(IEnumerable<UserLocation> userLocations)
        {
            List<UserLocationInfo> userLocInf = new List<UserLocationInfo>();

            foreach (UserLocation userLocation in userLocations)
            {
                userLocInf.Add(new UserLocationInfo(userLocation));
            }

            return userLocInf as IEnumerable<UserLocationInfo>;
        }

        public static IEnumerable<object> ShapeList(IEnumerable<UserLocation> userlocations, List<string> fields)
        {
            List<object> toReturn = new List<object>();

            foreach (UserLocation userLocation in userlocations)
            {
                toReturn.Add(Shape(userLocation, fields));
            }

            return toReturn;
        }

        public int Id { get; }
        public Coordinate Coordinate { get; }
        public string Name { get; }
        public string LocNr { get; }
        public int Hits { get; }
    }
}