using Api.Models;
using Api.Models.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Api.Models.Api
{
    public class PeopleViewModel : BaseViewModel
    {
        public string Name { get; set; }
        public string Jobtitle { get; set; }
        public string Desc { get; set; }
        public string TlfOffice { get; set; }
        public string TlfMobile { get; set; }
        public string Email { get; set; }
        public LocationViewModel Location { get; set; }
    }
}