namespace Api.Migrations.LocationConfiguration
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Fix3 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Locations", "CoordinateId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Locations", "CoordinateId", c => c.Int());
        }
    }
}
