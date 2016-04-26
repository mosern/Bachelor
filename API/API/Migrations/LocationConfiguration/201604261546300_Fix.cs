namespace Api.Migrations.LocationConfiguration
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Fix : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.LocationPoints", "LocationId", "dbo.Locations");
            DropForeignKey("dbo.People", "LocationId", "dbo.Locations");
            DropForeignKey("dbo.UserLocations", "LocationId", "dbo.Locations");
            DropForeignKey("dbo.LocationPoints", "PathPointId", "dbo.PathPoints");
            DropForeignKey("dbo.PathNeighbours", "PathPointId1", "dbo.PathPoints");
            DropForeignKey("dbo.PathNeighbours", "PathPointId2", "dbo.PathPoints");
            DropIndex("dbo.Accesspoints", new[] { "Id" });
            DropIndex("dbo.Locations", new[] { "Id" });
            DropIndex("dbo.PathPoints", new[] { "Id" });
            DropPrimaryKey("dbo.Accesspoints");
            DropPrimaryKey("dbo.Locations");
            DropPrimaryKey("dbo.PathPoints");
            AlterColumn("dbo.Accesspoints", "Id", c => c.Int(nullable: false, identity: true));
            AlterColumn("dbo.Locations", "Id", c => c.Int(nullable: false, identity: true));
            AlterColumn("dbo.PathPoints", "Id", c => c.Int(nullable: false, identity: true));
            AddPrimaryKey("dbo.Accesspoints", "Id");
            AddPrimaryKey("dbo.Locations", "Id");
            AddPrimaryKey("dbo.PathPoints", "Id");
            CreateIndex("dbo.Accesspoints", "Id");
            CreateIndex("dbo.Locations", "Id");
            CreateIndex("dbo.PathPoints", "Id");
            AddForeignKey("dbo.LocationPoints", "LocationId", "dbo.Locations", "Id", cascadeDelete: true);
            AddForeignKey("dbo.People", "LocationId", "dbo.Locations", "Id");
            AddForeignKey("dbo.UserLocations", "LocationId", "dbo.Locations", "Id", cascadeDelete: true);
            AddForeignKey("dbo.LocationPoints", "PathPointId", "dbo.PathPoints", "Id", cascadeDelete: true);
            AddForeignKey("dbo.PathNeighbours", "PathPointId1", "dbo.PathPoints", "Id");
            AddForeignKey("dbo.PathNeighbours", "PathPointId2", "dbo.PathPoints", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PathNeighbours", "PathPointId2", "dbo.PathPoints");
            DropForeignKey("dbo.PathNeighbours", "PathPointId1", "dbo.PathPoints");
            DropForeignKey("dbo.LocationPoints", "PathPointId", "dbo.PathPoints");
            DropForeignKey("dbo.UserLocations", "LocationId", "dbo.Locations");
            DropForeignKey("dbo.People", "LocationId", "dbo.Locations");
            DropForeignKey("dbo.LocationPoints", "LocationId", "dbo.Locations");
            DropIndex("dbo.PathPoints", new[] { "Id" });
            DropIndex("dbo.Locations", new[] { "Id" });
            DropIndex("dbo.Accesspoints", new[] { "Id" });
            DropPrimaryKey("dbo.PathPoints");
            DropPrimaryKey("dbo.Locations");
            DropPrimaryKey("dbo.Accesspoints");
            AlterColumn("dbo.PathPoints", "Id", c => c.Int(nullable: false));
            AlterColumn("dbo.Locations", "Id", c => c.Int(nullable: false));
            AlterColumn("dbo.Accesspoints", "Id", c => c.Int(nullable: false));
            AddPrimaryKey("dbo.PathPoints", "Id");
            AddPrimaryKey("dbo.Locations", "Id");
            AddPrimaryKey("dbo.Accesspoints", "Id");
            CreateIndex("dbo.PathPoints", "Id");
            CreateIndex("dbo.Locations", "Id");
            CreateIndex("dbo.Accesspoints", "Id");
            AddForeignKey("dbo.PathNeighbours", "PathPointId2", "dbo.PathPoints", "Id");
            AddForeignKey("dbo.PathNeighbours", "PathPointId1", "dbo.PathPoints", "Id");
            AddForeignKey("dbo.LocationPoints", "PathPointId", "dbo.PathPoints", "Id", cascadeDelete: true);
            AddForeignKey("dbo.UserLocations", "LocationId", "dbo.Locations", "Id", cascadeDelete: true);
            AddForeignKey("dbo.People", "LocationId", "dbo.Locations", "Id");
            AddForeignKey("dbo.LocationPoints", "LocationId", "dbo.Locations", "Id", cascadeDelete: true);
        }
    }
}
