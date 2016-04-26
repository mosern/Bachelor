namespace Api.Migrations.LocationConfiguration
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Accesspoints",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Desc = c.String(nullable: false),
                        MacAddress = c.String(nullable: false),
                        CoordinateId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Coordinates", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.Coordinates",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Lng = c.Double(nullable: false),
                        Lat = c.Double(nullable: false),
                        Alt = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Locations",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(nullable: false),
                        LocNr = c.String(nullable: false),
                        Desc = c.String(nullable: false),
                        Hits = c.Int(nullable: false),
                        CoordinateId = c.Int(),
                        TypeId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Coordinates", t => t.Id)
                .ForeignKey("dbo.Types", t => t.TypeId, cascadeDelete: true)
                .Index(t => t.Id)
                .Index(t => t.TypeId);
            
            CreateTable(
                "dbo.Types",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.PathPoints",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        CoordinateId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Coordinates", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.LocationPoints",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        LocationId = c.Int(nullable: false),
                        PathPointId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Locations", t => t.LocationId, cascadeDelete: true)
                .ForeignKey("dbo.PathPoints", t => t.PathPointId, cascadeDelete: true)
                .Index(t => t.LocationId)
                .Index(t => t.PathPointId);
            
            CreateTable(
                "dbo.PathNeighbours",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        length = c.Int(nullable: false),
                        PathPointId1 = c.Int(),
                        PathPointId2 = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PathPoints", t => t.PathPointId1)
                .ForeignKey("dbo.PathPoints", t => t.PathPointId2)
                .Index(t => t.PathPointId1)
                .Index(t => t.PathPointId2);
            
            CreateTable(
                "dbo.People",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        Jobtitle = c.String(nullable: false),
                        Desc = c.String(nullable: false),
                        TlfOffice = c.String(),
                        TlfMobile = c.String(),
                        Email = c.String(),
                        LocationId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Locations", t => t.LocationId)
                .Index(t => t.LocationId);
            
            CreateTable(
                "dbo.UserLocations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Hits = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                        LocationId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Locations", t => t.LocationId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.LocationId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserLocations", "UserId", "dbo.Users");
            DropForeignKey("dbo.UserLocations", "LocationId", "dbo.Locations");
            DropForeignKey("dbo.People", "LocationId", "dbo.Locations");
            DropForeignKey("dbo.PathNeighbours", "PathPointId2", "dbo.PathPoints");
            DropForeignKey("dbo.PathNeighbours", "PathPointId1", "dbo.PathPoints");
            DropForeignKey("dbo.LocationPoints", "PathPointId", "dbo.PathPoints");
            DropForeignKey("dbo.LocationPoints", "LocationId", "dbo.Locations");
            DropForeignKey("dbo.Accesspoints", "Id", "dbo.Coordinates");
            DropForeignKey("dbo.PathPoints", "Id", "dbo.Coordinates");
            DropForeignKey("dbo.Locations", "TypeId", "dbo.Types");
            DropForeignKey("dbo.Locations", "Id", "dbo.Coordinates");
            DropIndex("dbo.UserLocations", new[] { "LocationId" });
            DropIndex("dbo.UserLocations", new[] { "UserId" });
            DropIndex("dbo.People", new[] { "LocationId" });
            DropIndex("dbo.PathNeighbours", new[] { "PathPointId2" });
            DropIndex("dbo.PathNeighbours", new[] { "PathPointId1" });
            DropIndex("dbo.LocationPoints", new[] { "PathPointId" });
            DropIndex("dbo.LocationPoints", new[] { "LocationId" });
            DropIndex("dbo.PathPoints", new[] { "Id" });
            DropIndex("dbo.Locations", new[] { "TypeId" });
            DropIndex("dbo.Locations", new[] { "Id" });
            DropIndex("dbo.Accesspoints", new[] { "Id" });
            DropTable("dbo.UserLocations");
            DropTable("dbo.People");
            DropTable("dbo.PathNeighbours");
            DropTable("dbo.LocationPoints");
            DropTable("dbo.PathPoints");
            DropTable("dbo.Types");
            DropTable("dbo.Locations");
            DropTable("dbo.Coordinates");
            DropTable("dbo.Accesspoints");
        }
    }
}
