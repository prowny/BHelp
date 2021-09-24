namespace BHelp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAgeCategoryCountsToDelivery : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Deliveries", "Children", c => c.Int());
            AddColumn("dbo.Deliveries", "Adults", c => c.Int());
            AddColumn("dbo.Deliveries", "Seniors", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Deliveries", "Seniors");
            DropColumn("dbo.Deliveries", "Adults");
            DropColumn("dbo.Deliveries", "Children");
        }
    }
}
