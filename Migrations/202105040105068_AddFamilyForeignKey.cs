namespace BHelp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddFamilyForeignKey : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FamilyMembers", "ClientId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.FamilyMembers", "ClientId");
        }
    }
}
