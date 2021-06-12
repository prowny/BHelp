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

        [DisplayName("Client Last Name")]
        public string LastName { get; set; }

        public DateTime DateOfBirth { get; set; }

        [DisplayName("Age")]
        public int Age { get; set; }
        
        [DisplayName("Street Number")]
        public string StreetNumber { get; set; }
        
        [DisplayName("Street Name")]
        public string StreetName { get; set; }
        
        [DisplayName("City")]
        public string City { get; set; }
        
        [DisplayName("Zip Code")]
        public string Zip { get; set; }

        public string Phone { get; set; }

        [DataType(DataType.MultilineText)]
        public string Notes { get; set; }
        
        public string FullName
        {
            get { return string.Format("{0} {1}", FirstName, LastName); }
        }
        public List<FamilyMember> FamilyMembers { get; set; }
        
        public IEnumerable<SelectListItem> HouseholdMembers { get; set; }
        
        public string SearchString { get; set; }
        
        public string CurrentUserFullName { get; set; }
        public string StreetToolTip { get; set; }
        public string CityToolTip { get; set; }
        public string PhoneToolTip { get; set; }
        public string NotesToolTip { get; set; }  // Household/Client notes
    }
}
