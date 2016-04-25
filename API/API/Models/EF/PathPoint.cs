using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Api.Models.EF
{
    public class PathPoint : BaseModel
    {
        public int CoordinateId { get; set; }
        public virtual Coordinate Coordinate { get; set; }
    }
}