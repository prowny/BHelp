using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using BHelp.DataAccessLayer;
using BHelp.Models;
using Castle.Core.Internal;
using BHelp.ViewModels;
using Microsoft.AspNet.Identity;

namespace BHelp.Controllers
{
    public class ODController : Controller
    {
        private BHelpContext db = new BHelpContext();

        // GET: Household
        public ActionResult Index(string callLogDate, string searchString, int? selectedId)
        {
            if (callLogDate.IsNullOrEmpty())
            {
                DateTime cdt = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day + 1);
                var cdts = cdt.ToString("MM/dd/yyyy");
                Session["CallLogDate"] = cdts;
            }
            else
            { Session["CallLogDate"] = callLogDate; }

            if (searchString != null)
            { TempData["SearchResults"] = SearchHouseholds(searchString); }
            var userIid = System.Web.HttpContext.Current.User.Identity.GetUserId();
            if (userIid != null)
            {
                var user = db.Users.Find(userIid);
                Session["CurrentUserFullName"] = user.FullName;
            };
            HouseholdViewModel houseHoldView = new HouseholdViewModel();

            return View(houseHoldView);
        }

        public ActionResult SearchResults()
        {
            var householdView = new List<HouseholdViewModel>();
            if (TempData["SearchResults"] is IEnumerable<HouseholdViewModel> searchList)
            {
                foreach (HouseholdViewModel item in searchList)
                { householdView.Add(item); }
                return View(householdView);
            }
            return View();
        }

        static List<HouseholdViewModel> SearchHouseholds(string searchString)
        {
            if (searchString == null) { return null; }

            var householdView = new List<HouseholdViewModel>();
            using (var db = new BHelpContext())
            {
                List<Client> clientList;
                if (searchString.Any(char.IsDigit))
                {
                    clientList = db.Clients.Where(c => c.Active && c.Phone.Contains(searchString))
                        .OrderBy(c => c.LastName).ToList();
                }
                else
                {
                    clientList = db.Clients.Where(c => c.Active && c.LastName.Contains(searchString))
                        .OrderBy(c => c.LastName).ToList();
                }
                
                foreach (var client in clientList)
                {
                    //client.FamilyMembers = GetFamilyMembers(client.Id);
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
                        //FamilyMembers =client.FamilyMembers,
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

                    //household.FamilySelectList = new List<SelectListItem>();
                    //foreach (var mbr in client.FamilyMembers)
                    //{
                    //    var text = mbr.FirstName + " " + mbr.LastName + "/" + AppRoutines.GetAge(mbr.DateOfBirth, DateTime.Today);
                    //    var selListItem = new SelectListItem() { Value = mbr.FirstName, Text = text };
                    //    household.FamilySelectList.Add(selListItem);
                    //}
                    householdView.Add(household);
                }
            }
            return (householdView);
        }

        private static List<FamilyMember> GetFamilyMembers(int clientId)
        {
            var familyMembers = new List<FamilyMember>();   // For editiing
            using (var db = new BHelpContext())
            {
                var client = db.Clients.Find(clientId);
                var sqlString = "SELECT * FROM FamilyMembers ";
                sqlString += "WHERE Active > 0 AND ClientId =" + clientId;
                var familyList = db.Database.SqlQuery<FamilyMember>(sqlString).ToList();
                if (client != null)
                {
                    FamilyMember headOfHousehold = new FamilyMember()
                    {
                        FirstName = client.FirstName,
                        LastName = client.LastName,
                        DateOfBirth = client.DateOfBirth,
                    };
                    familyList.Add(headOfHousehold);
                }

                foreach (FamilyMember member in familyList)
                {
                    member.Age = AppRoutines.GetAge(member.DateOfBirth, DateTime.Today);
                    member.NameAge = member.FirstName + " " + member.LastName + "/" + member.Age;
                    member.Delete = false;
                    familyMembers.Add(member);
                }
            }
            return familyMembers;
        }

        public ActionResult AddDelivery(int clientId)
        {
            var userIid = System.Web.HttpContext.Current.User.Identity.GetUserId();

            Delivery delivery = new Delivery()
            {
                ODId = userIid,
                ClientId = clientId,
                DeliveryDate = Convert.ToDateTime(Session["CallLogDate"])
            };
            db.Deliveries.Add(delivery);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult UpdateHousehold(int Id) //Launches 2nd page of OD call log
        {
            Client client = db.Clients.Find( Id);
            if (client == null)
            { RedirectToAction("Index"); }

            HouseholdViewModel houseHold = new HouseholdViewModel()
            {
                ClientId = Id,
                // ReSharper disable once PossibleNullReferenceException
                LastName = client.LastName,
                FirstName = client.FirstName,
                Active =client.Active,
                Age=AppRoutines.GetAge(client.DateOfBirth, DateTime.Today),
                StreetNumber = client.StreetNumber,
                StreetName = client.StreetName,
                City = client.City,
                Zip = client.Zip,
                Phone = client.Phone,
                Notes = client.Notes,
                FamilyMembers = GetFamilyMembers(client.Id)
            };
            var newMember = new FamilyMember();
            houseHold.FamilyMembers.Add(newMember);  // Blank line for adding new member.
            newMember.ClientId = -1;
            newMember.Delete = false;
            return View(houseHold); // Launches page UpdateHousehold.cshtml
        }

        public ActionResult CreateNewHousehold()
        {
            return null;
        }
        public ActionResult ReturnToDashboard()
        {
            return RedirectToAction("Index", "Home");
        }
    }
}
