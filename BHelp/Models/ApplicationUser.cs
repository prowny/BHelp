using System.ComponentModel;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace BHelp.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ApplicationUser : IdentityUser
    {
        [JsonProperty, DisplayName("Active")]
        public bool Active { get; set; }    // Added PER 04/30/2021

        [JsonProperty, DisplayName("Begin Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime BeginDate { get; set; }    // Added PER 07/06/2021

        [JsonProperty, DisplayName("Last Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime LastDate { get; set; }    // Added PER 07/06/2021

        [JsonProperty, DisplayName("Notes")]
        public String Notes { get; set; }    // Added PER 07/06/2021

        [JsonProperty, StringLength(1), DisplayName("Volunteer Hours Category")]
        public string VolunteerCategory { get; set; }    // Added PER 01/13/2022

        [JsonProperty, DisplayName("Volunteer Hours Subcategory")]
        public string VolunteerSubcategory { get; set; }     // Added PER 01/13/2022

        [JsonProperty, DisplayName("First Name")]
        public string FirstName { get; set; }

        [JsonProperty, DisplayName("Last Name")]
        public string LastName { get; set; }

        [DisplayName("Title")]
        public string Title { get; set; }
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }

        [JsonProperty]
        public override string Email
        {
            get
            {
                return base.Email;
            }
            set
            {
                base.Email = value;
            }
        }
        [JsonProperty]
        public override string Id
        {
            get
            {
                return base.Id;
            }
            set
            {
                base.Id = value;
            }
        }
        [JsonProperty]
        public override string UserName
        {
            get
            {
                return base.UserName;
            }
            set
            {
                base.UserName = value;
            }
        }
        [JsonProperty]
        public override string PhoneNumber
        {
            get
            {
                return base.PhoneNumber;
            }
            set
            {
                base.PhoneNumber = value;
            }
        }
        
        [NotMapped]
        public string FullName
        {
            get { return string.Format("{0} {1}", FirstName, LastName); }
            set => throw new NotImplementedException();
        }
        [NotMapped] public List<SelectListItem> VolunteerCategories { get; set; }
        [NotMapped] public List<SelectListItem> VolunteerSubcategories { get; set; }
        [NotMapped] public bool ShowAdminDocumentsMenu { get; set; }
        [NotMapped] public bool ShowStaffDocumentsMenu { get; set; }
        [NotMapped] public bool ShowODDocumentsMenu { get; set; }
        [NotMapped] public bool ShowDriverDocumentsMenu { get; set; }
        [NotMapped] public bool ShowEveryoneDocumentsMenu { get; set; }
        [NotMapped] public bool ShowReportsDocumentsMenu { get; set; }
        [NotMapped] public bool ShowPantryCoordinatorDocumentsMenu { get; set; }
    }
}