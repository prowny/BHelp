using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BHelp.Models
{
    public class Delivery
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public int? Children { get; set; }  // # of children in this delivery
        public int? Adults { get; set; }     // # of adults in this delivery
        public int? Seniors { get; set; }    // # of senoprs in this delivery

        [DisplayName("Delivery Date")]
        [Column(TypeName = "Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime DeliveryDate { get; set; }

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

        public DateTime? DateDelivered { get; set; }
        public string ODNotes { get; set; }
        public string DriverNotes { get; set; }
    }
}