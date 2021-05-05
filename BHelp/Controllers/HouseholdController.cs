using System;
using System.Collections.Generic;
using System.Linq;
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
        public ActionResult Index()
        {
            var householdView = new List<HouseholdViewModel>();
            //List<HouseholdViewModel> households;

            List<Client> clientList =
                db.Clients.Where(a => a.Active == true).OrderBy(n => n.LastName).ToList();
            foreach (var client in clientList)
            {
                //List<FamilyMember> familyMembers;
                //List<FamilyMember> familyList =

                var x =
                    db.FamilyMembers.Where(c => c.Active == true && c.ClientId == client.Id).ToList();
                var familyList = new List<FamilyViewModel>();
                var headOfHousehold = new FamilyViewModel()
                {
                    FirstName = client.FirstName,
                    LastName = client.LastName,
                    DateOfBirth = client.DateOfBirth,
                };
                familyList.Add(headOfHousehold);
                //familyMembers.Add(headOfHousehold);
                var familyMembers = new List<SelectListItem>();
                foreach (FamilyViewModel member in familyList)
                {
                    member.Age = AppRoutines.GetAge(member.DateOfBirth, DateTime.Today);
                    SelectListItem selListItem = new SelectListItem() {Value = member.FirstName, Text = member.FirstName };
                    familyMembers.Add(selListItem);
                }

                var household = new HouseholdViewModel()
                    {
                        ClientId = client.Id,
                        FirstName = client.FirstName,
                        LastName = client.LastName,
                        StreetNumber = client.StreetNumber,
                        StreetName = client.StreetName,
                        City = client.City,
                        Zip = client.Zip,
                        Phone = client.Phone,
                        FamilyMembers = familyMembers,
                        Notes = client.Notes
                    };
                    householdView.Add(household);
            }
            
            return View(householdView);
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
        public ActionResult ReturnToDashboard()
        {
            return RedirectToAction("Index", "Home");
        }
    }
}