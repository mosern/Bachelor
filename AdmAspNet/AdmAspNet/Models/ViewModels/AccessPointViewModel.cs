using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AdmAspNet.Models.ViewModels
{
    public class AccessPointViewModel
    {
        public int Id { get; set; }

        [Display(Name="Beskrivelse")]
        public string Desc { get; set;  }

        [Display(Name ="MAC-adresse")]
        public string MacAddress { get; set; }

        public CoordinateViewModel Coordinate { get; set; }

    }
}