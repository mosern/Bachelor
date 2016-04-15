using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Api.Models.Api
{
    public class AccesspointViewModel : BaseViewModel
    {
        public int Id { get; set; }
        public string Desc { get; set; }
        public string MacAddress { get; set; }
        public CoordinateInfo Coordinate { get; set; }
    }
}