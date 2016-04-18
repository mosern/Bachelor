using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AdmAspNet.Models.ViewModels
{
    public class TypeViewModel
    {
        public int Id { get; set; }

        [Display(Name="Navn")]
        public string Name { get; set; }
    }
}