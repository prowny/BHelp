namespace BHelp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddDateDelivered : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Deliveries", "DateDelivered", c => c.DateTime());
            AlterColumn("dbo.Deliveries", "FullBags", c => c.Int(nullable:true));
            AlterColumn("dbo.Deliveries", "HalfBags", c => c.Int(nullable: true));
            AlterColumn("dbo.Deliveries", "KIdSnacks", c => c.Int(nullable: true));
            AlterColumn("dbo.Deliveries", "GiftCards", c => c.Int(nullable: true));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Deliveries", "GiftCards", c => c.Int(nullable: false));
            AlterColumn("dbo.Deliveries", "KIdSnacks", c => c.Int(nullable: false));
            AlterColumn("dbo.Deliveries", "HalfBags", c => c.Int(nullable: false));
            AlterColumn("dbo.Deliveries", "FullBags", c => c.Int(nullable: false));
            DropColumn("dbo.Deliveries", "DateDelivered");
        }
    }
}
