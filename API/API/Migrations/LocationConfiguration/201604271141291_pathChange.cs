namespace Api.Migrations.LocationConfiguration
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class pathChange : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.LocationPoints", "LocationId", "dbo.Locations");
            DropForeignKey("dbo.LocationPoints", "PathPointId", "dbo.PathPoints");
            DropIndex("dbo.LocationPoints", new[] { "LocationId" });
            DropIndex("dbo.LocationPoints", new[] { "PathPointId" });
            AddColumn("dbo.Locations", "NeighbourId", c => c.Int());
            CreateIndex("dbo.Locations", "NeighbourId");
            AddForeignKey("dbo.Locations", "NeighbourId", "dbo.PathPoints", "Id");
            DropTable("dbo.LocationPoints");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.LocationPoints",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        LocationId = c.Int(nullable: false),
                        PathPointId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            DropForeignKey("dbo.Locations", "NeighbourId", "dbo.PathPoints");
            DropIndex("dbo.Locations", new[] { "NeighbourId" });
            DropColumn("dbo.Locations", "NeighbourId");
            CreateIndex("dbo.LocationPoints", "PathPointId");
            CreateIndex("dbo.LocationPoints", "LocationId");
            AddForeignKey("dbo.LocationPoints", "PathPointId", "dbo.PathPoints", "Id", cascadeDelete: true);
            AddForeignKey("dbo.LocationPoints", "LocationId", "dbo.Locations", "Id", cascadeDelete: true);
        }
    }
}
