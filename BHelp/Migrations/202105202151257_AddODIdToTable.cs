namespace BHelp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddODIdToTable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Deliveries", "ODId", c => c.Int(nullable: true));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Deliveries", "ODId");
        }
    }
}
