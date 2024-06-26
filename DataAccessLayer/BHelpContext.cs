﻿using BHelp.Models;

namespace BHelp.DataAccessLayer
{
    public class BHelpContext : IdentityDbContext<ApplicationUser>
    {
        public static void EnableMigrations()
        {
            //Database.SetInitializer(new MigrateDatabaseToLatestVersion<BHelpContext>());
        }

        public static void DropAndCreateDatabase()
        {
            Database.SetInitializer(new DropCreateDatabaseAlways<BHelpContext>());
        }

        public BHelpContext() : base("BHelpContext")
        {
            Database.Log = msg => Debug.Write(msg);
        }

        public DbSet<Client> Clients { get; set; }
      
        public DbSet<Login> Logins { get; set; }

        public DbSet <FamilyMember> FamilyMembers { get; set;}

        public DbSet<Delivery> Deliveries { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            //modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }

        public static BHelpContext Create()
        {
            return new BHelpContext();
        }

    }

}