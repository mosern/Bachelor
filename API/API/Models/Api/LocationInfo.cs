using Api.Models.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Api.Models.Api
{
    public class LocationInfo
    {
        public LocationInfo(Location Location)
        {
            Id = Location.Id;
            Coordinate = Location.Coordinate;
            Name = Location.Name;
            LocNr = Location.LocNr;
            Hits = Location.Hits;

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


        public int Id { get; }
        public Coordinate Coordinate { get; }
        public string Name { get; }
        public string LocNr { get; }
        public int Hits { get; }

    }
}