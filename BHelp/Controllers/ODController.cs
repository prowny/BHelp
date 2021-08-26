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
        private readonly BHelpContext db = new BHelpContext();

        // GET: Household
        public ActionResult Index(string callLogDate, string searchString, int? selectedId)
        {
            if (callLogDate.IsNullOrEmpty())
            {
                DateTime cdt = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day);
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
                    clientList = db.Clients.Where(c => c.Phone.Contains(searchString) || c.StreetNumber.Contains(searchString)  
                                                       && c.Active).OrderBy(c =>c.LastName).ToList();
                }
                else
                {
                    clientList = db.Clients.Where(c => c.Active && c.LastName.Contains(searchString))
                        .OrderBy(c => c.LastName).ToList();
                }
                
                foreach (var client in clientList)
                {
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
                        Notes = client.Notes,
                        // (full length on mouseover)    \u00a0 is the Unicode character for NO-BREAK-SPACE.
                        NotesToolTip = client.Notes.Replace(" ", "\u00a0")
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
            }
            return (householdView);
        }

        public ActionResult AddDelivery(int clientId)
        {
            var userIid = System.Web.HttpContext.Current.User.Identity.GetUserId();
            var client = db.Clients.Find(clientId);
            if (client != null)
            {
                Delivery delivery = new Delivery
                {
                    ODId = userIid,
                    ClientId = clientId,
                    LogDate = DateTime.Today,
                    FirstName = client.FirstName,
                    LastName = client.LastName,
                    StreetNumber = client.StreetNumber,
                    StreetName = client.StreetName,
                    Phone = client.Phone,
                    City = client.City,
                    Zip=client.Zip,
                    NamesAgesInHH = AppRoutines.GetNamesAgesOfAllInHousehold(clientId),
                    Children = 0,
                    Adults = 0,
                    Seniors = 0
                };
                var familyList = AppRoutines.GetFamilyMembers(clientId);
                if (familyList != null)
                {
                    foreach (var mbr in familyList)
                    {
                        if (mbr.Age < 18) { delivery.Children += 1; }
                        if (mbr.Age >= 18 && mbr.Age < 60) { delivery.Adults += 1; }
                        if (mbr.Age >= 60) { delivery.Seniors += 1; }
                    }
                }
                // GIFT CARDS ELIGIBLE:
                // 1 per week maximum
                // 1 per household of 3 or fewer
                // 2 per household of 4 or more
                // 3 max per calendar month;
                var totalThisWeek = GetGiftCardsSince(client.Id, DateTime.Today.AddDays(-7));
                DateTime since1 = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var totalThisMonth = GetGiftCardsSince(client.Id, since1);
                var numberInHousehold = delivery.Children + delivery.Adults + delivery.Seniors;
                if (numberInHousehold < 4)   // 1 per household of 3 or fewer
                {
                    delivery.GiftCardsEligible = 1;
                    if (delivery.GiftCardsEligible + totalThisMonth > 3) delivery.GiftCardsEligible = 0;
                }
                if (numberInHousehold > 3)    // 2 per household of 4 or more
                {
                    delivery.GiftCardsEligible = 3 - totalThisMonth;
                    if (delivery.GiftCardsEligible > 2) delivery.GiftCardsEligible = 2;
                }
                if (totalThisWeek > 0) delivery.GiftCardsEligible = 0;   // 1 per week maximum

                db.Deliveries.Add(delivery);
            }
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        private int GetGiftCardsSince(int id, DateTime dt)
        {
            var total = 0;
            var dList = db.Deliveries.Where(d => d.Id == id && d.Completed
                                                            && d.DeliveryDate >= dt).Select(g => g.GiftCards).ToList();
            foreach (var i in dList)
            {
                if (i != null)
                {
                    var gc = (int)i;
                    total += gc;
                }
            }
            return total;
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
                Notes = client.Notes
            };
            houseHold.FamilyMembers = AppRoutines.GetFamilyMembers(client.Id);
            houseHold.ZipCodes = AppRoutines.GetZipCodesSelectList();
            foreach (var item in houseHold.ZipCodes)
            {
                if (item.Value == client.Zip)
                {
                    item.Selected = true;
                    break;
                }
            }
            var newMember = new FamilyMember();
            houseHold.FamilyMembers.Add(newMember);  // Blank line for adding new member.
            newMember.ClientId = -1;
            newMember.Delete = false;
            return View(houseHold); // Launches page UpdateHousehold.cshtml
        }

        public ActionResult CreateNewHousehold()
        {
            return RedirectToAction("Create", "Clients");
        }
        public ActionResult ReturnToDashboard()
        {
            return RedirectToAction("Index", "Home");
        }
    }
}
