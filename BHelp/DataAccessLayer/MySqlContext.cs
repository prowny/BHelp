using Microsoft.AspNet.Identity.EntityFramework;
using BHelp.Models;
using System.Data.Entity;
using System.Diagnostics;
using BHelp.ViewModels;

namespace BHelp.DataAccessLayer
{
    public class MySqlContext : IdentityDbContext<ApplicationUser>
    {
        public MySqlContext() : base("MySqlContext")
        {
            Database.Log = msg => Debug.Write(msg);
        }

        public DbSet<Client> Clients { get; set; }
        public DbSet<Login> Logins { get; set; }
        public DbSet<FamilyMember> FamilyMembers { get; set; }
        public DbSet<Delivery> Deliveries { get; set; }
        public DbSet<MySqlViewModel> Test { get; set; }
        public static MySqlContext Create()
        {
            return new MySqlContext();
        }

    }
}