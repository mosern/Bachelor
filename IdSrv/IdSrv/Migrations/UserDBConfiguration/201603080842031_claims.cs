namespace IdSrv.Migrations.UserDBConfiguration
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class claims : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Claims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Type = c.String(nullable: false),
                        Value = c.String(nullable: false),
                        UserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Claims", "UserId", "dbo.Users");
            DropIndex("dbo.Claims", new[] { "UserId" });
            DropTable("dbo.Claims");
        }
    }
}
