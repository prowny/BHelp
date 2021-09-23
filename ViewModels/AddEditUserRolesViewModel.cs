using BHelp.Models;
using System.Collections.Generic;
using System.Web.Mvc;

namespace BHelp.ViewModels
{
    public class AddEditUserRolesViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public virtual IEnumerable<SelectListItem> Users { get; set; }
        public virtual IEnumerable<SelectListItem> UserRoles { get; set; }
        public virtual ApplicationUser User { get; set; }
        public virtual ApplicationRole UserRole { get; set; }
        public List<UserNameRole> UserNameRoles { get; set; }
    }
    public class UserNameRole
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public virtual string Email { get; set; }  // for use in MaintainUserRoles to notify user when activated. 
    }
}