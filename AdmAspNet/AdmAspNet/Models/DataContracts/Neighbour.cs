using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace AdmAspNet.Models.DataContracts
{
    [DataContract]
    public class Neighbour
    {
        [DataMember(Name ="id")]
        public int Id { get; set; }

        [DataMember(Name ="distance")]
        public double Distance { get; set; }

        [DataMember(Name ="pathPoint1")]
        public PathPoint PathPoint1 { get; set; }

        [DataMember(Name ="pathPoint2")]
        public PathPoint PathPoint2 { get; set; }
    }
}