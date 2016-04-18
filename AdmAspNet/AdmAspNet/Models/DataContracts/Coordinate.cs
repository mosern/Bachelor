using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace AdmAspNet.Models.DataContracts
{
    [DataContract]
    public class Coordinate
    {
        [DataMember(Name ="lng")]
        public double Lng { get; set; }

        [DataMember(Name ="lat")]
        public double Lat { get; set; }

        [DataMember(Name ="alt")]
        public double Alt { get; set; }

        [DataMember(Name="id")]
        public int Id { get; set; }

    }
}