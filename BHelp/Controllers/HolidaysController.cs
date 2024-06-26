﻿using System;
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
        private readonly BHelpContext db = new();
        // GET: Holidays
        public ActionResult Index()
        {
            var holidayList = db.Holidays.ToList();

            holidayList = holidayList.OrderBy(md => md.FixedDate).ToList();
         
            return View(holidayList);
        }

        // GET: Holidays/Create
        public ActionResult Create()
        {
            var view = new HolidayViewModel
            {
                FixedDate = DateTime.Today,
                EffectiveDate = DateTime.Today,
            };
            return View(view);
        }

        // POST: Holidays/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Description,Repeat,FixedDate,Month,Day," +
                           "Weekday,WeekNumber,EffectiveDate,Repeats")] HolidayViewModel holiday)
        {
            if (string.IsNullOrEmpty(holiday.Description))
            {
                ModelState.AddModelError("Description", "*Description Required");
            }
            if (ModelState.IsValid)
            {
                var newHoliday = new Holiday()
                {
                    Description = holiday.Description,
                    Repeat = holiday.Repeat,
                    FixedDate = holiday.FixedDate,
                    Month = holiday.Month,
                    Day = holiday.Day,
                    Weekday = holiday.Weekday,
                    WeekNumber = holiday.WeekNumber,
                    EffectiveDate = holiday.EffectiveDate
                };
                db.Holidays.Add(newHoliday);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            
            return View(holiday);
        }

        // GET: Holidays/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var holiday = db.Holidays.Find(id);
            if (holiday == null)
            {
                return HttpNotFound();
            }

            return View(holiday);
        }

        // POST: Holidays/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Description,Repeat,FixedDate,Month,Day,WeekNumber,Weekday,EffectiveDate,CalculatedDate")] Holiday holiday)
        {
            if (!ModelState.IsValid) return View(holiday);
            db.Entry(holiday).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: Holidays/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var holiday = db.Holidays.Find(id);
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
            var holiday = db.Holidays.Find(id);
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
