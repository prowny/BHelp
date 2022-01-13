using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BHelp.DataAccessLayer;
using BHelp.Models;

namespace BHelp.Controllers
{
    public class VolunteerHoursController : Controller
    {
        private BHelpContext db = new BHelpContext();

        // GET: VolunteerHours
        public ActionResult Index()
        {
            return View(db.VolunteerHours.ToList());
        }

        // GET: VolunteerHours/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            VolunteerHours volunteerHours = db.VolunteerHours.Find(id);
            if (volunteerHours == null)
            {
                return HttpNotFound();
            }
            return View(volunteerHours);
        }

        // GET: VolunteerHours/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: VolunteerHours/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,UserId,OriginatorUserId,Category,Subcategory,WeekEndingDate,Hours,Minutes")] VolunteerHours volunteerHours)
        {
            if (ModelState.IsValid)
            {
                db.VolunteerHours.Add(volunteerHours);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(volunteerHours);
        }

        // GET: VolunteerHours/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            VolunteerHours volunteerHours = db.VolunteerHours.Find(id);
            if (volunteerHours == null)
            {
                return HttpNotFound();
            }
            return View(volunteerHours);
        }

        // POST: VolunteerHours/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,UserId,OriginatorUserId,Category,Subcategory,WeekEndingDate,Hours,Minutes")] VolunteerHours volunteerHours)
        {
            if (ModelState.IsValid)
            {
                db.Entry(volunteerHours).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(volunteerHours);
        }

        // GET: VolunteerHours/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            VolunteerHours volunteerHours = db.VolunteerHours.Find(id);
            if (volunteerHours == null)
            {
                return HttpNotFound();
            }
            return View(volunteerHours);
        }

        // POST: VolunteerHours/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            VolunteerHours volunteerHours = db.VolunteerHours.Find(id);
            db.VolunteerHours.Remove(volunteerHours);
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
    }
}
