using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;
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

        [DisplayName("Street Number")]
        public string StreetNumber { get; set; }
        
        [DisplayName("Street Name")]
        public string StreetName { get; set; }

        [DisplayName("City")]
        public string City { get; set; }

        [DisplayName("Zip Code")]
        public string Zip { get; set; }

        public string Phone { get; set; }

        public string Notes { get; set; }

        public int Age { get; set; }
        public List<FamilyViewModel> FamilyMembers { get; set; }
        //public IEnumerable<FamilyMember> FamilyMembers { get; set; }
    }
}