using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Api.Models.Api
{
    public class SearchViewModel : BaseViewModel
    {
        public IQueryable<LocationViewModel> LocationViewModel { get; set; }
        public IQueryable<PeopleViewModel> PeopleViewModel { get; set; }
    }
}