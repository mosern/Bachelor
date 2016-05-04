using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace AdmAspNet.Models.DataContracts
{
    [DataContract]
    public class PathPoint
    {
        [DataMember(Name ="id")]
        public int Id { get; set; }

        [DataMember(Name ="distance")]
        public int Distance { get; set; }

        [DataMember(Name ="coordinate")]
        public Coordinate Coordinate { get; set; }

        [DataMember(Name ="neighbours")]
        public List<PathPoint> Neighbours { get; set; }
    }
}