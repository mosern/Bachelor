using Api.Models.EF;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Api.Models.Api
{
    public class LocationViewModel
    {
        public int Id { get; set; }
        [Required]
        public Coordinate Coordinate { get; set; }
        [Required]
        public EF.Type Type { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string LocNr { get; set; }
        [Required]
        public int Hits { get; set; }
    }
}