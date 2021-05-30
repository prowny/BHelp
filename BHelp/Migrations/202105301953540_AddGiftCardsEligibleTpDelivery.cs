namespace BHelp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddGiftCardsEligibleTpDelivery : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Deliveries", "GiftCardsEligible", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Deliveries", "GiftCardsEligible");
        }
    }
}
