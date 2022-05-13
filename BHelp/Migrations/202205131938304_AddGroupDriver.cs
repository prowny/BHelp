namespace BHelp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddGroupDriver : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DriverSchedules", "GroupId", c => c.Int(nullable: true));
            AddColumn("dbo.DriverSchedules", "GroupDriverId", c => c.String(maxLength: 128));
        }
        
        public override void Down()
        {
            DropColumn("dbo.DriverSchedules", "GroupDriverId");
            DropColumn("dbo.DriverSchedules", "GroupId");
        }
    }
}
