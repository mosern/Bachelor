namespace Api.Migrations.LocationConfiguration
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class locationDistance : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Locations", "Distance", c => c.Double(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Locations", "Distance");
        }
    }
}
