﻿using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BHelp.Models
{
    public class FamilyMember
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public bool Active { get; set; }

        [DisplayName("First Name"), StringLength(128)]
        public string FirstName { get; set; }

        [DisplayName("Last Name"), StringLength(128)]
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