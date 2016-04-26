using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Api.Models.EF
{
    public class Accesspoint : BaseModel
    {
        [Key, ForeignKey("Coordinate")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public new int Id { get; set; }
        [Required]
        public string Desc { get; set; }

        [Required]
        public string MacAddress { get; set; }

        public int CoordinateId { get; set; }
        public virtual Coordinate Coordinate { get; set; } 
    }
}