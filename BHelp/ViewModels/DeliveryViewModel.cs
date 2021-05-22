using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;
using BHelp.Models;

namespace BHelp.ViewModels
{
    public class DeliveryViewModel
    {
        public int Id { get; set; }
        public int ClientId { get; set; }

        [DisplayName("Delivery Date")]
        [Column(TypeName = "Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime DeliveryDate { get; set; }

        public string Notes { get; set; }
        public int GiftCardsEligible { get; set; }
        public int FullBags { get; set; }
        public int HalfBags { get; set; }
        public int KIdSnacks { get; set; }
        public int GiftCards { get; set; }

        [DisplayName("OD Name")]
        public ApplicationUser User { get; set; }
        
        public Client Client { get; set; }
        public DateTime DateDelivered { get; set; }
       
        public List<FamilyMember> FamilyMembers { get; set; }
        public List<SelectListItem> FamilySelectList { get; set; }  // For display onlly
        public List<FamilyMember> Kids { get; set; }
        public int KidsCount { get; set; }
        public List<FamilyMember> Adults { get; set; }
        public int AdultsCount { get; set; }
        public List<FamilyMember> Seniors { get; set; }
        public int SeniorsCount { get; set; }
        public DateTime DateLastDelivery { get; set; }
        public DateTime DateLastGiftCard { get; set; }
        public string DriverId { get; set; }
        public string DriverName { get; set; }
        public string DriverNotes { get; set; }
    }
}
