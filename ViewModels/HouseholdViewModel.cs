﻿using System;
using System.Collections.Generic;
using BHelp.Models;

namespace BHelp.ViewModels
{
    public class HouseholdViewModel
    {
        public int ClientId { get; set; }
        public bool Active { get; set; }

        [DisplayName("Client First Name")]
        public string FirstName { get; set; }

        [DisplayName("Client Last Name")]
        public string LastName { get; set; }

        [DisplayName("Date of Birth")]
        [Column(TypeName = "Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime DateOfBirth { get; set; }

        [DisplayName("Street No.")]
        public string StreetNumber { get; set; }
        
        [DisplayName("Street Name")]
        public string StreetName { get; set; }

        [DisplayName("City")]
        public string City { get; set; }

        [DisplayName("Zip Code")]
        public string Zip { get; set; }

        public string Phone { get; set; }

        [DisplayName("Household Notes")]
        [DataType(DataType.MultilineText)]
        public string Notes { get; set; }
        
        // For display use:
        public int Age { get; set; }

        [DisplayName("Family Members")]
        public List<SelectListItem> FamilySelectList { get; set; }  // For display onlly
        [DisplayName("Family Members")]
        public List<FamilyMember> FamilyMembers { get; set; }  // For editing
        public IEnumerable<SelectListItem> ZipCodes { get; set; }
        public string StreetToolTip { get; set; } 
        public string CityToolTip { get; set; }
        public string PhoneToolTip { get; set; }
        public string NotesToolTip { get; set; }  // Household/Client notes
        public DateTime DateLastDelivery { get; set; }
        public DateTime DateLastGiftCard { get; set; }
        public int GiftCardsThisMonth { get; set; }
        public int DeliveriesThisMonth { get; set; }
        public DateTime NextDeliveryEligibleDate { get; set; }
        public DateTime NextGiftCardEligibleDate { get; set; }
        public DateTime DesiredDeliveryDate { get; set; }
    }
}