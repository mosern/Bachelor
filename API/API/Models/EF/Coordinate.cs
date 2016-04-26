using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Api.Models.EF
{
    public class Coordinate : BaseModel
    {
        [Required]
        public double Lng { get; set; }

        [Required]
        public double Lat { get; set; }

        [Required]
        public double Alt { get; set; }

        public virtual Location Location { get; set; }
        public virtual Accesspoint Accesspoint { get; set; }
        public virtual PathPoint PathPoint { get; set; }
    }
}