using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.WebPages;
using BHelp.DataAccessLayer;
using BHelp.Models;
using BHelp.ViewModels;
using Microsoft.AspNet.Identity;

namespace BHelp.Controllers
{
    [Authorize]
    public class ODController : Controller
    {
        private readonly BHelpContext db = new BHelpContext();

        // GET: OD
        [Authorize(Roles = "Administrator,Staff,Developer,OfficerOfTheDay")]
        public ActionResult Index(string callLogDate, string searchString, int? selectedId)
        {
            if (String.IsNullOrEmpty(callLogDate))
            {
                var cdt = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day);
                var cdts = cdt.ToString("MM/dd/yyyy");
                Session["CallLogDate"] = cdts;
            }
            else
            { Session["CallLogDate"] = callLogDate; }

            if (searchString != null)
            {TempData["SearchResults"] = SearchHouseholds(searchString); }
            var userIid = System.Web.HttpContext.Current.User.Identity.GetUserId();
            if (userIid != null)
            {
                var user = db.Users.Find(userIid);
                Session["CurrentUserFullName"] = user.FullName;
            };
            var houseHoldView = new HouseholdViewModel();

            return View(houseHoldView);
        }
        
        [Authorize(Roles = "Administrator,Staff,Developer,OfficerOfTheDay")]
        public ActionResult SearchResults()
        {
            var householdView = new List<HouseholdViewModel>();
            if (TempData["SearchResults"] is IEnumerable<HouseholdViewModel> searchList)
            {
                householdView.AddRange(searchList);
                return View(householdView);
            }
            return View();
        }

