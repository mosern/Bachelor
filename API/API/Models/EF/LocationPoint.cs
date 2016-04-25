using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Api.Models.EF
{
    public class LocationPoint : BaseModel
    {
        public int LocationId { get; set; }
        public virtual Location Location { get; set; }

        public int PathPointId { get; set; }
        public virtual PathPoint PathPoint { get; set; }
    }
}