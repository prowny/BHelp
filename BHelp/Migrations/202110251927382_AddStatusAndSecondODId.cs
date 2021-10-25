namespace BHelp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddStatusAndSecondODId : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Deliveries", "Status", c => c.Int(nullable: false));
            AddColumn("dbo.Deliveries", "DeliveryDateODId", c => c.String(maxLength: 128));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Deliveries", "DeliveryDateODId");
            DropColumn("dbo.Deliveries", "Status");
        }
    }
}
