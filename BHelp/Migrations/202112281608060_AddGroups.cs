namespace BHelp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddGroups : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Clients",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Active = c.Boolean(nullable: false),
                    FirstName = c.String(),
                    LastName = c.String(),
                    DateOfBirth = c.DateTime(nullable: false, storeType: "date"),
                    StreetNumber = c.String(nullable: false),
                    StreetName = c.String(nullable: false),
                    City = c.String(nullable: false),
                    Zip = c.String(nullable: false),
                    Phone = c.String(),
                    Notes = c.String(),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.Deliveries",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    ClientId = c.Int(nullable: false),
                    DeliveryDate = c.DateTime(nullable: false, storeType: "date"),
                    LogDate = c.DateTime(nullable: false, storeType: "date"),
                    FirstName = c.String(),
                    LastName = c.String(),
                    StreetNumber = c.String(),
                    StreetName = c.String(),
                    City = c.String(),
                    Zip = c.String(),
                    Phone = c.String(),
                    NamesAgesInHH = c.String(),
                    Children = c.Int(nullable: false),
                    Adults = c.Int(nullable: false),
                    Seniors = c.Int(nullable: false),
                    Notes = c.String(),
                    FullBags = c.Int(nullable: false),
                    HalfBags = c.Int(nullable: false),
                    KidSnacks = c.Int(nullable: false),
                    GiftCardsEligible = c.Int(nullable: false),
                    GiftCards = c.Int(nullable: false),
                    ODId = c.String(maxLength: 128),
                    DriverId = c.String(maxLength: 128),
                    DateDelivered = c.DateTime(storeType: "date"),
                    Completed = c.Boolean(nullable: false),
                    ODNotes = c.String(),
                    DriverNotes = c.String(),
                    FirstDelivery = c.Boolean(nullable: false),
                    Status = c.Int(nullable: false),
                    DeliveryDateODId = c.String(maxLength: 128),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.FamilyMembers",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    ClientId = c.Int(nullable: false),
                    Active = c.Boolean(nullable: false),
                    FirstName = c.String(),
                    LastName = c.String(),
                    DateOfBirth = c.DateTime(nullable: false, storeType: "date"),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.GroupMembers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        NameId = c.Int(nullable: false),
                        ClientId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.GroupNames",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.Logins",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    UserName = c.String(),
                    FirstName = c.String(),
                    LastName = c.String(),
                    DateTime = c.DateTime(nullable: false),
                    Status = c.String(),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.AspNetRoles",
                c => new
                {
                    Id = c.String(nullable: false, maxLength: 128),
                    Name = c.String(nullable: false, maxLength: 256),
                    Discriminator = c.String(nullable: false, maxLength: 128),
                })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");

            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                {
                    UserId = c.String(nullable: false, maxLength: 128),
                    RoleId = c.String(nullable: false, maxLength: 128),
                })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);

            CreateTable(
                "dbo.AspNetUsers",
                c => new
                {
                    Id = c.String(nullable: false, maxLength: 128),
                    Active = c.Boolean(nullable: false),
                    BeginDate = c.DateTime(nullable: false),
                    LastDate = c.DateTime(nullable: false),
                    Notes = c.String(),
                    FirstName = c.String(),
                    LastName = c.String(),
                    Title = c.String(),
                    Email = c.String(maxLength: 256),
                    UserName = c.String(nullable: false, maxLength: 256),
                    PhoneNumber = c.String(),
                    EmailConfirmed = c.Boolean(nullable: false),
                    PasswordHash = c.String(),
                    SecurityStamp = c.String(),
                    PhoneNumberConfirmed = c.Boolean(nullable: false),
                    TwoFactorEnabled = c.Boolean(nullable: false),
                    LockoutEndDateUtc = c.DateTime(),
                    LockoutEnabled = c.Boolean(nullable: false),
                    AccessFailedCount = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");

            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    UserId = c.String(nullable: false, maxLength: 128),
                    ClaimType = c.String(),
                    ClaimValue = c.String(),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);

            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                {
                    LoginProvider = c.String(nullable: false, maxLength: 128),
                    ProviderKey = c.String(nullable: false, maxLength: 128),
                    UserId = c.String(nullable: false, maxLength: 128),
                })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);

        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.Logins");
            DropTable("dbo.GroupNames");
            DropTable("dbo.GroupMembers");
            DropTable("dbo.FamilyMembers");
            DropTable("dbo.Deliveries");
            DropTable("dbo.Clients");
        }
    }
}
