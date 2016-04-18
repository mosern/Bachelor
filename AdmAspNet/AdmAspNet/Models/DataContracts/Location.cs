using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
namespace AdmAspNet.Models.DataContracts
{
    [DataContract]
    public class Location
    {
        [DataMember(Name ="id")]
        public int Id { get; set; }

        [DataMember(Name="coordinate")]
        public Coordinate Coordinate { get; set; }

        [DataMember(Name="type")]
        public Type Type { get; set; }

        [DataMember(Name ="name")]
        public string Name { get; set; }

        [DataMember(Name ="locNr")]
        public string LocNr { get; set; }

        [DataMember(Name ="hits")]
        public int Hits { get; set; }
    }
}