namespace Api.Migrations.LocationConfiguration
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Accesspoints",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Desc = c.String(nullable: false),
                        MacAddress = c.String(nullable: false),
                        CoordinateId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Coordinates", t => t.CoordinateId, cascadeDelete: true)
                .Index(t => t.CoordinateId);
            
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
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        LocNr = c.String(nullable: false),
                        Hits = c.Int(nullable: false),
                        CoordinateId = c.Int(nullable: false),
                        TypeId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Coordinates", t => t.CoordinateId, cascadeDelete: true)
                .ForeignKey("dbo.Types", t => t.TypeId, cascadeDelete: true)
                .Index(t => t.CoordinateId)
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
            DropForeignKey("dbo.Locations", "TypeId", "dbo.Types");
            DropForeignKey("dbo.Locations", "CoordinateId", "dbo.Coordinates");
            DropForeignKey("dbo.Accesspoints", "CoordinateId", "dbo.Coordinates");
            DropIndex("dbo.UserLocations", new[] { "LocationId" });
            DropIndex("dbo.UserLocations", new[] { "UserId" });
            DropIndex("dbo.Locations", new[] { "TypeId" });
            DropIndex("dbo.Locations", new[] { "CoordinateId" });
            DropIndex("dbo.Accesspoints", new[] { "CoordinateId" });
            DropTable("dbo.UserLocations");
            DropTable("dbo.Types");
            DropTable("dbo.Locations");
            DropTable("dbo.Coordinates");
            DropTable("dbo.Accesspoints");
        }
    }
}
