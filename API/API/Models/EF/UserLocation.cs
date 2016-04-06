using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using UserDB;

namespace Api.Models.EF
{
    public class UserLocation : BaseModel
    {
        public int Hits { get; set; }

        public int UserId { get; set; }
        public virtual User User { get; set; }

        public int LocationId { get; set; }
        public virtual Location Location { get; set; }
    }
}