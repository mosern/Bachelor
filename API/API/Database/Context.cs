using Api.Models.EF;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Web;

namespace Api.Classes
{
    public class Context : DbContext
    {
        public Context(): base("bachelor")
        {

        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            //modelBuilder.Entity<Location>()
            //            .HasRequired(l => l.Coordinate)
            //            .WithOptional(c => c.Location);

            //modelBuilder.Entity<Accesspoint>()
            //            .HasRequired(l => l.Coordinate)
            //            .WithOptional(c => c.Accesspoint);

            //modelBuilder.Entity<PathPoint>()
            //            .HasRequired(l => l.Coordinate)
            //            .WithOptional(c => c.PathPoint);
        }

        public virtual DbSet<Location> Location { get; set; }
        public virtual DbSet<Coordinate> Coordinate { get; set; }
        public virtual DbSet<Accesspoint> Accesspoint { get; set; }
        public virtual DbSet<Models.EF.Type> Type { get; set; }
        public virtual DbSet<UserLocation> UserLocation { get; set; }
        public virtual DbSet<People> People { get; set; }
        public virtual DbSet<PathPoint> PathPoint { get; set; }
        public virtual DbSet<PathNeighbours> PathNeighbours { get; set; }
        public virtual DbSet<LocationPoint> LocationPoint { get; set; }
    }
}