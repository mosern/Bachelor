using Api.Models.EF;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace Api.Models.Api
{
    public class CoordinateInfo
    {
        public CoordinateInfo(Coordinate coordinate)
        {
            Id = coordinate.Id;
            Lng = coordinate.Lng;
            Lat = coordinate.Lat;
            Alt = coordinate.Alt;
        }

        public CoordinateInfo(int id, double lng, double lat, double alt)
        {
            Id = id;
            Lng = lng;
            Lat = lat;
            Alt = alt;
        }

        public static object Shape(Coordinate coordinate, List<string> fields)
        {
            object corInf = coordinate;

            ExpandoObject toReturn = new ExpandoObject();

            foreach (var field in fields)
            {
                try
                {
                    var value = corInf.GetType().GetProperty(field, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance).GetValue(corInf);
                    ((IDictionary<String, Object>)toReturn).Add(field, value);
                }
                catch (NullReferenceException)
                {

                }
            }

            return toReturn;
        }

        public IEnumerable<CoordinateInfo> List(IEnumerable<Coordinate> coordinates)
        {
            List<CoordinateInfo> corInf = new List<CoordinateInfo>();

            foreach(Coordinate coordinate in coordinates)
            {
                corInf.Add(new CoordinateInfo(coordinate));
            }

            return corInf;
        }

        public IEnumerable<object> ShapeList(IEnumerable<Coordinate> coordinates, List<string> fields)
        {
            List<object> toReturn = new List<object>();

            foreach (Coordinate coordinate in coordinates)
            {
                toReturn.Add(Shape(coordinate, fields));
            }

            return toReturn;
        }

        public int Id { get; }
        public double Lng { get; }
        public double Lat { get; }
        public double Alt { get; }
    }
}