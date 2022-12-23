using System;
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
            var holidayList = db.Holidays.ToList();
            foreach (var hol in holidayList)
            {
                hol.MonthList = HolidayRoutines.GetMonthArray();
                hol.WeekDayList = HolidayRoutines.GetWeekdayArray();
                hol.WeekDayNumber = HolidayRoutines.GetWeekdayNumberArray();
            }
            return View(holidayList);
        }

        // GET: Holidays/Create
        public ActionResult Create()
        {
            var view = new HolidayViewModel
            {
                FixedDate = DateTime.Today,
                EffectiveDate = DateTime.Today,
                Repeats = HolidayRoutines.GetRepeatsSelectList(),
                Months = HolidayRoutines.GetMonthsSelectList(),
                Days = HolidayRoutines.GetDaysSelectList(),
                WeekDayNumbers = HolidayRoutines.GetWeekDayNumberSelectList(),
                WeekDays = HolidayRoutines.GetWeekDaySelectList(),
            };
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

            holiday.Days = HolidayRoutines.GetDaysSelectList();
            holiday.Months = HolidayRoutines.GetMonthsSelectList();
            holiday.Repeats = HolidayRoutines.GetRepeatsSelectList();
            holiday.WeekDayNumbers = HolidayRoutines.GetWeekDayNumberSelectList();
            return View(holiday);
        }

        // POST: Holidays/Edit/5
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
