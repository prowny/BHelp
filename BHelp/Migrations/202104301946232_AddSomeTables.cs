namespace BHelp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSomeTables : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Client", "LocationID_Id", "dbo.Location");
            DropIndex("dbo.Client", new[] { "LocationID_Id" });
            AddColumn("dbo.Client", "Active", c => c.Boolean(nullable: false));
            AddColumn("dbo.Client", "StreetNumber", c => c.String(nullable: false));
            AddColumn("dbo.Client", "StreetName", c => c.String(nullable: false));
            AddColumn("dbo.Client", "City", c => c.String(nullable: false));
            AddColumn("dbo.Client", "Zip", c => c.String(nullable: false));
            AddColumn("dbo.Client", "Phone", c => c.String());
            AddColumn("dbo.Client", "Notes", c => c.String());
            AddColumn("dbo.AspNetUsers", "Active", c => c.Boolean(nullable: false));
            DropColumn("dbo.Client", "LocationID_Id");
            DropTable("dbo.Location");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.Location",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        StreetNumber = c.String(nullable: false),
                        StreetName = c.String(nullable: false),
                        Address2 = c.String(),
                        City = c.String(nullable: false),
                        Zip = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Client", "LocationID_Id", c => c.Int());
            DropColumn("dbo.AspNetUsers", "Active");
            DropColumn("dbo.Client", "Notes");
            DropColumn("dbo.Client", "Phone");
            DropColumn("dbo.Client", "Zip");
            DropColumn("dbo.Client", "City");
            DropColumn("dbo.Client", "StreetName");
            DropColumn("dbo.Client", "StreetNumber");
            DropColumn("dbo.Client", "Active");
            CreateIndex("dbo.Client", "LocationID_Id");
            AddForeignKey("dbo.Client", "LocationID_Id", "dbo.Location", "Id");
        }
    }
}
