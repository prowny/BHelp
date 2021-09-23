namespace BHelp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RetryInitial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Clients",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Active = c.Boolean(nullable: true),
                        FirstName = c.String(nullable: true),
                        LastName = c.String(nullable: true),
                        DateOfBirth = c.DateTime(nullable: true, storeType: "date"),
                        StreetNumber = c.String(nullable: true),
                        StreetName = c.String(nullable: true),
                        City = c.String(nullable: true),
                        Zip = c.String(nullable: true),
                        Phone = c.String(nullable: true),
                        Notes = c.String(nullable: true)
                })
                .PrimaryKey(t => t.Id);
            
            
        }
    }
}
