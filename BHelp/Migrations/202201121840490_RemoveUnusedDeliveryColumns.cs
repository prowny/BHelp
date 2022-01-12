namespace BHelp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveUnusedDeliveryColumns : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Deliveries", "DeliveryDate");
            DropColumn("dbo.Deliveries", "Completed");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Deliveries", "Completed", c => c.Boolean(nullable: false));
            AddColumn("dbo.Deliveries", "DeliveryDate", c => c.DateTime(nullable: false, storeType: "date"));
        }
    }
}
