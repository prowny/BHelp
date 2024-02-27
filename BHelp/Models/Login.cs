using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BHelp.Models

{
    public class Login
    {
        public int Id { get; set; }

        [StringLength(128)]
        public string UserName { get; set; }

        [DisplayName("First Name"), StringLength(128)]
        public string FirstName { get; set; }

        [DisplayName("Last Name"), StringLength(128)]
        public string LastName { get; set; }

        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy hh:mm tt}")]
        public DateTime DateTime { get; set; }

        [StringLength(16)]
        public string Status { get; set; }
    }
}