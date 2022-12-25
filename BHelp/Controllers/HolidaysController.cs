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
            //List<Holiday> holidayList = HolidayRoutines.GetHolidays(2023);
            var holidayList = db.Holidays.ToList();
            // Add Month-Day to list for sorting
            foreach (var hol in holidayList)
            {
                if (hol.Repeat == 0) // get mo-day from fixed date
                {
                    hol.MonthDay = hol.FixedDate.Month.ToString("00")
                                   + hol.FixedDate.Day.ToString("00");
                }

                if (hol.Repeat == 1)
                {
                    hol.MonthDay = hol.Month.ToString("00") + hol.Day.ToString("00");
                }

                if (hol.Repeat == 2)
                {
                    var weekNo = 7 * (hol.WeekNumber + 1); // (close enough)
                    hol.MonthDay = hol.Month.ToString("00") + weekNo.ToString("00");
                }

                hol.MonthList = HolidayRoutines.GetMonthArray();
                hol.WeekDayList = HolidayRoutines.GetWeekdayArray();
                hol.WeekDayNumber = HolidayRoutines.GetWeekdayNumberArray();
                hol.RepeatList = HolidayRoutines.GetRepeatArray();
            }
            
            return View(holidayList.OrderBy(md => md.MonthDay));
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
                WeekDays = HolidayRoutines.GetWeekDaySelectList()
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
            
            holiday.Repeats = HolidayRoutines.GetRepeatsSelectList();
            holiday.Months = HolidayRoutines.GetMonthsSelectList();
            holiday.Days = HolidayRoutines.GetDaysSelectList();
            holiday.WeekDayNumbers = HolidayRoutines.GetWeekDayNumberSelectList();
            holiday.WeekDays = HolidayRoutines.GetWeekDaySelectList();
            return View(holiday);
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
            holiday.WeekDays = HolidayRoutines.GetWeekDaySelectList();
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
            Holiday holiday = db.Holidays.Find(id);
            if (holiday == null)
            {
                return HttpNotFound();
            }
            holiday.MonthList = HolidayRoutines.GetMonthArray();
            holiday.WeekDayList = HolidayRoutines.GetWeekdayArray();
            holiday.WeekDayNumber = HolidayRoutines.GetWeekdayNumberArray();
            holiday.RepeatList = HolidayRoutines.GetRepeatArray();
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
