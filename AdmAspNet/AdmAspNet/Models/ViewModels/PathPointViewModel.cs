using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AdmAspNet.Models.ViewModels
{
    public class PathPointViewModel
    {
        public int Id { get; set; }

        [Display(Name="Distanse")]
        public int Distance { get; set; }

        [Display(Name ="Koordinater")]
        public CoordinateViewModel Coordinate { get; set; }

        [Display(Name ="Naboer")]
        public List<PathPointViewModel> Neighbours { get; set; }
    }
}