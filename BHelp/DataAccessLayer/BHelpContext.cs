using System;
using Microsoft.AspNet.Identity.EntityFramework;
using BHelp.Models;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using Castle.Core.Internal;

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

        public DbSet<FamilyMember> FamilyMembers { get; set; }

        public DbSet<Delivery> Deliveries { get; set; }

        public static BHelpContext Create()
        {
            return new BHelpContext();
        }

        public DbSet<GroupName> GroupNames { get; set; }

        public DbSet<GroupMember> GroupMembers { get; set; }

        public DbSet<VolunteerHours> VolunteerHours { get; set; }

        public DbSet<Document> Documents { get; set; }

        public DbSet<DriverSchedule> DriverSchedules { get; set; }

        public DbSet<ODSchedule> ODSchedules { get; set; }

        public DbSet<Holiday> Holidays { get; set; }

        public DbSet<AddressCheck> AddressChecks { get; set; }

        public string GetStringAllRolesForUser(string userId)
        {

            var sqlString = "SELECT Name from AspNetUserRoles "
                            + "LEFT JOIN AspNetRoles ON AspNetRoles.Id = AspNetUserRoles.RoleId "
                            + "WHERE AspNetUserRoles.UserId =  '" + userId + "'";

            var isEmpty = this.Database.SqlQuery<string>(sqlString).IsNullOrEmpty();
            var roleNameString = "";
            if (!isEmpty)
            {
                var roleNameList = this.Database.SqlQuery<string>(sqlString).ToList();
                foreach (var roleName in roleNameList)
                {
                    roleNameString = roleNameString + roleName + " ";
                }
            }

            if (roleNameString.Length > 0)
            {
                return roleNameString.Substring(0, roleNameString.Length - 1);
            }

            return roleNameString;
        }

        public void SetDeliveryStatus(int id, int status)
        {
            var rec = this.Deliveries.Find(id);
            if (rec != null)
            {
                rec.Status = status;
                this.SaveChanges();
            }
        }

        public void SetDeliveryDate(int id, DateTime dateDelivered)
        {
            var rec = this.Deliveries.Find(id);
            if (rec != null)
            {
                rec.DateDelivered = dateDelivered;
                this.SaveChanges();
            }
        }

        public void SetDeliveryDriver(int id, string driverId)
        {
            var rec = this.Deliveries.Find(id);
            if (rec != null)
            {
                rec.DriverId = driverId;
                this.SaveChanges();
            }
        }
        public void SetDeliveryDateODId(int id, string odId)
        {
            var rec = this.Deliveries.Find(id);
            if (rec != null)
            {
                rec.DeliveryDateODId = odId;
                this.SaveChanges();
            }
        }
    }
}
