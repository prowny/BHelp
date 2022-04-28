namespace BHelp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddODSchedule : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ODSchedules",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Date = c.DateTime(nullable: false),
                        ODId = c.String(maxLength: 128),
                        Note = c.String(),
                    })
                .PrimaryKey(t => t.Id);
        }
        
        public override void Down()
        {
            DropTable("dbo.ODSchedules");
        }
    }
}
