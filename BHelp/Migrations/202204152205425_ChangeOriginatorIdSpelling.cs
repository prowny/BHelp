namespace BHelp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeOriginatorIdSpelling : DbMigration
    {
        public override void Up()
        {
            //AddColumn("dbo.Documents", "OriginatorId", c => c.String(maxLength: 128));
            //DropColumn("dbo.Documents", "OrginatorId");
        }
        
        public override void Down()
        {
            //AddColumn("dbo.Documents", "OrginatorId", c => c.String(maxLength: 128));
            //DropColumn("dbo.Documents", "OriginatorId");
        }
    }
}
