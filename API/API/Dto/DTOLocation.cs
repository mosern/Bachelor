using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Api.Dto
{
    public class DTOLocation
    {
        public int Id { get; set; }
        [Required]
        public int CoordinateId { get; set; }
        [Required]
        public int TypeId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string LocNr { get; set; }
        [Required]
        public int Hits { get; set; }
    }
}