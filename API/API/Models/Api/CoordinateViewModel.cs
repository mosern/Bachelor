using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Api.Models.Api
{
    public class CoordinateViewModel : BaseViewModel
    {
        public double Lng { get; set; }
        public double Lat { get; set; }
        public double Alt { get; set; }
    }
}