using Api.Models.EF;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Api.Classes
{
    public class Context : DbContext
    {
        public Context(): base("bachelor")
        {

        }

        public virtual DbSet<Location> Location { get; set; }
        public virtual DbSet<Coordinate> Coordinate { get; set; }
        public virtual DbSet<Accesspoint> Accesspoint { get; set; }
        public virtual DbSet<Models.EF.Type> Type { get; set; }
        public virtual DbSet<UserLocation> UserLocation { get; set; }
    }
}