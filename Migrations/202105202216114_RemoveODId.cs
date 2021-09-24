namespace BHelp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveODId : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Deliveries", "ODId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Deliveries", "ODId", c => c.Int(nullable: false));
        }
    }
}
