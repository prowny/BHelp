using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BHelp.Models

{
    public class Login
    {
        public int Id { get; set; }
        public string UserName { get; set; }

        [DisplayName("First Name")]
        public string FirstName { get; set; }

        [DisplayName("Last Name")]
        public string LastName { get; set; }

        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy hh:mm tt}")]
        public DateTime DateTime { get; set; }

        public string Status { get; set; }
    }
}