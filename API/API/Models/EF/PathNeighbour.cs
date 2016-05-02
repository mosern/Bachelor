using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Api.Models.EF
{
    public class PathNeighbour : BaseModel
    {
        [Required]
        public double Distance { get; set; }

        [ForeignKey("PathPoint1")]
        public int? PathPointId1 { get; set; }
        public virtual PathPoint PathPoint1 { get; set; }
        [ForeignKey("PathPoint2")]
        public int? PathPointId2 { get; set; }
        public virtual PathPoint PathPoint2 { get; set; }

    }
}