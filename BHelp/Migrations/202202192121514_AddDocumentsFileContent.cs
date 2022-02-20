namespace BHelp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddDocumentsFileContent : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Documents", "FileContent", c => c.Binary());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Documents", "FileContent");
        }
    }
}