        private static List<HouseholdViewModel> SearchHouseholds(string searchString)
        {
            if (searchString == null) { return null; }

            var householdView = new List<HouseholdViewModel>();
            using var db = new BHelpContext();
            List<Client> clientList;
            if (searchString.Any(char.IsDigit))
            {
                clientList = db.Clients.Where(c => c.Phone.Contains(searchString)
                                                   || c.StreetNumber.Contains(searchString)  
                                                   || c.Notes.Contains(searchString)
                                                   || c.StreetName.Contains(searchString)
                                                   && c.Active).OrderBy(c =>c.LastName).ToList();
            }
            else
            {
                clientList = db.Clients.Where(c => c.Active && c.LastName.Contains(searchString))
                    .OrderBy(c => c.LastName).ToList();
            }
                
            foreach (var client in clientList)
            {
                // if first character of StreetName is numeric, then it is a street number, not a name.
                var apt = client .StreetName;
                if (client.StreetName.Length > 2)
                {
                    if (client.StreetName.Substring(0, 1).IsInt())
                    {
                        apt = client.StreetName.Substring(3); // skip numbered streets
                    }

                    apt = new string(apt.Where(char.IsDigit).ToArray());
                }

                var household = new HouseholdViewModel
                {
                    ClientId = client.Id,
                    FirstName = client.FirstName,
                    LastName = client.LastName,
                    StreetNumber = client.StreetNumber,
                    StreetName = client.StreetName,
                    //Apartment = new string(client.StreetName.Where(char.IsDigit).ToArray()),
                    Apartment = apt,
                    StreetToolTip = client.StreetName.Replace(" ", "\u00a0"),
                    City = client.City,
                    CityToolTip = client.City.Replace(" ", "\u00a0"),
                    Zip = client.Zip,
                    Phone = client.Phone,
                    PhoneToolTip = client.Phone.Replace(" ", "\u00a0"),
                    Notes = client.Notes,
                    // (full length on mouseover)    \u00a0 is the Unicode character for NO-BREAK-SPACE.
                    NotesToolTip = client.Notes.Replace(" ", "\u00a0"),
                    DateCreated = client .DateCreated,
                    DateCreatedString = client .DateCreated.ToString( "MM/dd/yyyy") 
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

            return (householdView);
        }

        [Authorize(Roles = "Administrator,Staff,Developer,OfficerOfTheDay")]
        public ActionResult HouseholdAndDeliveryActions(HouseholdViewModel household,
            string btnAddMember, string btnDeleteMember, string btnAdd, string btnSave, string btnDeliveryConfirmed )
        {
            if (btnDeliveryConfirmed != null)
            {
                household = (HouseholdViewModel)Session["ConfirmHousehold"];
            }

            if (btnSave != null) // Button Save and Exit was pressed
            {
                SaveHouseholdData(household);
                return RedirectToAction("Index","Home");
            }  // Button Save and Exit was pressed

            if (btnAddMember != null || btnDeleteMember != null) // Button Add or Delete Family Member was pressed
            {
                SaveHouseholdData(household);
                var id = Convert.ToInt32(household.ClientId);
                return RedirectToAction("UpdateHousehold", new { id });
            } // Button Add or Delete Family Member was pressed

            //  ===== Button Create Delivery OR btnDeliveryConfirmed was pressed =====

            SaveHouseholdData(household);
            var clientId = household.ClientId;

            var newDelivery = AppRoutines.NewDeliveryRecord(clientId);
            if (newDelivery.DateDelivered != null)
            {
                var openCount = db.Deliveries.Count(d => d.ClientId == clientId && d.Status == 0);
                var nextDeliveryEligible =
                    AppRoutines.GetNextEligibleDeliveryDate(clientId, newDelivery.DateDelivered.Value);
                if (newDelivery.DateDelivered < nextDeliveryEligible || openCount > 0)
                { // go to confirm page
                    TempData["NextEligibleDeliveryDate"] = nextDeliveryEligible;
                    Session["ConfirmHousehold"] = household;
                    if (btnDeliveryConfirmed == null)
                    {
                        return RedirectToAction("ConfirmCreateDelivery", new { newDelivery.ClientId });
                    }
                }
            }

            if (Request.UrlReferrer != null)
            {
                newDelivery.ReturnURL = Request.UrlReferrer.ToString();
            }

            db.Deliveries.Add(newDelivery);
            db.SaveChanges(); // Save the new delivery record to record the Id for the log record.

            // insert delivery log record:
            var dtNow = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(
                DateTime.UtcNow, "Eastern Standard Time");
            var logRec = new DeliveryLog
            {
                DeliveryId = newDelivery.Id,
                DateModified = dtNow,
                ModifiedBy = User.Identity.Name,
                ActionSource = "CREATE",
                DateDelivered = newDelivery.DateDelivered,
                LogDate = dtNow,
                LogOD = AppRoutines.GetUserName(newDelivery.ODId),
                DeliveryOD = AppRoutines.GetUserName(newDelivery.DeliveryDateODId),
                Driver = AppRoutines.GetDriverName(newDelivery.DriverId),
                ClientId = newDelivery.ClientId,
                Client = newDelivery.LastName,
                Status = newDelivery.Status
            };

            db.DeliveryLogs.Add(logRec);
            db.SaveChanges();
            db.Entry(newDelivery).GetDatabaseValues();
            return RedirectToAction("Edit", "Deliveries", new { id = newDelivery.Id });
        }
   
        private void SaveHouseholdData(HouseholdViewModel household) 
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
                cli.Email = household.Email + "";

                foreach (var member in household.FamilyMembers)
                {
                    switch (member.Id)
                    {
                        // is Head of Household
                        case 0 when member.ClientId == 0:
                            cli.FirstName = member.FirstName;
                            cli.LastName = member.LastName;
                            cli.DateOfBirth = DateTime.Today.AddYears(-member.Age);
                            break;
                        //Adding new member (ClientId = -1)
                        case 0 when member.ClientId < 0:
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

                            break;
                        }
                        default:
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

                            break;
                        }
                    }
                }
                db.SaveChanges();
            }
        }

        public ActionResult ConfirmCreateDelivery(int? clientId)
        {
            var dt = TempData["NextEligibleDeliveryDate"];
            var client = db.Clients.Find(clientId);
            if (client == null) return null;
            if (dt == null) return null;
            var newDeliveryView = new DeliveryViewModel
            {
                FullName = client.LastName + ", " + client.FirstName,
                StreetNumber = client.StreetNumber,
                StreetName = client.StreetName,
                DateDelivered = (DateTime)dt
            };
            return View(newDeliveryView);
        }

        [Authorize(Roles = "Administrator,Staff,Developer,OfficerOfTheDay")]
        public ActionResult AdviseDeliveryCreated(int? newId)
        {
            var delivery = db.Deliveries.Find(newId);
            if (delivery == null) return null;
            var newDeliveryView = new DeliveryViewModel
            {
                FullName = delivery.LastName + ", " + delivery.FirstName,
                StreetNumber = delivery.StreetNumber,
                StreetName = delivery.StreetName
            };
            return View(newDeliveryView);
        }

        [Authorize(Roles = "Administrator,Staff,Developer,OfficerOfTheDay")]
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
                Email =client.Email,
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
                OpenDeliveryExists = false,
                DateCreated = client.DateCreated 
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
            { houseHold.OpenDeliveryExists = true; }
            TempData["UpdateHouseholdDirty"] = "false";
            return View(houseHold); // Launches page UpdateHousehold.cshtml
        }

        [Authorize(Roles = "Administrator,Staff,Developer,OfficerOfTheDay")]
        public ActionResult CreateNewHousehold()
        {
            return RedirectToAction("Create", "Clients");
        }

        [Authorize(Roles = "Administrator,Staff,Developer,OfficerOfTheDay")]
        public ActionResult FlagChanges()
        {
            TempData["UpdateHouseholdDirty"] = "true";
            return null;
        }
    }
}
