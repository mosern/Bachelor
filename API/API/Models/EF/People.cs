using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Api.Models.EF
{
    public class People : BaseModel
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Jobtitle { get; set; }
        [Required]
        public string Desc { get; set; }
        public string TlfOffice { get; set; }
        public string TlfMobile { get; set; }
        public string Email { get; set; }

        public int? LocationId { get; set; }
        public virtual Location Location { get; set; }
    }
}