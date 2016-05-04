using AdmAspNet.Models.DataContracts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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

        [Display(Name="Beskrivelse")]
        public string Desc { get; set; }

        [Display(Name ="Type")]
        public TypeViewModel Type { get; set; }

        [Display(Name="Naboid")]
        public int NeighbourId { get; set; }

        public virtual List<TypeViewModel> Types { get; set; }

        public SelectList DropDown { get; set; }

    }
}