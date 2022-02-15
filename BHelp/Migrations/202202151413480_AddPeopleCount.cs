namespace BHelp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPeopleCount : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.VolunteerHours", "PeopleCount", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.VolunteerHours", "PeopleCount");
        }
    }
}
