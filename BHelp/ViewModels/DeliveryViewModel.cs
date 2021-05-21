using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
        public ApplicationUser User { get; set; }
        public DateTime DateDelivered { get; set; }
        public Client Client { get; set; }
        public List<FamilyMember> FamilyMembers { get; set; }
        public List<FamilyMember> Kids { get; set; }
        public List<FamilyMember> Adults { get; set; }
        public List<FamilyMember> Seniors { get; set; }
        public DateTime DateLastDelivery { get; set; }
        public DateTime DateLastGiftCard { get; set; }
    }
}
