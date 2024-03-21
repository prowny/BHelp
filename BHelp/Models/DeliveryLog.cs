using System;
using System.ComponentModel.DataAnnotations;

namespace BHelp.Models
{  // Added 03/21/2024 by PER to track changes to deliveries
    public class DeliveryLog
    {
        public int Id { get; set; }
        public int DeliveryId { get; set; }

        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy hh:mm tt}")]
        public DateTime LogDate { get; set; }

        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy hh:mm tt}")]
        public DateTime DateModified { get; set; }

        [StringLength(128)] public string ModifiedBy { get; set; } // User.Name
        [StringLength(128)] public string ActionSource { get; set; } // Create, Edit, or Filters

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime? DateDelivered { get; set; }
        [StringLength(128)] public string LogODId { get; set; }
        [StringLength(128)] public string DeliveryODId { get; set; }
        [StringLength(128)] public string DriverId { get; set; }
        public int ClientId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Status { get; set; }  // 0 = Open, 1 = Completed, 2 = Undelivered
    }
}