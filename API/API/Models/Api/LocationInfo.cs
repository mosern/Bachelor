using Api.Classes;
using Api.Models.EF;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Linq;
using System.Web;

namespace Api.Models.Api
{
    public class LocationInfo
    {
        static LocationRepository<Coordinate> CoorRepo = new LocationRepository<Coordinate>();

        public LocationInfo()
        {
            //Name = "Ola";
            //LocNr = "A1111";
        }

        //public LocationInfo(int coordinateId, string name, string locNr, int hits)
        //{
        //    Coordinate = coordinateId;
        //    Name = name;
        //    LocNr = locNr;
        //    Hits = hits;
        //}

        public LocationInfo(Location location)
        {
            Id = location.Id;
            Coordinate = CoorRepo.Read(location.CoordinateId);
            Name = location.Name;
            LocNr = location.LocNr;
            Hits = location.Hits;
        }

        public static Location toLocation(LocationInfo locInf)
        {
            return new Location() { Coordinate = locInf.Coordinate, Name = locInf.Name, LocNr = locInf.LocNr, Hits = locInf.Hits };
        }

        public static object Shape(Location location, List<string> fields)
        {
            object usrLoc;

            if (fields.Any(f =>
            f.Equals("coordinate", StringComparison.OrdinalIgnoreCase) ||
            f.Equals("ing", StringComparison.OrdinalIgnoreCase) ||
            f.Equals("lat", StringComparison.OrdinalIgnoreCase) ||
            f.Equals("alt", StringComparison.OrdinalIgnoreCase)
            ))
            {
                LocationInfo tempLocInf = new LocationInfo(location);

                ExpandoObject temp = new ExpandoObject();
                ((IDictionary<string, object>)temp).Add("id", tempLocInf.Id);
                ((IDictionary<string, object>)temp).Add("name", tempLocInf.Name);
                ((IDictionary<string, object>)temp).Add("coordinate", CoordinateInfo.Shape(tempLocInf.Coordinate, fields));
                ((IDictionary<string, object>)temp).Add("locnr", tempLocInf.LocNr);
                ((IDictionary<string, object>)temp).Add("hits", tempLocInf.Hits);

                usrLoc = temp;

                if (!fields.Any(f => f.Equals("coordinate", StringComparison.OrdinalIgnoreCase)))
                {
                    fields.Add("coordinate");
                }
            }
            else
            {
                ExpandoObject temp = new ExpandoObject();
                ((IDictionary<string, object>)temp).Add("id", location.Id);
                ((IDictionary<string, object>)temp).Add("name", location.Name);
                ((IDictionary<string, object>)temp).Add("locnr", location.LocNr);
                ((IDictionary<string, object>)temp).Add("hits", location.Hits);

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

        public static IEnumerable<LocationInfo> List(IEnumerable<Location> Locations)
        {
            List<LocationInfo> LocInf = new List<LocationInfo>();

            foreach(Location Location in Locations)
            {
                LocInf.Add(new LocationInfo(Location));
            }

            return LocInf as IEnumerable<LocationInfo>;
        }

        public static IEnumerable<object> ShapeList(IEnumerable<Location> locations, List<string> fields)
        {
            List<object> toReturn = new List<object>();

            foreach (Location location in locations)
            {
                toReturn.Add(Shape(location, fields));
            }

            return toReturn;
        }


        public int Id { get; set; }
        [Required]
        public Coordinate Coordinate { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string LocNr { get; set; }
        [Required]
        public int Hits { get; set; }

    }
}