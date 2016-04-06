using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Api.Models.EF
{
    public class Accesspoint : BaseModel
    {
        [Required]
        public string Desc { get; set; }

        [Required]
        public string MacAddress { get; set; }

        public int CoordinateId { get; set; }
        public virtual Coordinate Coordinate { get; set; } 
    }
}