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

        [StringLength(128)]
        public string FirstName { get; set; }

        [StringLength(128), DisplayName("Client Last Name")]
        public string LastName { get; set; }

        [DisplayName("Date of Birth")]
        [Column(TypeName = "Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime DateOfBirth { get; set; }

        [Required,StringLength(128), DisplayName("Street Number")]
        public string StreetNumber { get; set; }

        [Required, StringLength(128)]
        [DisplayName("Street Name")]
        public string StreetName { get; set; }

        [Required,StringLength(128)]
        [DisplayName("City")]
        public string City { get; set; }

        [Required, StringLength(20)]
        [DisplayName("Zip Code")]
        public string Zip { get; set; }

        [StringLength(256)]
        public string Email { get; set; }

        [StringLength(128)]
        public string Phone { get; set; }

        [DataType(DataType.MultilineText), StringLength(4096)]
        public string Notes { get; set; }

        [DataType(DataType.Date)]
        public DateTime DateCreated { get; set; }

        [NotMapped] public string FullName
        {
            // ReSharper disable once ArrangeAccessorOwnerBody
            get { return $"{FirstName} {LastName}"; }
        }

        [NotMapped] public string LastFirstName => $"{LastName + ", "} {FirstName}";

        [NotMapped] public IEnumerable<SelectListItem> ZipCodes { get; set; }

        [NotMapped] public List<FamilyMember> FamilyMembers { get; set; }

        [NotMapped] public int HouseholdCount { get; set; }

        [NotMapped] public IEnumerable<SelectListItem> FamilySelectList { get; set; }

        [NotMapped] public string SearchString { get; set; }

        [NotMapped] public string CurrentUserFullName { get; set; }

        [NotMapped] public string StreetToolTip { get; set; }

        [NotMapped] public string CityToolTip { get; set; }

        [NotMapped] public string PhoneToolTip { get; set; }

        [NotMapped] public string NotesToolTip { get; set; }
       
        [NotMapped] public IEnumerable<Client> SearchResults { get; set; }

        [NotMapped] public int Age { get; set; }

        [NotMapped] public bool Delete { get; set; }

        [NotMapped] public string ClientNameAddress { get; set; }

        [NotMapped] public string NameAddressToolTip { get; set; }    // for open delivery filtering

        [NotMapped] public string ReturnURL { get; set; }

        [NotMapped] public bool OKtoDelete { get; set; }
    }
}