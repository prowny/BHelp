using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using BHelp.DataAccessLayer;
using BHelp.Models;
using Castle.Core.Internal;
using BHelp.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security.Provider;
using Newtonsoft.Json;

namespace BHelp.Controllers
{
    public class ODController : Controller
    {
        private BHelpContext db = new BHelpContext();

        // GET: Household
        public ActionResult Index(string callLogDate, string searchString)
        {
            if (callLogDate.IsNullOrEmpty())
            {
                DateTime cdt = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day + 1);
                var cdts = cdt.ToString("MM/dd/yyyy");
                Session["CallLogDate"] = cdts;
            }
            else
            {
                Session["CallLogDate"] = callLogDate;
            }

            var householdView = new List<HouseholdViewModel>();
            object searchList = TempData["SearchResults"];
            if (searchList != null)
            {
                foreach (var item in (IEnumerable<HouseholdViewModel>) searchList)
                {
                    householdView.Add(item);
                }
            }

            var userIid = System.Web.HttpContext.Current.User.Identity.GetUserId();
            if (userIid != null)
            {
                var user = db.Users.Find(userIid);
                Session["CurrentUserFullName"] = user.FullName;
            };
            return View(householdView);
        }

        public ActionResult SearchClients()
        {
            var userIid = System.Web.HttpContext.Current.User.Identity.GetUserId();
            if(userIid != null)
            { var user = db.Users.Find(userIid);
            Session["CurrentUserFullName"] = user.FullName;}
            return View();
        }
        
        public ActionResult SearchHouseholds(string searchString)
        {
            if (searchString == null)
            {
                return RedirectToAction("Index", "OD", new {callLogDate = Session["CallLogDate"]});
            }

            var householdView = new List<HouseholdViewModel>();

            List<Client> clientList =
                db.Clients.Where(c => c.Active && c.Phone.Contains(searchString)).OrderBy(c => c.LastName).ToList();
            foreach (var client in clientList)
            {
                var sqlString = "SELECT * FROM FamilyMembers ";
                sqlString += "WHERE Active > 0 AND ClientId =" + client.Id;
                var familyList = db.Database.SqlQuery<FamilyMember>(sqlString).ToList();

                FamilyMember headOfHousehold = new FamilyMember()
                {
                    // ! sql result reversed First and Last Names!
                    FirstName = client.LastName,
                    LastName = client.FirstName,
                    DateOfBirth = client.DateOfBirth,
                };
                familyList.Add(headOfHousehold);
                var familyMembers = new List<SelectListItem>();
                foreach (FamilyMember member in familyList)
                {
                    member.Age = AppRoutines.GetAge(member.DateOfBirth, DateTime.Today);
                    var text = member.FirstName + " " + member.LastName + "/" + member.Age;
                    SelectListItem selListItem = new SelectListItem() {Value = member.FirstName, Text = text};
                    familyMembers.Add(selListItem);
                }

                var household = new HouseholdViewModel()
                {
                    ClientId = client.Id,
                    FirstName = client.FirstName,
                    LastName = client.LastName,
                    StreetNumber = client.StreetNumber,
                    StreetName = client.StreetName,
                    StreetToolTip = client.StreetName.Replace(" ", "\u00a0"),
                    City = client.City,
                    CityToolTip = client.City.Replace(" ", "\u00a0"),
                    Zip = client.Zip,
                    Phone = client.Phone,
                    PhoneToolTip = client.Phone.Replace(" ", "\u00a0"),
                    FamilyMembers = familyMembers,
                    Notes = client.Notes,
                    // (full length on mouseover)    \u00a0 is the Unicode character for NO-BREAK-SPACE.
                    NotesToolTip = client.Notes.Replace(" ", "\u00a0"),
                };

                var s = household.StreetName; // For display, abbreviate to 10 characters:           
                s = s.Length <= 10 ? s : s.Substring(0, 10) + "...";
                household.StreetName = s;
                s = household.City; // For display, abbreviate to 11 characters:           
                s = s.Length <= 11 ? s : s.Substring(0, 11) + "...";
                household.City = s;
                s = household.Phone; // For display, abbreviate to 12 characters:           
                s = s.Length <= 12 ? s : s.Substring(0, 12) + "...";
                household.Phone = s;
                s = household.Notes; // For display, abbreviate to 12 characters:           
                s = s.Length <= 12 ? s : s.Substring(0, 12) + "...";
                household.Notes = s;

                householdView.Add(household);
            }

            //return View(householdView);
            TempData["SearchResults"] = householdView;
            //return RedirectToAction("Index", "OD");
            try
            {
                String json = JsonConvert.SerializeObject(householdView, Formatting.Indented);
                return Content(json, "application/json");
                //return View(householdView);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public ActionResult ReturnToDashboard()
        {
            return RedirectToAction("Index", "Home");
        }
    }
}
