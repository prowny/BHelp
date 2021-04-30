using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
    }
}