namespace BHelp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddFirstDelivery : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Deliveries", "FirstDelivery", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Deliveries", "FirstDelivery");
        }
    }
}
