namespace BHelp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddZipToDelivery : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Deliveries", "Zip", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Deliveries", "Zip");
        }
    }
}
