using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Api.Models.EF
{
    public class Location : BaseModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string LocNr { get; set; }

        public int Hits { get; set; }

        public int CoordinateId { get; set; }
        public virtual Coordinate Coordinate { get; set; }

        public int TypeId { get; set; }
        public virtual Type Type { get; set; }
    }
}