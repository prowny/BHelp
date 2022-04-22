namespace BHelp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddDriverScheules : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DriverSchedules",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Date = c.DateTime(nullable: false),
                        DriverId = c.String(maxLength: 128),
                        BackupDriverId = c.String(maxLength: 128),
                        Note = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.DriverSchedules");
        }
    }
}
