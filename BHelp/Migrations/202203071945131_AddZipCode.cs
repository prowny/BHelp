namespace BHelp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddZipCode : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "Zip", c => c.String(maxLength: 10));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "Zip");
        }
    }
}
