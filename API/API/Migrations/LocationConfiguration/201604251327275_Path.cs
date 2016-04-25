namespace Api.Migrations.LocationConfiguration
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Path : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Accesspoints", "CoordinateId", "dbo.Coordinates");
            DropForeignKey("dbo.Locations", "CoordinateId", "dbo.Coordinates");
            DropForeignKey("dbo.Locations", "TypeId", "dbo.Types");
            DropForeignKey("dbo.People", "LocationId", "dbo.Locations");
            DropForeignKey("dbo.UserLocations", "LocationId", "dbo.Locations");
            DropForeignKey("dbo.UserLocations", "UserId", "dbo.Users");
            CreateTable(
                "dbo.LocationPoints",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        LocationId = c.Int(nullable: false),
                        PathPointId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Locations", t => t.LocationId)
                .ForeignKey("dbo.PathPoints", t => t.PathPointId)
                .Index(t => t.LocationId)
                .Index(t => t.PathPointId);
            
            CreateTable(
                "dbo.PathPoints",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CoordinateId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Coordinates", t => t.CoordinateId)
                .Index(t => t.CoordinateId);
            
            CreateTable(
                "dbo.PathNeighbours",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        length = c.Int(nullable: false),
                        PathPointId1 = c.Int(nullable: false),
                        PathPointId2 = c.Int(nullable: false),
                        PathPoint1_Id = c.Int(),
                        PathPoint2_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PathPoints", t => t.PathPoint1_Id)
                .ForeignKey("dbo.PathPoints", t => t.PathPoint2_Id)
                .Index(t => t.PathPoint1_Id)
                .Index(t => t.PathPoint2_Id);
            
            AddForeignKey("dbo.Accesspoints", "CoordinateId", "dbo.Coordinates", "Id");
            AddForeignKey("dbo.Locations", "CoordinateId", "dbo.Coordinates", "Id");
            AddForeignKey("dbo.Locations", "TypeId", "dbo.Types", "Id");
            AddForeignKey("dbo.People", "LocationId", "dbo.Locations", "Id");
            AddForeignKey("dbo.UserLocations", "LocationId", "dbo.Locations", "Id");
            AddForeignKey("dbo.UserLocations", "UserId", "dbo.Users", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserLocations", "UserId", "dbo.Users");
            DropForeignKey("dbo.UserLocations", "LocationId", "dbo.Locations");
            DropForeignKey("dbo.People", "LocationId", "dbo.Locations");
            DropForeignKey("dbo.Locations", "TypeId", "dbo.Types");
            DropForeignKey("dbo.Locations", "CoordinateId", "dbo.Coordinates");
            DropForeignKey("dbo.Accesspoints", "CoordinateId", "dbo.Coordinates");
            DropForeignKey("dbo.PathNeighbours", "PathPoint2_Id", "dbo.PathPoints");
            DropForeignKey("dbo.PathNeighbours", "PathPoint1_Id", "dbo.PathPoints");
            DropForeignKey("dbo.LocationPoints", "PathPointId", "dbo.PathPoints");
            DropForeignKey("dbo.PathPoints", "CoordinateId", "dbo.Coordinates");
            DropForeignKey("dbo.LocationPoints", "LocationId", "dbo.Locations");
            DropIndex("dbo.PathNeighbours", new[] { "PathPoint2_Id" });
            DropIndex("dbo.PathNeighbours", new[] { "PathPoint1_Id" });
            DropIndex("dbo.PathPoints", new[] { "CoordinateId" });
            DropIndex("dbo.LocationPoints", new[] { "PathPointId" });
            DropIndex("dbo.LocationPoints", new[] { "LocationId" });
            DropTable("dbo.PathNeighbours");
            DropTable("dbo.PathPoints");
            DropTable("dbo.LocationPoints");
            AddForeignKey("dbo.UserLocations", "UserId", "dbo.Users", "Id", cascadeDelete: true);
            AddForeignKey("dbo.UserLocations", "LocationId", "dbo.Locations", "Id", cascadeDelete: true);
            AddForeignKey("dbo.People", "LocationId", "dbo.Locations", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Locations", "TypeId", "dbo.Types", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Locations", "CoordinateId", "dbo.Coordinates", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Accesspoints", "CoordinateId", "dbo.Coordinates", "Id", cascadeDelete: true);
        }
    }
}
