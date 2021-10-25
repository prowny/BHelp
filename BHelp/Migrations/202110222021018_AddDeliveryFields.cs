using System;
using System.Data.Entity.Migrations;
    
    public partial class AddDeliveryFields : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Deliveries", "FirstDelivery", c => c.Boolean(nullable: false));
            AddColumn("dbo.Deliveries", "DeliveryDateODId", c => c.String(maxLength: 128));
            AddColumn("dbo.Deliveries", "Status", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Deliveries", "Status");
            DropColumn("dbo.Deliveries", "DeliveryDateODId");
            DropColumn("dbo.Deliveries", "FirstDelivery");
        }
    }
