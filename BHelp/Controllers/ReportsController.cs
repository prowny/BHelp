using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using BHelp.DataAccessLayer;
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
                BoxHoliday = new bool[15],
                BoxHolidayDescription = new string[15],
                BoxDriverId = new string[15],
                BoxDriverName = new string[15],
                BoxDriverPhone = new string[15],
                BoxDriverEmail = new string[15],
                BoxBackupDriverId = new string[15],
                BoxBackupDriverName = new string[15],
                BoxBackupDriverPhone = new string[15],
                BoxBackupDriverEmail = new string[15],
                BoxGroupDriverId = new string[15],
                BoxGroupName = new string[15],
                BoxGroupDriverName = new string[15],
                BoxGroupDriverPhone = new string[15],
                BoxGroupDriverEmail = new string[15],
                BoxODId = new string[15],
                BoxODName = new string[15],
                BoxODPhone = new string[15],
                BoxODEmail = new string[15],
                BoxODOddEvenMsg = new string[15]
            };

            var shortMonthList = AppRoutines.GetShortMonthArray();
            var shortWeekdayList = AppRoutines.GetShortWeekdayArray();
            var holidays = (List<Holiday>)Session["Holidays"];

            var db = new BHelpContext();
            for (var row = 0; row < 5; row++) // Mon - Fri
            {
                // First Column - Driver
                var box = row * 3;
                var boxDate = view.BeginDate.AddDays(row);
                view.BoxDateDay[box] = shortWeekdayList[(int)boxDate.DayOfWeek] +
                                       " " + shortMonthList[boxDate.Month] + " " + boxDate.Day;
                var isHoliday = HolidayRoutines.IsHoliday(boxDate, holidays);
                view.BoxHoliday[box] = isHoliday;
                if (isHoliday)
                {
                    var holiday = GetHolidayData(boxDate);
                    view.BoxHolidayDescription[box] = holiday.Description;
                }
                var drSched = db.DriverSchedules
                    .SingleOrDefault(d => d.Date == boxDate);
                if (drSched != null)
                {
                    if (drSched.DriverId != null)
                    {
                        view.BoxDriverId[box] = drSched.DriverId;
                        var usr = db.Users.SingleOrDefault(d => d.Id == drSched.DriverId);
                        if (usr != null)
                        {
                            view.BoxDriverName[box] = usr.FullName;
                            view.BoxDriverPhone[box] = usr.PhoneNumber;
                            view.BoxDriverEmail[box] = usr.Email;
                        }
                    }

                    if (drSched.BackupDriverId != null)
                    {
                        view.BoxBackupDriverId[box] = drSched.BackupDriverId;
                        var usr = db.Users.SingleOrDefault(d => d.Id == drSched.BackupDriverId);
                        if (usr != null)
                        {
                            view.BoxBackupDriverName[box] = usr.FullName;
                            view.BoxBackupDriverPhone[box] = usr.PhoneNumber;
                            view.BoxBackupDriverEmail[box] = usr.Email;
                        }
                    }

                    if (drSched.GroupDriverId != null)
                    {
                        view.BoxGroupDriverId[box] = drSched.GroupDriverId;
                        var grp = db.GroupNames.SingleOrDefault(n => n.Id == drSched.GroupId);
                        if (grp != null) view.BoxGroupName[box] = grp.Name;
                        var usr = db.Users.SingleOrDefault(d => d.Id == drSched.GroupDriverId);
                        if (usr != null)
                        {
                            view.BoxGroupDriverName[box] = usr.FullName;
                            view.BoxGroupDriverPhone[box] = usr.PhoneNumber;
                            view.BoxGroupDriverEmail[box] = usr.Email;
                        }
                    }
                }

                // Second Column - OD
                box++;
                view.BoxDateDay[box] = view.BoxDateDay[box - 1]; // repeats first column date
                view.BoxHoliday[box] = view.BoxHoliday[box -1]; // repeats first column date
                view.BoxHolidayDescription[box] = view.BoxHolidayDescription[box - 1];
                var odSched = db.ODSchedules
                    .SingleOrDefault(d => d.Date == boxDate); // repeats first column date
                if (odSched != null)
                {
                    if (odSched.ODId != null)
                    {
                        view.BoxODId[box] = odSched.ODId;
                        var usr = db.Users.SingleOrDefault(d => d.Id == odSched.ODId);
                        if (usr != null)
                        {
                            view.BoxODName[box] = usr.FullName;
                            view.BoxODPhone[box] = usr.PhoneNumber;
                            view.BoxODEmail[box] = usr.Email;
                            if (boxDate.Day % 2 == 0)
                            {
                                view.BoxODOddEvenMsg[box] = "Take Food Requests Only From EVEN Numbers";
                            }
                            else
                            {
                                view.BoxODOddEvenMsg[box] = "Take Food Requests Only From ODD Numbers";
                            }
                        }
                    }
                }

                // Third Column - next day Driver
                box++;
                if ((int)boxDate.DayOfWeek == 5)
                {
                    boxDate = boxDate.AddDays(3);
                }
                else
                {
                    boxDate = boxDate.AddDays(1);
                }
                view.BoxDateDay[box] = shortWeekdayList[(int)boxDate.DayOfWeek] + " " +
                    shortMonthList[boxDate.Month] + " " + boxDate.Day;
                isHoliday = HolidayRoutines.IsHoliday(boxDate, holidays);
                view.BoxHoliday[box] = isHoliday;
                if (isHoliday)
                {
                    var holiday = GetHolidayData(boxDate);
                    view.BoxHolidayDescription[box] = holiday.Description;
                }
                drSched = db.DriverSchedules
                   .SingleOrDefault(d => d.Date == boxDate);
                if (drSched != null)
                {
                    if (drSched.DriverId != null)
                    {
                        view.BoxDriverId[box] = drSched.DriverId;
                        var usr = db.Users.SingleOrDefault(d => d.Id == drSched.DriverId);
                        if (usr != null)
                        {
                            view.BoxDriverName[box] = usr.FullName;
                            view.BoxDriverPhone[box] = usr.PhoneNumber;
                            view.BoxDriverEmail[box] = usr.Email;
                        }
                    }

                    if (drSched.BackupDriverId != null)
                    {
                        view.BoxBackupDriverId[box] = drSched.BackupDriverId;
                        var usr = db.Users.SingleOrDefault(d => d.Id == drSched.BackupDriverId);
                        if (usr != null)
                        {
                            view.BoxBackupDriverName[box] = usr.FullName;
                            view.BoxBackupDriverPhone[box] = usr.PhoneNumber;
                            view.BoxBackupDriverEmail[box] = usr.Email;
                        }
                    }

                    if (drSched.GroupDriverId != null)
                    {
                        view.BoxGroupDriverId[box] = drSched.GroupDriverId;
                        var grp = db.GroupNames.SingleOrDefault(n => n.Id == drSched.GroupId);
                        if (grp != null) view.BoxGroupName[box] = grp.Name;
                        var usr = db.Users.SingleOrDefault(d => d.Id == drSched.GroupDriverId);
                        if (usr != null)
                        {
                            view.BoxGroupDriverName[box] = usr.FullName;
                            view.BoxGroupDriverPhone[box] = usr.PhoneNumber;
                            view.BoxGroupDriverEmail[box] = usr.Email;
                        }
                    }
                }
            }
            return view;
        }

        private Holiday GetHolidayData(DateTime dt)
        {
            var holidays = (List<Holiday>)Session["Holidays"];
            if (holidays.Count == 0)
            {
                holidays = HolidayRoutines.GetHolidays(dt.Year);
                Session["Holidays"] = holidays;
            }
            // check if ANY calculated date in proper year:
            foreach (var hol in holidays)
            {
                if (hol.CalculatedDate.Year == dt.Year) { break; }
                // else load requested year's holidays:
                holidays = HolidayRoutines.GetHolidays(dt.Year);
                Session["Holidays"] = holidays;
            }
            return holidays.Find(h => h.CalculatedDate == dt);
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