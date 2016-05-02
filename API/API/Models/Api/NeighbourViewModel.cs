using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Api.Models.Api
{
    public class NeighbourViewModel : BaseViewModel
    {
        public double Distance { get; set; }
        public PathPointViewModel pathPoint1 { get; set; }
        public PathPointViewModel pathPoint2 { get; set; }
    }
}