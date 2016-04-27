using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Api.Models.Api
{
    public class PathPointViewModel : BaseViewModel
    {
        public int Distance { get; set; }
        public CoordinateViewModel Coordinate { get; set; }
        public IEnumerable<PathPointViewModel> Neighbours { get; set; }
    }
}