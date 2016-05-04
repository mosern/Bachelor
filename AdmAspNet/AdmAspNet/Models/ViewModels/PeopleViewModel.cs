using AdmAspNet.Models.DataContracts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AdmAspNet.Models.ViewModels
{
    public class PeopleViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Navn")]
        public string Name { get; set; }

        [Display(Name = "Jobbtittel")]
        public string JobTitle { get; set; }

        [Display(Name="Beskrivelse")]
        public string Desc { get; set; }

        [Display(Name = "Kontortelefon")]
        [DataType(DataType.PhoneNumber)]
        public int TlfOffice { get; set; }

        [Display(Name = "Mobiltelefon")]
        [DataType(DataType.PhoneNumber)]
        public int TlfMobile { get; set; }

        [Display(Name="Epost")]
        [EmailAddress(ErrorMessage ="Epostadressen er ugyldig")]
        public string Email { get; set; }

        [Display(Name = "Lokasjon")]
        public Location Location { get; set; }

        public SelectList DropDown { get; set; }
    }
}