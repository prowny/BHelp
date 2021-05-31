namespace BHelp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SetDeliveryDateNullable : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Deliveries", "DateDelivered", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Deliveries", "DateDelivered", c => c.DateTime(nullable: false));
        }
    }
}
