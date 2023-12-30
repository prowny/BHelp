using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using BHelp.DataAccessLayer;
using BHelp.Models;
using BHelp.ViewModels;

namespace BHelp.Controllers
{
    [Authorize]
    public class HouseholdController : Controller
    {
        private readonly BHelpContext db = new BHelpContext();

        // GET: Household
        [Authorize(Roles = "Administrator,Staff,Developer,OfficerOfTheDay,Driver")]
        public ActionResult Index(int clientId)
        {
            var householdView = new List<HouseholdViewModel>();

            List<Client> clientList =
                db.Clients.Where(a => a.Active).OrderBy(n => n.LastName).ToList();
            foreach (var client in clientList)
            {
                //var sqlString = "SELECT * FROM FamilyMembers ";
                //sqlString += "WHERE Active > 0 AND ClientId =" + clientId;
                //var familyList = db.Database.SqlQuery<FamilyMember>(sqlString).ToList();
                var familyList = db.FamilyMembers.Where(
                    m => m.ClientId == clientId && m.Active).ToList();
                FamilyMember headOfHousehold = new FamilyMember()
                {
                    FirstName = client.LastName,
                    LastName = client.FirstName,
                    DateOfBirth = client.DateOfBirth,
                };
                familyList.Add(headOfHousehold);
                var familyMembers = new List<FamilyMember>();
                foreach (var member in familyList)
                {
                    member.Age = AppRoutines.GetAge(member.DateOfBirth, DateTime.Today);
                    familyMembers.Add(member);
                }

                var household = new HouseholdViewModel
                {
                    ClientId = client.Id,
                    FirstName = client.FirstName,
                    LastName = client.LastName,
                    DateOfBirth = client.DateOfBirth,
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
            Session["OriginalHouseholdModel"] = householdView;
            return View(householdView);
        }

        [Authorize(Roles = "Administrator,Staff,Developer,OfficerOfTheDay,Driver")]
        public ActionResult SaveHousehold(HouseholdViewModel household)
        {
            return RedirectToAction("Index", household);
        }

        // POST: Save Household
        [HttpPost, Authorize(Roles = "Administrator,Staff,Developer,OfficerOfTheDay,Driver")]
        [ValidateAntiForgeryToken]
        public ActionResult Index(HouseholdViewModel household)
        {
            // Check Client data
            var client = db.Clients.Find(household.ClientId);
            if (client != null)
            {
                client.StreetNumber = household.StreetNumber + "";
                client.StreetName = household.StreetName + "";
                client.City = household.City + "";
                client.Zip = household.Zip;
                client.Notes = household.Notes + "";
                client.Phone = household.Phone + "";

                foreach (var member in household.FamilyMembers)
                {
                    if (member.Id == 0 && member.ClientId == 0) // is Head of Household
                    {
                        client.FirstName = member.FirstName;
                        client.LastName = member.LastName;
                        client.DateOfBirth = DateTime.Today.AddYears(-member.Age);
                    }
                    else if (member.Id == 0 && member.ClientId < 0)     //Adding new member (ClientId = -1)
                    {
                        var  familyMember = new FamilyMember();
                        if (member.FirstName != null || member.LastName !=null)
                        {
                            familyMember.ClientId = client.Id;
                            familyMember.Active = true;
                            familyMember.Delete = false;
                            familyMember.FirstName = member.FirstName;
                            familyMember.LastName = member.LastName;
                            familyMember.DateOfBirth=DateTime.Today.AddYears(-member.Age);
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
                // insert Changes Saved! notice
                return RedirectToAction("ConfirmChangesSaved", new { clientId = client.Id });
            }
            return null;
        }

    // GET: Household/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Household/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Active,FirstName,LastName,DateOfBirth,StreetNumber,StreetName,City,Zip,Phone,Notes")] Client client)
        {
            if (ModelState.IsValid)
            {
                db.Clients.Add(client);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(client);
        }
        
        [Authorize(Roles = "Administrator,Staff,Developer,OfficerOfTheDay,Driver")]
        public ActionResult Edit(int? Id)
        {
            if (Id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Administrator,Staff,Developer,OfficerOfTheDay,Driver")]
        public ActionResult ConfirmChangesSaved(int clientId)
        {
            var newDeliveryView = new HouseholdViewModel { ClientId = clientId };
            return View(newDeliveryView);
        }
        
    }
}