using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Api.Models.EF
{
    public class Location : BaseModel
    {
        [Key, ForeignKey("Coordinate")]
        public new int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string LocNr { get; set; }
        [Required]
        public string Desc { get; set; }

        public int Hits { get; set; }

        public int? CoordinateId { get; set; }
        public virtual Coordinate Coordinate { get; set; }

        public int TypeId { get; set; }
        public virtual Type Type { get; set; }
    }
}