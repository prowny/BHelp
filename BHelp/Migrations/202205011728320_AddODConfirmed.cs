namespace BHelp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddODConfirmed : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ODSchedules", "ODConfirmed", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ODSchedules", "ODConfirmed");
        }
    }
}
