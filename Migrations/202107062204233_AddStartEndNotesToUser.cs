namespace BHelp.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class AddStartEndNotesToUser : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "BeginDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.AspNetUsers", "LastDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.AspNetUsers", "Notes", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "Notes");
            DropColumn("dbo.AspNetUsers", "LastDate");
            DropColumn("dbo.AspNetUsers", "BeginDate");
        }
    }
}
