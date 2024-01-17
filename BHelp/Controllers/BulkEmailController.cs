using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using BHelp.Models;
using BHelp.ViewModels;
using System.Text.Json;
using BHelp.DataAccessLayer;

namespace BHelp.Controllers
{
    public class BulkEmailController : Controller
    {
        // GET: BulkEmail
        public ActionResult Index()
        {
            using var _db = new BHelpContext();
            var allActiveUsersList = _db.Users
                .Where(u => u.Active).OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName).ToList();

            var view = new BulkEmailViewModel
            {
                ActiveDriverList = GetMasterList("Driver",
                    allActiveUsersList, true),

                ActiveBaggerList = GetMasterList("Bagger",
                    allActiveUsersList, true),

                ActiveODList = GetMasterList("OfficerOfTheDay",
                    allActiveUsersList, true),

                ActiveStaffList = GetMasterList("Staff",
                    allActiveUsersList, true),

                AllActiveUsersList = allActiveUsersList,
                // json serialize list (view does not persist unserialized lists)
                JsonAllActiveUsers = JsonSerializer.Serialize(allActiveUsersList)
        };

            view.JsonActiveDrivers = JsonSerializer.Serialize(view.ActiveDriverList);
            view.JsonActiveODs = JsonSerializer.Serialize(view.ActiveODList);
            view.JsonActiveBaggers = JsonSerializer.Serialize(view.ActiveBaggerList);
            view.JsonActiveStaff = JsonSerializer.Serialize(view.ActiveStaffList);
            return View(view);
        }

        [HttpPost]  
        public ActionResult Index(BulkEmailViewModel view)
        {
            // for Drivers / ODs, Staff / All Users
            view.ActiveDriverList=JsonSerializer.Deserialize<List<ApplicationUser>>(view.JsonActiveDrivers);
            view.ActiveBaggerList = JsonSerializer.Deserialize<List<ApplicationUser>>(view.JsonActiveBaggers);
            view.ActiveODList = JsonSerializer.Deserialize<List<ApplicationUser>>(view.JsonActiveODs);
            view.ActiveStaffList = JsonSerializer.Deserialize<List<ApplicationUser>>(view.JsonActiveStaff);
            view.AllActiveUsersList = JsonSerializer.Deserialize<List<ApplicationUser>>(view.JsonAllActiveUsers);
            view.EmailString = "";

            var emailAddressList = new List<string>();
            if (view.DriverSelect)
            {
                var _dL = view.ActiveDriverList;
                if (_dL != null)
                {
                    foreach (var _user in _dL.Where(_user => !emailAddressList.Contains(_user.Email)))
                    { emailAddressList.Add(_user.Email); }
                }
            }
            
            if (view.BaggerSelect)
            {
                var _bL = view.ActiveBaggerList;
                if (_bL != null)
                {
                    foreach (var _user in _bL.Where(_user => !emailAddressList.Contains(_user.Email)))
                    { emailAddressList.Add(_user.Email); }
                }
            }

            if (view.ODSelect)
            {
                var _oL = view.ActiveODList;
                if (_oL != null)
                {
                    foreach (var _user in _oL.Where(_user => !emailAddressList.Contains(_user.Email)))
                    { emailAddressList.Add(_user.Email); }
                }
            }

            if (view.StaffSelect)
            {
                var _sL = view.ActiveStaffList;
                if (_sL != null)
                {
                    foreach (var _user in _sL.Where(_user => !emailAddressList.Contains(_user.Email)))
                    { emailAddressList.Add(_user.Email); }
                }
            }

            if (view.AllUsersSelect)
            {
                var _aL = view.AllActiveUsersList;
                if (_aL != null)
                {
                    foreach (var _user in _aL.Where(_user => !emailAddressList.Contains(_user.Email)))
                    { emailAddressList.Add(_user.Email); }
                }
            }

            for (var i = 1; i < emailAddressList.Count; i++) // item [0] is null
            {
                view.EmailString += emailAddressList[i] + ",";
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
                            PhoneNumber2 = _user.PhoneNumber2,
                            EmailRoleName = roleName
                        };

                        switch (activeOnly)
                        {
                            case true when addUsr.Active:
                            case false:
                                listMaster.Add(addUsr);
                                break;
                        }

                    }
                }
            }
            return listMaster;
        }
    }
}