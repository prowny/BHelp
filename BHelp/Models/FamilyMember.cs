using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;

namespace BHelp.Models
{
    public class FamilyMember
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

        [NotMapped]
        public string NameAge { get; set; }

        [NotMapped]
        public bool Delete { get; set; }
    }
}