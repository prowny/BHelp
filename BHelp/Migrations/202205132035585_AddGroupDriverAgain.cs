namespace BHelp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddGroupDriverAgain : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.DriverSchedules", "GroupId", c => c.Int());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.DriverSchedules", "GroupId", c => c.Int(nullable: false));
        }
    }
}
