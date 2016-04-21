using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Api.Models.EF
{
    public class People : BaseModel
    {
        public string Name { get; set; }
        public string Jobtitle { get; set; }
        public string Desc { get; set; }
        public string TlfOffice { get; set; }
        public string TlfMobile { get; set; }
        public string Email { get; set; }

        public int LocationId { get; set; }
        public virtual Location Location { get; set; }
    }
}