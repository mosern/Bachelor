using Api.Models.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Api.Models.Api
{
    public class UserLocationInfo
    {
        public UserLocationInfo(UserLocation userlocation)
        {
            Id = userlocation.Id;
            Coordinate = userlocation.Location.Coordinate;
            Name = userlocation.Location.Name;
            LocNr = userlocation.Location.LocNr;
            Hits = userlocation.Hits;
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

        public int Id { get; }
        public Coordinate Coordinate { get; }
        public string Name { get; }
        public string LocNr { get; }
        public int Hits { get; }
    }
}