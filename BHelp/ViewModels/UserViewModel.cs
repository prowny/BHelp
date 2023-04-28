using System.Collections;
using System.Collections.Generic;
using BHelp.Models;

namespace BHelp.ViewModels
{
    public class UserViewModel
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string PhoneNumber2 { get; set; }
        public List<ApplicationUser> UserList { get; set; }

    }
}