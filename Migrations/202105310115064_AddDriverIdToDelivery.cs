namespace BHelp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddDriverIdToDelivery : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Deliveries", "DriverId", c => c.String(maxLength: 128));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Deliveries", "DriverId");
        }
    }
}
