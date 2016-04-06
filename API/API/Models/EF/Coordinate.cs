﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
    }
}