namespace IdSrv.Migrations.UserDBConfiguration
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Anotations : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Providers", "Name", c => c.String(nullable: false));
            AlterColumn("dbo.Users", "Username", c => c.String(nullable: false, maxLength: 40));
            AlterColumn("dbo.Users", "Password", c => c.String(nullable: false, maxLength: 25));
            AlterColumn("dbo.UserProviders", "Identifier", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.UserProviders", "Identifier", c => c.String());
            AlterColumn("dbo.Users", "Password", c => c.String());
            AlterColumn("dbo.Users", "Username", c => c.String());
            AlterColumn("dbo.Providers", "Name", c => c.String());
        }
    }
}
