using System;
using System.Collections.Generic;

namespace BHelp.Models
{
    public class Delivery
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
      

        [DisplayName("Desired Delivery Date")]
        [Column(TypeName = "Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime DeliveryDate { get; set; }

        [DisplayName("Call Log Date")]
        [Column(TypeName = "Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime LogDate { get; set; }

        // Snapshot: Client Data as of the Log Date
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string StreetNumber { get; set; }
        public string StreetName { get; set; }
        public string City { get; set; }
        public string Zip { get; set; }
        public string Phone { get; set; }
        public string NamesAgesInHH { get; set; }

        public int? Children { get; set; }  // # of children in this delivery
        public int? Adults { get; set; }     // # of adults in this delivery
        public int? Seniors { get; set; }    // # of seniors in this delivery
       
        public string Notes { get; set; }  // Notes entered by drivers, baggers. etc. (as opposed to OD notes or Household notes)
        public int? FullBags { get; set; }
        public int? HalfBags { get; set; }
        public int? KidSnacks { get; set; }
        public int? GiftCardsEligible { get; set; }
        public int? GiftCards { get; set; }

        [StringLength(128)]
        public string ODId { get; set; }

        [StringLength(128)]
        public string DriverId { get; set; }

        [Column(TypeName = "Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime? DateDelivered { get; set; }

        public Boolean Completed { get; set; }
        public string ODNotes { get; set; }
        public string DriverNotes { get; set; }

        [NotMapped]
        public IEnumerable<SelectListItem> DriversList { get; set; }

        [NotMapped]
        public string ClientNameAddress { get; set; }

        [NotMapped]
        public string DateDeliveredString { get; set; }
        
        [NotMapped]
        public int HouseoldCount { get; set; }

        [NotMapped]
        public int NumberOfKids2_17 { get; set; }

        [NotMapped]
        public string DriverName { get; set; }
    }
}