using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using BHelp.Models;

namespace BHelp.ViewModels
{
    public class ClientViewModel
    {
        public int Id { get; set; }
        public bool Active { get; set; }

        [DisplayName("Client First Name")]
        public string FirstName { get; set; }

        [Required]
        [DisplayName("Client Last Name")]
        public string LastName { get; set; }

        public DateTime DateOfBirth { get; set; }

        [DisplayName("Age")]
        public int Age { get; set; }
        
        [Required]
        [DisplayName("Street Number")]
        public string StreetNumber { get; set; }
        
        [Required]
        [DisplayName("Street Name")]
        public string StreetName { get; set; }
        
       
        [Required]
        [DisplayName("City")]
        public string City { get; set; }

        [Required]
        [EnforceTrue(ErrorMessage = @"Error Message")]
        [DisplayName("Not On List or OK'd by Supervisor")]
        public bool AddressOK { get; set; }

        [Required]
        [DisplayName("Zip Code")]
        public string Zip { get; set; }

        public string Phone { get; set; }
        public string Email { get; set; }

        [DisplayName("Permanent Notes")]
        [DataType(DataType.MultilineText)]
        public string Notes { get; set; }

        public IEnumerable<SelectListItem> ZipCodes { get; set; }
        
        public List<FamilyMember> FamilyMembers { get; set; }

        [DisplayName("Address Check List")]
        public IEnumerable<SelectListItem> AddressCheckSelectList { get; set; }
        public bool AddressCheckListExists { get; set; }
        public string AddressCheck { get; set; }  // dummy value for DropDownListFor 

        public string StreetToolTip { get; set; }
        public string CityToolTip { get; set; }
        public string PhoneToolTip { get; set; }
        public string EmailToolTip { get; set; }
        public string NotesToolTip { get; set; }  // Household/Client notes
        public string ReportTitle { get; set; }
        public string[,] ClientStrings { get; set; }
        public int ClientCount { get; set; }
        public string ReturnURL { get; set; }
    }
}
