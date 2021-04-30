using Microsoft.AspNet.Identity.EntityFramework;
using BHelp.Models;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Diagnostics;

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
      
        public DbSet<Login> Login { get; set; }

        //public DbSet<Staff> StaffMembers { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }

        public static BHelpContext Create()
        {
            return new BHelpContext();
        }

    }

}