using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AdmAspNet.Models.ViewModels
{
    public class NeighbourViewModel
    {
        public int Id { get; set; }

        [Display(Name="Fra")]
        public PathPointViewModel PathPoint1 { get; set; }

        [Display(Name ="Til")]
        public PathPointViewModel PathPoint2 { get; set; }

        [Display(Name = "Distanse")]
        public double Distance { get; set; }

        public SelectList Liste { get; set; }
    }
}