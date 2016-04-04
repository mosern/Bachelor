namespace IdSrv.Migrations.UserDBConfiguration
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Salt : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "Salt", c => c.String(nullable: false, maxLength: 100));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "Salt");
        }
    }
}
