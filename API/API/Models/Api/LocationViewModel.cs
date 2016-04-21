﻿using Api.Models.EF;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Api.Models.Api
{
    public class LocationViewModel : BaseViewModel
    {
        [Required]
        public CoordinateViewModel Coordinate { get; set; }
        [Required]
        public TypeViewModel Type { get; set; }
        [Required]
        public string Name { get; set; }
        [Required, MaxLength(5)]
        public string LocNr { get; set; }
        [Required]
        public int Hits { get; set; }
    }
}