using System;
using System.Collections.Generic;
using BHelp.Models;

namespace BHelp.ViewModels
{
    public class BulkEmailViewModel
    {
        public List<ApplicationUser> ActiveODList { get; set; }
        public List<ApplicationUser> ODPlusInactiveList { get; set; }
        public List<ApplicationUser> ActiveDriverList { get; set; }
        public List<ApplicationUser> DriverPlusInactiveList { get; set; }
        public List<ApplicationUser> ActiveManagementList { get; set; }
        public List<ApplicationUser> ManagementPlusInactiveList { get; set; }
        public List<ApplicationUser> AllActiveUsersList { get; set; }
        public List<ApplicationUser> AllUsersPlusInactiveList { get; set; }
        public List<ApplicationUser> DonorList { get; set; }

        public bool ODSelect { get; set; }
        public bool ODIncludeInactives { get; set; }
        public bool DriverSelect { get; set; }
        public bool DriverIncludeInactives { get; set; }
        public bool ManagementSelect { get; set; }
        public bool ManagementIncludeInactives { get; set; }
        public bool AllUsersSelect { get; set; }
        public bool AllUsersIncludeInactives { get; set; }

        public string Subject { get; set; }
        public string EmailText { get; set; } 
    }
}
