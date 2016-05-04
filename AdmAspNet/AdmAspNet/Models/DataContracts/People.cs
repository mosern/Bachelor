using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace AdmAspNet.Models.DataContracts
{
    [DataContract]
    public class People
    {
        [DataMember(Name ="name")]
        public string Name { get; set; }

        [DataMember(Name ="jobtitle")]
        public string JobTitle { get; set; }

        [DataMember(Name ="tlfOffice")]
        public int TlfOffice { get; set; }

        [DataMember(Name ="tlfMobile")]
        public int TlfMobile { get; set; }

       [DataMember(Name ="location")]
       public Location Location { get; set; }
    }
}