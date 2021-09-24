namespace BHelp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RedoDeliveriesMigration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Deliveries",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ClientId = c.Int(nullable: false),
                        DeliveryDate = c.DateTime(nullable: true, storeType: "date"),
                        Notes = c.String(),
                        FullBags = c.Int(nullable: true),
                        HalfBags = c.Int(nullable: true),
                        KIdSnacks = c.Int(nullable: true),
                        GiftCards = c.Int(nullable: true)
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Deliveries");
        }
    }
}
