﻿using System;
using System.Collections.Generic;
using BHelp.DataAccessLayer;
using BHelp.Models;
using BHelp.ViewModels;

namespace BHelp.Controllers
{
    public class FamilyMembersController : Controller
    {
        private BHelpContext db = new BHelpContext();

        // GET: FamilyMembers
        public ActionResult Index()
        {
            var familyView = new FamilyViewModel();
            List<SelectListItem> clientList = new List<SelectListItem>();
            var clients = db.Clients.OrderBy(c => c.LastName).ToList();
            foreach (var client in clients)
            {
                var selectListItem = new SelectListItem() { Value = client.Id.ToString(), Text = client.LastFirstName};
                clientList.Add(selectListItem);
            }
            familyView.Clients = clientList;
            familyView.FamilyMembers = new List<FamilyMember> {new FamilyMember()};
            return View(familyView);
        }

        // GET: FamilyMembers/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FamilyMember familyMember = db.FamilyMembers.Find(id);
            if (familyMember == null)
            {
                return HttpNotFound();
            }
            return View(familyMember);
        }

        // GET: FamilyMembers/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: FamilyMembers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,ClientId,Active,FirstName,LastName,DateOfBirth")] FamilyMember familyMember)
        {
            if (ModelState.IsValid)
            {
                db.FamilyMembers.Add(familyMember);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(familyMember);
        }

        // GET: FamilyMembers/Edit/5
        public ActionResult Edit(int? Id) // Id contains Client.Id 
        {
            if (Id == null)
            { return new HttpStatusCodeResult(HttpStatusCode.BadRequest); }
            var familyMembers = db.FamilyMembers.Where(m => m.ClientId == Id).ToList();
            foreach (var mbr in familyMembers)
            {
                mbr.Age = AppRoutines.GetAge(mbr.DateOfBirth, DateTime.Today);
            }
            familyMembers.Add(new FamilyMember()); // One blank member
            var familyView = new FamilyViewModel {FamilyMembers = familyMembers};

            return View(familyView);
        }

        // POST: FamilyMembers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,ClientId,Active,FirstName,LastName,DateOfBirth")] FamilyMember familyMember)
        {
            if (ModelState.IsValid)
            {
                db.Entry(familyMember).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return null;  // View(familyMember);
        }

        // GET: FamilyMembers/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FamilyMember familyMember = db.FamilyMembers.Find(id);
            if (familyMember == null)
            {
                return HttpNotFound();
            }
            return View(familyMember);
        }

        // POST: FamilyMembers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            FamilyMember familyMember = db.FamilyMembers.Find(id);
            if (familyMember != null) db.FamilyMembers.Remove(familyMember);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult GetFamilyDetails(int id /* drop down value */)
        {
            var members = db.FamilyMembers.Where(m => m.ClientId == id).ToList();
            members.Add(new FamilyMember()); /*add a blank additional family member*/ 

            try
            {
                return RedirectToAction("UpdateHousehold", "OD", new {Id=id});
                //String json = JsonConvert.SerializeObject(members, Formatting.Indented);
                //return Content(json, "application/json");
            }
            catch (Exception)
            {
                return null;
            }
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
