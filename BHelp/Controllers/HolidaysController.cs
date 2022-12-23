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
    public class HolidaysController : Controller
    {
        private readonly BHelpContext db = new BHelpContext();

        // GET: Holidays
        public ActionResult Index()
        {
            return View(db.Holidays.ToList());
        }

        // GET: Holidays/Create
        public ActionResult Create()
        {
            var view = new HolidayViewModel()
            {
                Repeat = 0,
                FixedDate = DateTime.Today,
                EffectiveDate = DateTime.Today 
            };
            List<SelectListItem> repeats = new List<SelectListItem>
            {
                new SelectListItem() { Text = "Does not repeat", Value = "0" },
                new SelectListItem() { Text = "Annually on fixed month/day", Value = "1" },
                new SelectListItem() { Text = "Annually on fixed month/week/day", Value = "2" }
            };
            view.Repeats = repeats;

            List<SelectListItem> months = new List<SelectListItem>
            {
                new SelectListItem() { Text = "January", Value = "1" },
                new SelectListItem() { Text = "February", Value = "2" },
                new SelectListItem() { Text = "March", Value = "3" },
                new SelectListItem() { Text = "April", Value = "4" },
                new SelectListItem() { Text = "May", Value = "5" },
                new SelectListItem() { Text = "June", Value = "6" },
                new SelectListItem() { Text = "July", Value = "7" },
                new SelectListItem() { Text = "August", Value = "8" },
                new SelectListItem() { Text = "September", Value = "9" },
                new SelectListItem() { Text = "October", Value = "10" },
                new SelectListItem() { Text = "November", Value = "11" },
                new SelectListItem() { Text = "December", Value = "12" }
            };
            view.Months = months;

            List<SelectListItem> days = new List<SelectListItem>();
            for (int i = 1; i < 32; i++)
            {
                days.Add(new SelectListItem() { Text = i.ToString(), Value = i.ToString() });
            }
            view.Days = days;

            List<SelectListItem> weekdays = new List<SelectListItem>
            {
                new SelectListItem() { Text = "Monday", Value = "1" },
                new SelectListItem() { Text = "Tuesday", Value = "2" },
                new SelectListItem() { Text = "Wednesday", Value = "3" },
                new SelectListItem() { Text = "Thursday", Value = "4" },
                new SelectListItem() { Text = "Friday", Value = "5" }
            };
            view.WeekDays = weekdays;

            List<SelectListItem> weekdaynumbers = new List<SelectListItem>
            {
                new SelectListItem() { Text = "First", Value = "1" },
                new SelectListItem() { Text = "Second", Value = "2" },
                new SelectListItem() { Text = "Third", Value = "3" },
                new SelectListItem() { Text = "Fourth", Value = "4" },
                new SelectListItem() { Text = "Last", Value = "5" }
            };
            view.WeekDayNumbers = weekdaynumbers;

            return View(view);
        }

        // POST: Holidays/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Description,Repeat,FixedDate,Month,Day,Weekday,WeekNumber,EffectiveDate")] Holiday holiday)
        {
            if (ModelState.IsValid)
            {
                db.Holidays.Add(holiday);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return RedirectToAction("Index");
        }

        // GET: Holidays/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Holiday holiday = db.Holidays.Find(id);
            if (holiday == null)
            {
                return HttpNotFound();
            }
            return View(holiday);
        }

        // POST: Holidays/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Description,Repeat,FixedDate,Month,Day,Weekday,WeekNumber,EffectiveDate")] Holiday holiday)
        {
            if (ModelState.IsValid)
            {
                db.Entry(holiday).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(holiday);
        }

        // GET: Holidays/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Holiday holiday = db.Holidays.Find(id);
            if (holiday == null)
            {
                return HttpNotFound();
            }
            return View(holiday);
        }

        // POST: Holidays/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Holiday holiday = db.Holidays.Find(id);
            if (holiday != null) db.Holidays.Remove(holiday);
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
