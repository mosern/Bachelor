namespace Api.Migrations.LocationConfiguration
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RequireDescJobtitle : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Locations", "Desc", c => c.String(nullable: false));
            AlterColumn("dbo.People", "Name", c => c.String(nullable: false));
            AlterColumn("dbo.People", "Jobtitle", c => c.String(nullable: false));
            AlterColumn("dbo.People", "Desc", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.People", "Desc", c => c.String());
            AlterColumn("dbo.People", "Jobtitle", c => c.String());
            AlterColumn("dbo.People", "Name", c => c.String());
            AlterColumn("dbo.Locations", "Desc", c => c.String());
        }
    }
}
