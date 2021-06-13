using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using BHelp.DataAccessLayer;
using BHelp.Models;
using BHelp.ViewModels;

namespace BHelp.Controllers
{
    public class ClientsController : Controller
    {
        private BHelpContext db = new BHelpContext();

        // GET: Clients
        public ActionResult Index()
        {
            var clientList = db.Clients.OrderBy(c => c.LastName).ToList();
            var clientView = new List<ClientViewModel>();
            foreach (var client in clientList)
            {
                var household = new ClientViewModel()
                {
                    Id = client.Id,
                    Active = client.Active,
                    FirstName = client.FirstName,
                    LastName = client.LastName,
                    Age = AppRoutines.GetAge(client.DateOfBirth,DateTime.Today),
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
                clientView.Add(household);
            }
            return View(clientView);
        }

        // GET: Clients/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Client client = db.Clients.Find(id);
            if (client == null)
            {
                return HttpNotFound();
            }
            return View(client);
        }

        // GET: Clients/Create
        public ActionResult Create()
        {
            var viewModel = new ClientViewModel {FamilyMembers = new List<FamilyMember>()};
            // Empty family members:  
            for (int i = 0; i < 10; i++)
            {
                var newMember = new FamilyMember
                { Id = i };
                viewModel.FamilyMembers.Add(newMember);
            }
            return View(viewModel);
        }

        // POST: Clients/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,FirstName,LastName,Age,StreetNumber,StreetName,City,Zip,Phone,Notes,FamilyMembers")] ClientViewModel client)
        {
            if (ModelState.IsValid)
            {
                var newClient = new Client()
                {
                    Active = true,
                    DateOfBirth = DateTime.Today.AddYears(-client.Age),
                    FirstName = client.FirstName,
                    LastName = client.LastName,
                    StreetNumber = client.StreetNumber,
                    StreetName = client.StreetName,
                    Phone = client.Phone,
                    City = client.City,
                    Zip = client.Zip,
                    Notes = client.Notes
                };
                
                db.Clients.Add(newClient);
                db.SaveChanges();
                int clientId = newClient.Id;

                foreach (var member in client.FamilyMembers)
                {
                    if (member.FirstName != null || member.LastName != null)
                    {
                        var newMember = new FamilyMember()
                        {
                            ClientId = clientId,
                            FirstName = member.FirstName,
                            LastName = member.LastName,
                            DateOfBirth = DateTime.Today.AddYears(-member.Age),
                            Active = true
                        };
                        db.FamilyMembers.Add(newMember);
                        db.SaveChanges();
                    }
                }

                return RedirectToAction("Index");
            }

            return View(client);
        }

        // GET: Clients/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Client client = db.Clients.Find(id);
            if (client == null)
            {
                return HttpNotFound();
            }
            return View(client);
        }

        // POST: Clients/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Active,FirstName,LastName,Age," +
                           "StreetNumber,StreetName,City,Zip,Phone,Notes")] Client client)
        {
            if (ModelState.IsValid)
            {
                if (client.FirstName == null) { client.FirstName = ""; }
                if (client.LastName == null) { client.LastName = ""; }
                if (client.StreetNumber == null) { client.StreetNumber = ""; }
                if (client.StreetName == null) { client.StreetName = ""; }
                if (client.City == null) { client.City = ""; }
                if (client.Phone == null) { client.Phone = ""; }
                if (client.Notes == null) { client.Notes = ""; }
                client.DateOfBirth=DateTime.Today.AddYears(-client.Age);
                db.Entry(client).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(client);
        }

        // GET: Clients/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Client client = db.Clients.Find(id);
            if (client == null)
            {
                return HttpNotFound();
            }
            return View(client);
        }

        // POST: Clients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Client client = db.Clients.Find(id);
            if (client != null) db.Clients.Remove(client);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        public ActionResult ReturnToDashboard()
        {
            return RedirectToAction("Index", "Home");
        }
    }
}
