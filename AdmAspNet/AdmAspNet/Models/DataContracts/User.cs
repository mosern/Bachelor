using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace AdmAspNet.Models.DataContracts
{
    [DataContract]
    public class User
    {
        [DataMember(Name ="id")]
        public int Id { get; set; }

        [DataMember(Name ="username")]
        public string Username { get; set; }

        [DataMember(Name ="location")]
        public List<Location> Locations { get; set; }
    }
}