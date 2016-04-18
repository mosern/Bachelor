using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace AdmAspNet.Models.DataContracts
{
    [DataContract]
    public class Type
    {
        [DataMember(Name ="id")]
        public int Id { get; set; }

        [DataMember(Name ="name")]
        public string Name { get; set; }
    }
}