namespace BHelp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddVolunteerHours : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "VolunteerCategory", c => c.String(maxLength: 1));
            AddColumn("dbo.AspNetUsers", "VolunteerSubcategory", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "VolunteerSubcategory");
            DropColumn("dbo.AspNetUsers", "VolunteerCategory");
        }
    }
}
