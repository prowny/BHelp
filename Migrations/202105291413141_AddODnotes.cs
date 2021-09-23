namespace BHelp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddODnotes : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Deliveries", "ODNotes", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Deliveries", "ODNotes");
        }
    }
}
