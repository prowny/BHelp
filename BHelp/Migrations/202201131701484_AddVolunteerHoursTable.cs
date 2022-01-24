namespace BHelp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddVolunteerHoursTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.VolunteerHours",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(maxLength: 128),
                        OriginatorUserId = c.String(maxLength: 128),
                        Category = c.String(maxLength: 1),
                        Subcategory = c.String(),
                        Date = c.DateTime(nullable: false),
                        Hours = c.Int(nullable: false),
                        Minutes = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.VolunteerHours");
        }
    }
}
