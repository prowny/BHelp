namespace BHelp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeDeliveryDateType : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Deliveries", "DateDelivered", c => c.DateTime(storeType: "date"));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Deliveries", "DateDelivered", c => c.DateTime());
        }
    }
}
