namespace IdSrv.Migrations.UserDBConfiguration
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Providers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Username = c.String(),
                        Password = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.UserProviders",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Identifier = c.String(),
                        ProviderId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Providers", t => t.ProviderId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.ProviderId)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserProviders", "UserId", "dbo.Users");
            DropForeignKey("dbo.UserProviders", "ProviderId", "dbo.Providers");
            DropIndex("dbo.UserProviders", new[] { "UserId" });
            DropIndex("dbo.UserProviders", new[] { "ProviderId" });
            DropTable("dbo.UserProviders");
            DropTable("dbo.Users");
            DropTable("dbo.Providers");
        }
    }
}
