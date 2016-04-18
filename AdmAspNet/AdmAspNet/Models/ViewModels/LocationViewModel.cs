﻿using AdmAspNet.Models.DataContracts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AdmAspNet.Models.ViewModels
{
    public class LocationViewModel
    {
        public int Id { get; set; }

        [Display(Name="Lokasjon")]
        public CoordinateViewModel Coordinate { get; set; }

        [Display(Name="Navn")]
        public string Name { get; set; }

        [Display(Name="Romnummer")]
        public string LocNr { get; set; }

        [Display(Name="Treff")]
        public int Hits { get; }

        [Display(Name ="Type")]
        public TypeViewModel Type { get; set; }

        public virtual List<TypeViewModel> Types { get; set; }

    }
}