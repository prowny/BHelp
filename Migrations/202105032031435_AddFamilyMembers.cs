namespace BHelp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddFamilyMembers : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.FamilyMembers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Active = c.Boolean(nullable: true),
                        FirstName = c.String(nullable: true),
                        LastName = c.String(nullable: true),
                        DateOfBirth = c.DateTime(nullable: true, storeType: "date"),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.FamilyMembers");
        }
    }
}
