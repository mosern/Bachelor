namespace Api.Migrations.LocationConfiguration
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class pathpointneighbourcounter : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PathPoints", "NeighbourCount", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PathPoints", "NeighbourCount");
        }
    }
}
