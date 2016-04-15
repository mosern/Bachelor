using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Api.Models.Api
{
    public class UserViewModel : BaseViewModel
    {
        public string Username { get; set; }
        public IQueryable<LocationViewModel> Locations { get; set; }

    }
}