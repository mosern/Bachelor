using AdmAspNet.Models.DataContracts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AdmAspNet.Models.ViewModels
{
    public class PeopleViewModel
    {
        [Display(Name = "Navn")]
        public string Name { get; set; }

        [Display(Name = "Jobbtittel")]
        public string JobTitle { get; set; }

        [Display(Name = "Kontortelefon")]
        public int TlfOffice { get; set; }

        [Display(Name = "Mobiltelefon")]
        public int TlfMobile { get; set; }

        [Display(Name = "Lokasjon")]
        public Location Location { get; set; }
    }
}