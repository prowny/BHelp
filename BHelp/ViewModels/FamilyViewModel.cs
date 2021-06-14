using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;

namespace BHelp.ViewModels
{
    public class FamilyViewModel

    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public bool Active { get; set; }

        [DisplayName("First Name")]
        public string FirstName { get; set; }

        [DisplayName("Last Name")]
        public string LastName { get; set; }

        [DisplayName("Date of Birth")]
        [Column(TypeName = "Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime DateOfBirth { get; set; }

        [NotMapped]
        public int Age { get; set; }
        public List<FamilyViewModel> FamilyMembers { get; set; }
        //public virtual IEnumerable<SelectListItem> FamilyMembers { get; set; }

        public IEnumerable<SelectListItem> Clients { get; set; }
    }
}