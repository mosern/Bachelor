﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Api.Models.EF
{
    public class Location : BaseModel
    {
        [ForeignKey("Coordinate")]
        public override int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string LocNr { get; set; }
        [Required]
        public string Desc { get; set; }

        public double Distance { get; set; }

        public int Hits { get; set; }

        public int CoordinateId { get; set; }
        public virtual Coordinate Coordinate { get; set; }

        public int TypeId { get; set; }
        public virtual Type Type { get; set; }

        public int? NeighbourId { get; set; }
        public virtual PathPoint Neighbour { get; set; }
    }
}