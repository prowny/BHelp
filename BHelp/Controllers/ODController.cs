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

        public ActionResult HouseholdAndDeliveryActions(HouseholdViewModel household,
            string btnAddMember, string btnDeleteMember, string btnAdd, string btnSave)
        {
            if (btnSave != null) // Button Save and Exit was pressed
            {
                SaveHouseholdData(household);
                return RedirectToAction("ReturnToDashboard");
            }  // Button Save and Exit was pressed

            if (btnAddMember != null || btnDeleteMember != null) // Button Add or Delete Family Member was pressed
            {
                SaveHouseholdData(household);
                var id = Convert.ToInt32(household.ClientId);
                return RedirectToAction("UpdateHousehold", new { id });
            }
             // Button Create Delivery was pressed
            var clientId = household.ClientId;
            var userid = System.Web.HttpContext.Current.User.Identity.GetUserId();
            var client = db.Clients.Find(clientId);
            if (client != null)
            {
                Delivery delivery = new Delivery
                {
                    ODId = userid,
                    //DeliveryDateODId not specified
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
                    Seniors = 0,
                    DeliveryDate =  DateTime.Today.AddDays(1),  // Desired Delivery Date
                    DateDelivered = DateTime.Today.AddDays(1)
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
                
                var dateDelivered = delivery.DateDelivered.Value;
                delivery.GiftCardsEligible = AppRoutines.GetGiftCardsEligible(delivery.ClientId, dateDelivered);
                delivery.GiftCards = delivery.GiftCardsEligible;

                // Full Bags:
                var numberInHousehold = delivery.Children + delivery.Adults + delivery.Seniors;
                if (numberInHousehold <= 2) { delivery.FullBags = 1; }
                if (numberInHousehold >= 3 && numberInHousehold <= 4) { delivery.FullBags = 2; }
                if (numberInHousehold == 5 || numberInHousehold == 6) { delivery.FullBags = 3; }
                if (numberInHousehold == 7 || numberInHousehold == 8) { delivery.FullBags = 4; }
                if (numberInHousehold >= 9) { delivery.FullBags = 5; }
                // Half Bags:
                if (numberInHousehold <= 4) { delivery.HalfBags = 1; }
                if (numberInHousehold >= 5 && numberInHousehold <= 8) { delivery.HalfBags = 2; }
                if (numberInHousehold >= 9) { delivery.HalfBags = 3; }
                // Kid Snacks:
                delivery.KidSnacks = AppRoutines.GetNumberOfKids2_17(clientId);
                
                delivery.FirstDelivery = db.Deliveries.Count(d => d.ClientId == clientId) == 0;
                
                db.Deliveries.Add(delivery);
                db.SaveChanges();
                db.Entry(delivery).GetDatabaseValues();
                return RedirectToAction("ConfirmCreateDelivery", new { newId = delivery.Id });
            }
            return RedirectToAction("Index");
        }

        private void SaveHouseholdData(HouseholdViewModel household) //, List<FamilyMember> famList)
        {
            var cli = db.Clients.Find(household.ClientId);
            if (cli != null) // update client
            {
                cli.StreetNumber = household.StreetNumber + "";
                cli.StreetName = household.StreetName + "";
                cli.City = household.City + "";
                cli.Zip = household.Zip;
                cli.Notes = household.Notes + "";
                cli.Phone = household.Phone + "";

                foreach (var member in household.FamilyMembers)
                {
                    if (member.Id == 0 && member.ClientId == 0) // is Head of Household
                    {
                        cli.FirstName = member.FirstName;
                        cli.LastName = member.LastName;
                        cli.DateOfBirth = DateTime.Today.AddYears(-member.Age);
                    }
                    else if (member.Id == 0 && member.ClientId < 0) //Adding new member (ClientId = -1)
                    {
                        var familyMember = new FamilyMember();
                        if (member.FirstName != null || member.LastName != null)
                        {
                            familyMember.ClientId = cli.Id;
                            familyMember.Active = true;
                            familyMember.Delete = false;
                            familyMember.FirstName = member.FirstName;
                            familyMember.LastName = member.LastName;
                            familyMember.DateOfBirth = DateTime.Today.AddYears(-member.Age);
                            db.FamilyMembers.Add(familyMember);
                        }
                    }
                    else
                    {
                        var familyMember = db.FamilyMembers.Find(member.Id);
                        if (familyMember != null)
                        {
                            familyMember.FirstName = member.FirstName;
                            familyMember.LastName = member.LastName;
                            familyMember.DateOfBirth = DateTime.Today.AddYears(-member.Age);
                            if (member.Delete)
                            {
                                db.FamilyMembers.Remove(familyMember);
                            }
                        }
                    }
                }
                db.SaveChanges();
            }
        }

        public ActionResult ConfirmCreateDelivery(int? newId)
        {
            var delivery = db.Deliveries.Find(newId);
            if (delivery != null)
            {
                var newDeliveryView = new DeliveryViewModel
                {
                    FullName = delivery.LastName + ", " + delivery.FirstName,
                    StreetNumber = delivery.StreetNumber,
                    StreetName = delivery.StreetName
                };
                return View(newDeliveryView);
            }
            return null;
        }

        public ActionResult UpdateHousehold(int Id) //Launches 2nd page of OD call log
        {
            var client = db.Clients.Find( Id);
            if (client == null)
            { RedirectToAction("Index"); }

            
            var dtLastDelivery = AppRoutines.GetLastDeliveryDate(Id);
            var dtLastGiftCard = AppRoutines.GetDateLastGiftCard(Id, DateTime.Today);
            var houseHold = new HouseholdViewModel
            {
                ClientId = Id,
                // ReSharper disable once PossibleNullReferenceException
                LastName = client.LastName,
                FirstName = client.FirstName,
                Active = client.Active,
                Age = AppRoutines.GetAge(client.DateOfBirth, DateTime.Today),
                StreetNumber = client.StreetNumber,
                StreetName = client.StreetName,
                City = client.City,
                Zip = client.Zip,
                Phone = client.Phone,
                Notes = client.Notes,
                FamilyMembers = AppRoutines.GetFamilyMembers(client.Id),
                ZipCodes = AppRoutines.GetZipCodesSelectList(),
                DateLastDelivery = dtLastDelivery,
                DesiredDeliveryDate = DateTime.Today.AddDays(1),  // Desired Delivery Date
                GiftCardsThisMonth = AppRoutines.GetPriorGiftCardsThisMonth(Id, DateTime.Today),
                DateLastGiftCard = dtLastGiftCard,
                DeliveriesThisMonth = AppRoutines.GetDeliveriesCountThisMonth( Id,DateTime.Today) ,
                NextDeliveryEligibleDate =AppRoutines.GetNextEligibleDeliveryDate(Id,dtLastDelivery),
                NextGiftCardEligibleDate = AppRoutines.GetNextGiftCardEligibleDate(Id, dtLastGiftCard),
                OpenDeliveryExists = false
            };

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

            var openCount= db.Deliveries.Count(d => d.ClientId == client.Id && d.Status == 0);
            if (openCount > 0)
            { houseHold.OpenDeliveryExists = true;}
            TempData["UpdateHouseholdDirty"] = "false";
            return View(houseHold); // Launches page UpdateHousehold.cshtml
        }

        public ActionResult CreateNewHousehold()
        {
            return RedirectToAction("Create", "Clients");
        }

        public ActionResult FlagChanges()
        {
            TempData["UpdateHouseholdDirty"] = "true";
            return null;
        }

        public ActionResult ReturnToDashboard()
        {
            return RedirectToAction("Index", "Home");
        }
    }
}
