using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AdmAspNet.Models.ViewModels
{
    public class CoordinateViewModel
    {
        public int Id { get; set; }
        [Display(Name ="Lengdegrad")]
        public double Lng { get; set; }
        [Display(Name ="Breddegrad")]
        public double Lat { get; set; }
        [Display(Name ="Høyde")]
        public double Alt { get; set; }
    }
}