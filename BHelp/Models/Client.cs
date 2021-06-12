using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;

namespace BHelp.Models
{
    public class Client
    {
        public int Id { get; set; }
        public bool Active { get; set; }

        [DisplayName("Client First Name")]
        public string FirstName { get; set; }

        [DisplayName("Client Last Name")]
        public string LastName { get; set; }

        [DisplayName("Date of Birth")]
        [Column(TypeName = "Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime DateOfBirth { get; set; }

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
        [DisplayName("Zip Code")]
        public string Zip { get; set; }

        public string Phone { get; set; }

        public string Notes { get; set; }

        [NotMapped]
        public string FullName
        {
            get { return string.Format("{0} {1}", FirstName, LastName); }
        }
        [NotMapped]
        public List<FamilyMember> FamilyMembers { get; set; }

        [NotMapped]
        public IEnumerable<SelectListItem> FamilySelectList { get; set; }

        [NotMapped]
        public string SearchString { get; set; }

        [NotMapped]
        public string CurrentUserFullName { get; set; }
        [NotMapped]
        public string StreetToolTip { get; set; }
        [NotMapped]
        public string CityToolTip { get; set; }
        [NotMapped]
        public string PhoneToolTip { get; set; }
        [NotMapped]
        public string NotesToolTip { get; set; }
       
        [NotMapped]
        public IEnumerable<Client> SearchResults { get; set; }

        [NotMapped]
        public int Age { get; set; }
    }
}