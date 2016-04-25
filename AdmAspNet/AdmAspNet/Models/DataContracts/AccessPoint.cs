using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace AdmAspNet.Models.DataContracts
{
    [DataContract]
    public class AccessPoint
    {
        [DataMember(Name ="id")]
        public int Id { get; set; }
        [DataMember(Name ="desc")]
        public string Desc { get; set; }

        [DataMember(Name ="macAddress")]
        public string MacAddress { get; set; }

        [DataMember(Name ="coordinate")]
        public Coordinate Coordinate { get; set; }
    }
}