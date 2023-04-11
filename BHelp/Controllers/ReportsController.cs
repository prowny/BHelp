using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Web.Mvc;
using BHelp.Models;
using BHelp.ViewModels;

namespace BHelp.Controllers
{
    public class ReportsController : Controller // WEEKLY INFO REPORT ONLY as of 04/23
    {
        // GET: Reports

        [Authorize(Roles = "Administrator,Developer")]
        public ActionResult WeeklyInfoReport(DateTime? monday)
        {
            if (monday != null) { }
            else
            { monday = GetMondaysDate(DateTime.Today); }
            
            if (Session["Holidays"] == null)
            { Session["Holidays"] = HolidayRoutines.GetHolidays(DateTime.Today.Year); }
            
            var view = GetWeeklyInfoReportData((DateTime)monday);
         
            return View(view);
        }

        private ReportsViewModel GetWeeklyInfoReportData(DateTime monday)
        {
            var view = new ReportsViewModel
            {
                BeginDate = monday,
                DateRangeTitle = monday.ToString("MM/dd/yyyy") + " - " + monday.AddDays(4).ToString("MM/dd/yyyy"),
                BoxDateDay = new string[15],
                BoxHoliday = new bool[15]
            };

            if(Session["Holidays"] == null) { HolidayRoutines.GetHolidays(monday.Year);}
            var holidays = (List<Holiday>)Session["Holidays"];

            for (var row = 0; row < 5; row++) // Mon - Fri
            {
                var box = row * 3;
                var boxDate = view.BeginDate.AddDays(row);
                view.BoxDateDay[box] = boxDate.Day.ToString();
                view.BoxHoliday[box] = HolidayRoutines.IsHoliday(boxDate, holidays);
                
                boxDate = view.BeginDate.AddDays(row);
                view.BoxDateDay[box + 1] = boxDate.Day.ToString();
                view.BoxHoliday[box + 1] = HolidayRoutines.IsHoliday(boxDate, holidays);

                if (row < 3)
                {
                    boxDate = view.BeginDate.AddDays(row + 1);
                }
                else
                {
                    boxDate = view.BeginDate.AddDays(row + 3);
                }

                view.BoxDateDay[box + 2] = boxDate.Day.ToString();
                view.BoxHoliday[box + 2] = HolidayRoutines.IsHoliday(boxDate, holidays);
            }

            return view;
        }

        public ActionResult WeekPrevious(DateTime monday)
        {
            monday = monday.AddDays(-7);
            return RedirectToAction("WeeklyInfoReport", new { monday });
        }
        public ActionResult WeekNext(DateTime monday)
        {
            monday = monday.AddDays(7);
            return RedirectToAction("WeeklyInfoReport", new { monday });
        }
        private static DateTime GetMondaysDate(DateTime date)
        {
            var returnDate = date;
            // if Sat or Sun (6 or 0), get next Monday
            var dow = (int)date.DayOfWeek;
            switch (dow)
            {
                case 0:
                    returnDate = date.AddDays(1);
                    break;
                case 2:
                    returnDate = date.AddDays(-1);
                    break;
                case 3:
                    returnDate = date.AddDays(-2);
                    break;
                case 4:
                    returnDate = date.AddDays(-3);
                    break;
                case 5:
                    returnDate = date.AddDays(-4);
                    break;
                case 6:
                    returnDate = date.AddDays(2);
                    break;
            }
            return returnDate;
        }
    }
}