using Microsoft.AspNet.Identity.EntityFramework;
using BHelp.Models;
using System.Data.Entity;
using System.Diagnostics;

namespace BHelp.DataAccessLayer
{
    public class BHelpContext : IdentityDbContext<ApplicationUser>
    {
        public BHelpContext() : base("BHelpContext")
        {
            Database.Log = msg => Debug.Write(msg);
        }

        public DbSet<Client> Clients { get; set; }
      
        public DbSet<Login> Logins { get; set; }

        public DbSet <FamilyMember> FamilyMembers { get; set;}

        public DbSet<Delivery> Deliveries { get; set; }

        public static BHelpContext Create()
        {
            return new BHelpContext();
        }

    }
}
