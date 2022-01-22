namespace BHelp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeNameWeekEndingDateToDate : DbMigration
    {
        public override void Up()
        {
            //AddColumn("dbo.VolunteerHours", "Date", c => c.DateTime(nullable: false));
            //DropColumn("dbo.VolunteerHours", "WeekEndingDate");
        }
        
        public override void Down()
        {
            //AddColumn("dbo.VolunteerHours", "WeekEndingDate", c => c.DateTime(nullable: false));
            //DropColumn("dbo.VolunteerHours", "Date");
        }
    }
}
