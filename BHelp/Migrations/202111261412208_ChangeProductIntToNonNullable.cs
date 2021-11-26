namespace BHelp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeProductIntToNonNullable : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Deliveries", "Children", c => c.Int(nullable: false));
            AlterColumn("dbo.Deliveries", "Adults", c => c.Int(nullable: false));
            AlterColumn("dbo.Deliveries", "Seniors", c => c.Int(nullable: false));
            AlterColumn("dbo.Deliveries", "FullBags", c => c.Int(nullable: false));
            AlterColumn("dbo.Deliveries", "HalfBags", c => c.Int(nullable: false));
            AlterColumn("dbo.Deliveries", "KidSnacks", c => c.Int(nullable: false));
            AlterColumn("dbo.Deliveries", "GiftCardsEligible", c => c.Int(nullable: false));
            AlterColumn("dbo.Deliveries", "GiftCards", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Deliveries", "GiftCards", c => c.Int());
            AlterColumn("dbo.Deliveries", "GiftCardsEligible", c => c.Int());
            AlterColumn("dbo.Deliveries", "KidSnacks", c => c.Int());
            AlterColumn("dbo.Deliveries", "HalfBags", c => c.Int());
            AlterColumn("dbo.Deliveries", "FullBags", c => c.Int());
            AlterColumn("dbo.Deliveries", "Seniors", c => c.Int());
            AlterColumn("dbo.Deliveries", "Adults", c => c.Int());
            AlterColumn("dbo.Deliveries", "Children", c => c.Int());
        }
    }
}
