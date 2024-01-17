using System.Collections.Generic;
using BHelp.Models;

namespace BHelp.ViewModels
{
    public sealed class BulkEmailViewModel
    {
        public List<ApplicationUser> ActiveODList { get; set; }
        public List<ApplicationUser> ActiveDriverList { get; set; }
        public List<ApplicationUser> ActiveBaggerList { get; set; }
        public List<ApplicationUser> ActiveStaffList { get; set; }
        public List<ApplicationUser> AllActiveUsersList { get; set; }

        public bool ODSelect { get; set; }
        public bool DriverSelect { get; set; }
        public bool StaffSelect { get; set; }
        public bool BaggerSelect { get; set; }
        public bool AllUsersSelect { get; set; }

        public string JsonActiveODs { get; set; }
        public string JsonActiveDrivers { get; set; }
        public string JsonActiveBaggers { get; set; }
        public string JsonActiveStaff { get; set; }
        public string JsonAllActiveUsers { get; set; }
        public string EmailString { get; set; } // list of emails concatenated with semicolon 
    }
}
