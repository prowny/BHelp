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
    public class HouseholdController : Controller
    {
        private BHelpContext db = new BHelpContext();

        // GET: Household
        public ActionResult Index(int clientId)
        {
            var householdView = new List<HouseholdViewModel>();

            List<Client> clientList =
                db.Clients.Where(a => a.Active).OrderBy(n => n.LastName).ToList();
            foreach (var client in clientList)
            {
                var sqlString = "SELECT * FROM FamilyMembers ";
                sqlString += "WHERE Active > 0 AND ClientId =" + clientId;
                var familyList = db.Database.SqlQuery<FamilyMember>(sqlString).ToList();
                FamilyMember headOfHousehold = new FamilyMember()
                {
                    FirstName = client.LastName,
                    LastName = client.FirstName,
                    DateOfBirth = client.DateOfBirth,
                };
                familyList.Add(headOfHousehold);
                var familyMembers = new List<FamilyMember>();
                foreach (FamilyMember member in familyList)
                {
                    member.Age = AppRoutines.GetAge(member.DateOfBirth, DateTime.Today);
                    familyMembers.Add(member);
                }

                var household = new HouseholdViewModel()
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
            Session["OriginalModel"] = householdView;
            return View(householdView);
        }

        // POST: Household
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(HouseholdViewModel household)
        {
            // Check Client data
            var client = db.Clients.Find(household.ClientId);
            if (client != null)
            {
                //client.FirstName = household.FirstName;
                //client.LastName = household.LastName;
                // check age in list of FamilyMembers
                client.StreetNumber = household.StreetNumber;
                client.StreetName = household.StreetName;
                client.City = household.City;
                client.Zip = household.Zip;
                client.Notes = household.Notes;
                client.Phone = household.Phone;
                
                foreach (var member in household.FamilyMembers)
                {
                    if (member.Id == 0) // is Head of Household
                    {
                        client.FirstName = member.FirstName;
                        client.LastName = member.LastName;
                        client.DateOfBirth = DateTime.Today.AddYears(-member.Age);
                    }
                    else
                    {
                        var familyMember = db.FamilyMembers.Find(member.Id);
                        if (familyMember != null)
                        {
                            familyMember.FirstName = member.FirstName;
                            familyMember.LastName = member.LastName;
                            familyMember.DateOfBirth=DateTime.Today.AddYears(-member.Age);
                        }
                    }
                }
                db.SaveChanges();
                return RedirectToAction("UpdateHousehold", "OD", new { Id = client.Id });
            }
            return null;
        }

    // GET: Household/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Household/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
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

        
        public ActionResult Edit(int? Id)
        {
            if (Id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            return RedirectToAction("Index");
        }
        public ActionResult ReturnToDashboard()
        {
            return RedirectToAction("Index", "Home");
        }
    }
}