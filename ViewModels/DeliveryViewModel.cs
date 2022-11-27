using System;
using System.Collections.Generic;
using BHelp.Models;

namespace BHelp.ViewModels
{
    public class DeliveryViewModel
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string ODId { get; set; }

        [DisplayName("Call Log Date")]
        [Column(TypeName = "Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime LogDate { get; set; }

        [DisplayName("Desired Delivery Date")]
        [Column(TypeName = "Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime DeliveryDate { get; set; }

        [DataType(DataType.MultilineText)]
        public string Notes { get; set; }   // Client/Household Notes

        [DataType(DataType.MultilineText)]
        public string ODNotes { get; set; }

        [DataType(DataType.MultilineText)]
        public string DriverNotes { get; set; }

        [Range(0, 20)]
        public int GiftCardsEligible { get; set; }

        [Range(0,20)]
        public int FullBags { get; set; }

        [Range(0, 20)]
        public int HalfBags { get; set; }

        [Range(0, 20)]
        public int KidSnacks { get; set; }

        [Range(0, 20)]
        public int GiftCards { get; set; }

        [DisplayName("OD Name")]
        public string ODName { get; set; }
        
        public Client Client { get; set; }

        [Column(TypeName = "Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime? DateDelivered { get; set; }
        public string DateDeliveredString { get; set; }
        public DateTime DatePriorDelivery { get; set; }
        public Boolean Completed { get; set; }
        public List<FamilyMember> FamilyMembers { get; set; }
        public List<SelectListItem> FamilySelectList { get; set; }  // For display onlly
        public string NamesAgesInHH { get; set; }
        public List<FamilyMember> Kids { get; set; }
        public int KidsCount { get; set; }
        public List<FamilyMember> Adults { get; set; }
        public int AdultsCount { get; set; }
        public List<FamilyMember> Seniors { get; set; }
        public int SeniorsCount { get; set; }
        public int HouseholdCount { get; set; }
        public DateTime? DateLastDelivery { get; set; }
        public DateTime DateLastGiftCard { get; set; }
        public int GiftCardsThisMonth { get; set; }
        public string DriverId { get; set; }
        public string DriverName { get; set; }

        //For Delivery List display:
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public  string FullName { get; set; }
        public string StreetNumber { get; set; }
        public string StreetName { get; set; }
        public string City { get; set; }
        public string Zip { get; set; }
        public string Phone { get; set; }
        public string StreetToolTip { get; set; }
        public string CityToolTip { get; set; }
        public string PhoneToolTip { get; set; }
        public string NotesToolTip { get; set; }  // Household/Client notes
        public string ODNotesToolTip { get; set; }  // OD notes
        public string DriverNotesToolTip { get; set; }  // Driver notes
        public string ClientNameAddress { get; set; }
        public IEnumerable<SelectListItem> DriversList { get; set; }
        public IEnumerable<SelectListItem> ODList { get; set; }
        public List<Delivery> DeliveryList { get; set; }
        public IEnumerable<SelectListItem> DeliveriesList { get; set; }
        public IEnumerable<SelectListItem> ClientSelectList { get; set; }
        public DateTime HistoryStartDate { get; set; }
        public DateTime HistoryEndDate { get; set; }
        public  string ReturnURL { get; set; }
    }
}
