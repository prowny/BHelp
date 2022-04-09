using System;
using System.Web.Mvc;
using BHelp.ViewModels;
using Org.BouncyCastle.Utilities;

namespace BHelp.Controllers
{
    public class DriverScheduleController : Controller
    {
        // GET: DriverSchedule
        public ActionResult Index()
        {
            var view = new DriverScheduleViewModel();
            if (Session["DriverScheduleMonthYear"] == null)
            {
                view.Month = DateTime.Today.Month;
                view.Year = DateTime.Today.Year;
                view.MonthName = Strings.ToUpperCase(DateTime.Today.ToString("MMMM"));
                Session["DriverScheduleMonthYear"] = view.Month.ToString("00") + view.Year;
            }
            else
            {
                var x = Session["DriverScheduleMonthYear"].ToString();
                view.Month = Convert.ToInt32(x.Substring(0, 2));
                view.Year = Convert.ToInt32(x.Substring(2, 4));
                var tempDate = new DateTime(view.Year, view.Month, 1);
                view.MonthName = Strings.ToUpperCase(tempDate.ToString("MMMM"));
            }

            var startDt = GetFirstWeekDay(view.Month, view.Year);
            var startDayOfWk = (int)startDt.DayOfWeek;
            view.Boxes = new DateTime[7, 6];
            for (var i = 1; i < 7; i++)
            {
                for (int j = 1; j < 6; j++)
                {
                    if (i == 1)
                    {
                        if (j < startDayOfWk) continue;
                        view.Boxes[i, j] = startDt.AddDays(5 - startDayOfWk);
                        continue;
                    }

                    if (view.Boxes[i - 1, j] == DateTime.MinValue)
                    {
                        view.Boxes[i, j] = startDt.AddDays(7 + j - startDayOfWk);
                    }
                    else
                    {
                        view.Boxes[i, j] =view .Boxes [i - 1, j].AddDays(7);
                    }
                }
            } 
            return View(view);
        }

        public ActionResult PreviousMonth(int month, int year)
        {
            month = month - 1;
            if (month < 1)
            {
                month = 12;
                year = year - 1;
            }
            Session["DriverScheduleMonthYear"] = month.ToString("00") + year;
            return RedirectToAction( "Index");
        }

        public ActionResult NextMonth(int month, int year)
        {
            month = month + 1;
            if (month >12)
            {
                month = 1;
                year = year + 1;
            }
            Session["DriverScheduleMonthYear"] = month.ToString("00") + year;
            return RedirectToAction("Index");
        }

        private static DateTime GetFirstWeekDay(int month, int year)
        {
            DateTime dt = new DateTime(year, month, 1);
            var dayOfWeek = (int) dt.DayOfWeek;
            if (dayOfWeek == 0) dt = dt.AddDays(1); // change from Sun to Mon 
            if (dayOfWeek == 6) dt = dt.AddDays(2); // change from Sat to Mon
            return dt;
        }

        //private static DateTime GetBoxDate(int i, int j, DateTime startDt)
        //{
        //    var startDayOfWk = (int)startDt.DayOfWeek;
        //    DateTime dt = DateTime.MinValue; // default
        //    // Boxes are 1-5; 6-10; 11-15; 16-20; 21-25; 26-31.
        //    if (i == 1) // 1st row, j = 1-5
        //    {
        //        if (j < startDayOfWk) return dt;
        //        return startDt.AddDays(5 - startDayOfWk);
        //    }

        //    if (i == 2) // 2nd row, j = 6-10
        //    {
        //        return startDt.AddDays(10 - startDayOfWk);
        //    }
        //    return dt;
        //}
    }
}