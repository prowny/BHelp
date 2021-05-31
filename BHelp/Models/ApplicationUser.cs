using System.ComponentModel;
//using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace BHelp.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ApplicationUser : IdentityUser
    {
        [JsonProperty, DisplayName("Active")]
        public bool Active { get; set; }    // Added PER 04/30/2021

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
        //[InverseProperty("PrimaryTutor")]
        //public virtual ICollection<Student> PrimaryTutees { get; set; }

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

        //// For use by TutorSchedule update routines:
        //[NotMapped]
        //public List<TutorSchedule> TutorSchedules { get; set; }

        [NotMapped]
        public string FullName
        {
            get { return string.Format("{0} {1}", FirstName, LastName); }
            set => throw new System.NotImplementedException();
        }
    }
}