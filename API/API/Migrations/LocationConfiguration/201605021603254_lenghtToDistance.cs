namespace Api.Migrations.LocationConfiguration
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class lenghtToDistance : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PathNeighbours", "Distance", c => c.Double(nullable: false));
            DropColumn("dbo.PathNeighbours", "length");
        }
        
        public override void Down()
        {
            AddColumn("dbo.PathNeighbours", "length", c => c.Int(nullable: false));
            DropColumn("dbo.PathNeighbours", "Distance");
        }
    }
}
