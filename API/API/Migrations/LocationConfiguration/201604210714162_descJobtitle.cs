namespace Api.Migrations.LocationConfiguration
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class descJobtitle : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Locations", "Desc", c => c.String());
            AddColumn("dbo.People", "Jobtitle", c => c.String());
            AddColumn("dbo.People", "Desc", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.People", "Desc");
            DropColumn("dbo.People", "Jobtitle");
            DropColumn("dbo.Locations", "Desc");
        }
    }
}
