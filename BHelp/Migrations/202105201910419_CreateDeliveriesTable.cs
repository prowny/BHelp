namespace BHelp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CreateDeliveriesTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
               "dbo.Deliveries",
               c => new
               {
                   Id = c.Int(nullable: false, identity: true),
                   DeliveryDate = c.DateTime(nullable: true, storeType: "date"),
                   Notes = c.String(nullable: true),
                   FullBags=c.Int(nullable:true),
                   HalfBags = c.Int(nullable: true),
                   KidSnacks=c.Int(nullable:true),
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
