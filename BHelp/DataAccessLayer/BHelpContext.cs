using System;
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

        public DbSet<BaggerSchedule> BaggerSchedules { get; set; }

        public DbSet<Holiday> Holidays { get; set; }

        public DbSet<AddressCheck> AddressChecks { get; set; }

        public DbSet <AssistancePayment> AssistancePayments { get; set; }

        public void SetDeliveryStatus(int id, int status)
        {
            var rec = this.Deliveries.Find(id);
            if (rec != null)
            {
                rec.Status = status;
                this.SaveChanges();
            }
        }

        public void SetDeliveryDate(int id, DateTime dateDelivered, string usrName)
        {
            var rec = this.Deliveries.Find(id);
            if (rec != null)
            {
                rec.DateModified = DateTime.Now;
                rec.ModifiedBy = usrName; 
                rec.OldDateDelivered = rec.DateDelivered;

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

        internal object SqlQuery<T>(T sqlString)
        {
            throw new NotImplementedException();
        }
    }
}
