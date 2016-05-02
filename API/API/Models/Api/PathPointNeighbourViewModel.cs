using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Api.Models.Api
{
    public class PathPointNeighbourViewModel : BaseViewModel
    {
        public CoordinateViewModel Coordinate { get; set; }
        public IEnumerable<object> Neighbours { get; set; }

    }
}