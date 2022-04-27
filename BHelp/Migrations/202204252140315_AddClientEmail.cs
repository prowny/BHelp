namespace BHelp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddClientEmail : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Clients", "Email", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Clients", "Email");
        }
    }
}
