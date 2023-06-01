using System.Collections.Generic;
using System.Web.Mvc;
using BHelp.Models;
using BHelp.ViewModels;

namespace BHelp.Controllers
{
    public class BulkEmailController : Controller
    {
        // GET: BulkEmail
        public ActionResult Index()
        {
            var AllUsersPlusInactiveList = AppRoutines.GetAllUserList();
            var view = new BulkEmailViewModel
            {
                DriverPlusInactiveList = GetMasterList("Driver",
                    AllUsersPlusInactiveList, false),
                ActiveDriverList = GetMasterList("Driver",
                    AllUsersPlusInactiveList, true),

                ODPlusInactiveList = GetMasterList("OfficerOfTheDay",
                    AllUsersPlusInactiveList, false),
                ActiveODList = GetMasterList("OfficerOfTheDay",
                    AllUsersPlusInactiveList, true),

                ManagementPlusInactiveList = GetMasterList("Management",
                    AllUsersPlusInactiveList, false),
                ActiveManagementList = GetMasterList("Management",
                    AllUsersPlusInactiveList, true),

                AllUsersPlusInactiveList = AllUsersPlusInactiveList,
                AllActiveUsersList = new List<ApplicationUser>(), 
                Subject = "BHELP "
            };

            foreach (var _user in AllUsersPlusInactiveList)
            {
                if(_user.Active) view.AllActiveUsersList.Add(_user);
            }
            return View(view);
        }

        private static List<ApplicationUser> GetMasterList(string roleName, 
            List<ApplicationUser> users, bool activeOnly)
        {
            var roleId = AppRoutines.GetRoleId(roleName);
            var listUserIdsInRole = AppRoutines.GetUserIdsInRole(roleId);
            var listMaster = new List<ApplicationUser>();
            foreach (var _user in users)
            {
                foreach (var userIdInRole in listUserIdsInRole)
                {
                    if (userIdInRole == _user.Id)
                    {
                        var addUsr = new ApplicationUser
                        {
                            Id = _user.Id,
                            Active = _user.Active,
                            FirstName = _user.FirstName,
                            LastName = _user.LastName,
                            Email = _user.Email,
                            PhoneNumber = _user.PhoneNumber,
                            PhoneNumber2 = _user.PhoneNumber2
                        };
                        if (activeOnly && addUsr.Active)
                        {
                            listMaster.Add(addUsr);
                        }
                        if(!activeOnly)
                        {
                            listMaster.Add(addUsr);
                        }
                    }
                }
            }
            return listMaster;
        }
    }
}