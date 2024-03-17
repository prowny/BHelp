using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;

namespace BHelp.Models
{
    public class Delivery
    {
        public int Id { get; set; }
        public int ClientId { get; set; }

        [DisplayName("Call Log Date")]
        [Column(TypeName = "Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime LogDate { get; set; }

        // Snapshot: Client Data as of the Log Date
        [StringLength(128)]
        public string FirstName { get; set; }

        [StringLength(128)]
        public string LastName { get; set; }

        [StringLength(128)]
        public string StreetNumber { get; set; }

        [StringLength(128)]
        public string StreetName { get; set; }

        [StringLength(128)]
        public string City { get; set; }

        [StringLength(20)]
        public string Zip { get; set; }

        [StringLength(128)]
        public string Phone { get; set; }

        [StringLength(2048)]
        public string NamesAgesInHH { get; set; }

        public int Children { get; set; }  // # of children in this delivery
        public int Adults { get; set; }     // # of adults in this delivery
        public int Seniors { get; set; }    // # of seniors in this delivery

        [StringLength(4096)]
        public string Notes { get; set; }  // Notes entered by drivers, baggers. etc. (as opposed to OD notes or Household notes)
        public int FullBags { get; set; }
        public int HalfBags { get; set; }
        public int KidSnacks { get; set; }
        public int GiftCardsEligible { get; set; }
        public int GiftCards { get; set; }
        public int HolidayGiftCards { get; set; }

        [StringLength(128)]
        public string ODId { get; set; }

        [StringLength(128)]
        public string DriverId { get; set; }

        [Column(TypeName = "Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime? DateDelivered { get; set; }

        [StringLength(4096)]
        public string ODNotes { get; set; }

        [DataType(DataType.MultilineText), StringLength(4096)]
        public string DriverNotes { get; set; }
        public bool FirstDelivery { get; set; }
        public int Status { get; set; }  // 0 = Open, 1 = Completed, 2 = Undelivered

        public DateTime? DateModified { get; set; }  // addedPER 03/16/2024

        [StringLength(128)]
        public string ModifiedBy { get; set; }  // User.Identity.Name - addedPER 03/16/2024
        public DateTime? OldDateDelivered { get; set; }  // addedPER 03/16/2024


        [NotMapped]
        public string ODName { get; set; }

        [StringLength(128)]
        public string DeliveryDateODId { get; set; }

        [NotMapped]
        public string DriverName { get; set; }

        [NotMapped]
        public string DeliveryDateODName { get; set; }

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
        public string SelectedStatus { get; set; }

        [NotMapped]
        public string LogDateString { get; set; }
        
        [NotMapped]
        public bool IsChecked { get; set; }    // for open delivery filtering

        [NotMapped]
        public Client Client { get; set; }  // for open delivery filtering

        [NotMapped]
        public bool AllZeroProducts { get; set; }

        [NotMapped]
        public bool EligiibilityRulesException { get; set; }

        [NotMapped]
        public int PoundsOfFood { get; set; }

        [NotMapped]
        public string ReturnURL { get; set; }

        [NotMapped]
        public List<Client> Clients { get; set; }
    }
}