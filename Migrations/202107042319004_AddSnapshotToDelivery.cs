namespace BHelp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSnapshotToDelivery : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Deliveries", "LogDate", c => c.DateTime(nullable: false, storeType: "date"));
            AddColumn("dbo.Deliveries", "FirstName", c => c.String());
            AddColumn("dbo.Deliveries", "LastName", c => c.String());
            AddColumn("dbo.Deliveries", "StreetNumber", c => c.String());
            AddColumn("dbo.Deliveries", "StreetName", c => c.String());
            AddColumn("dbo.Deliveries", "City", c => c.String());
            AddColumn("dbo.Deliveries", "Phone", c => c.String());
            AddColumn("dbo.Deliveries", "NamesAgesInHH", c => c.String());
            AddColumn("dbo.Deliveries", "Completed", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Deliveries", "Completed");
            DropColumn("dbo.Deliveries", "NamesAgesInHH");
            DropColumn("dbo.Deliveries", "Phone");
            DropColumn("dbo.Deliveries", "City");
            DropColumn("dbo.Deliveries", "StreetName");
            DropColumn("dbo.Deliveries", "StreetNumber");
            DropColumn("dbo.Deliveries", "LastName");
            DropColumn("dbo.Deliveries", "FirstName");
            DropColumn("dbo.Deliveries", "LogDate");
        }
    }
}
