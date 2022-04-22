using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using BHelp.DataAccessLayer;
using BHelp.Models;
using BHelp.ViewModels;
using Org.BouncyCastle.Utilities;

namespace BHelp.Controllers
{
    public class DriverScheduleController : Controller
    {
        // GET: DriverSchedule
        public ActionResult Edit( DateTime? boxDate)
        {
            var db = new BHelpContext(); 
            var view = new DriverScheduleViewModel();
            if (Session["DriverScheduleDateData"] == null)
            {
                view.Month = DateTime.Today.Month;
                view.Year = DateTime.Today.Year;
                view.Date = new DateTime(view.Year, view.Month, 1);
                view.MonthName = Strings.ToUpperCase(view.Date.ToString("MMMM"));
                Session["DriverScheduleDateData"] = "01" + view.Month.ToString("00") + view.Year;
            }
            else  // returning to DriverSchedule
            {
                if (boxDate == null)
                {
                    view.Month = DateTime.Today.Month;
                    view.Year = DateTime.Today.Year;
                    var tempDate = new DateTime(view.Year, view.Month, 1);
                    view.Date = tempDate;
                    view.MonthName = Strings.ToUpperCase(tempDate.ToString("MMMM"));
                    Session["DriverScheduleDateData"] = view.Date.Day.ToString("00") + view.Month.ToString("00") + view.Year;
                }
                else  // boxDate has value
                {
                    var _day = boxDate.GetValueOrDefault().Day;
                    var _month= boxDate.GetValueOrDefault().Month;
                    var _year = boxDate.GetValueOrDefault().Year;  
                    var tempDate = new DateTime(_year,_month, _day);
                    view.MonthName = Strings.ToUpperCase(tempDate.ToString("MMMM"));
                    view.Date = (DateTime)boxDate;
                    view.Month = view.Date.Month;
                    view.Year = view.Date.Year;
                    Session["DriverScheduleDateData"] = view.Date.Day.ToString("00") + view.Month.ToString("00") + view.Year;
                }
            }
            
            var startDt = GetFirstWeekDay(view.Month, view.Year);
            var endDate = new DateTime(view.Year, view.Month, DateTime.DaysInMonth(view.Year, view.Month));
            var startDayOfWk = (int)startDt.DayOfWeek;
            if (Session["DriverList"] == null)
            {
                Session["DriverList"] = GetDriverIdSelectList();
            }
            var driverList = GetDriverIdSelectList();

            // Check for existing record
            var existngRec = db.DriverSchedules.FirstOrDefault( r => r.Date == view.Date);
            if (existngRec != null)
            {
                view.DriverList = driverList; 
                view.DriverId = existngRec.DriverId;
                view.BackupDriverList = driverList; 
                view.BackupDriverId = existngRec.BackupDriverId;
                view.Note = existngRec.Note;
            }
            else
            {
                view.DriverList = (List<SelectListItem>)Session["DriverList"]; 
                view.BackupDriverList = (List<SelectListItem>)Session["DriverList"]; 
            }
            
            view.BoxDay = new DateTime[6, 6];
            view.BoxDriverName = new string[26];
            view.BoxBackupDriverName = new string[26];
            view.BoxNote = new string[26];

            // Get all driver records for this month
            var monthlyList = GetMonthlyList(view.Month, view.Year);
            var dummy = monthlyList;

            for (var i = 1; i < 6; i++)
            {
                for (var j = 1; j < 6; j++)
                {
                    if (i == 1)
                    {
                        if (j < startDayOfWk) continue;
                        view.BoxDay[i, j] = startDt.AddDays(j - startDayOfWk);
                        var mIdx = monthlyList.FindIndex(d => d.Date == view.BoxDay[i, j]);
                        if (mIdx >= 0)  // mIdx = -1 if match not found
                        {
                            var dIdx = driverList.FindIndex(d => d.Value == monthlyList[mIdx].DriverId);
                            var bdIdx = driverList.FindIndex(d => d.Value == monthlyList[mIdx].BackupDriverId);
                            var idx = j + 5 * (i - 1);
                            if(dIdx >= 0) view.BoxDriverName[idx] = driverList[dIdx].Text;
                            if(bdIdx >= 0) view.BoxBackupDriverName[idx] = driverList[bdIdx].Text;
                            view.BoxNote[idx] = monthlyList[mIdx].Note;
                        }
                        continue;
                    }

                    if (view.BoxDay[i - 1, j] == DateTime.MinValue)
                    {
                        view.BoxDay[i, j] = startDt.AddDays(7 + j - startDayOfWk);
                        var mIdx = monthlyList.FindIndex(d => d.Date == view.BoxDay[i, j]);
                        if (mIdx >= 0)  // mIdx = -1 if match not found
                        {
                            var dIdx = driverList.FindIndex(d => d.Value == monthlyList[mIdx].DriverId);
                            var bdIdx = driverList.FindIndex(d => d.Value == monthlyList[mIdx].BackupDriverId);
                            var idx = j + 5 * (i - 1);
                            if (dIdx >= 0) view.BoxDriverName[idx] = driverList[dIdx].Text;
                            if (bdIdx >= 0) view.BoxBackupDriverName[idx] = driverList[bdIdx].Text;
                            view.BoxNote[idx] = monthlyList[mIdx].Note;
                        }
                    }
                    else
                    {
                        if (view.BoxDay[i - 1, j].AddDays(7) <= endDate)
                        {
                            view.BoxDay[i, j] = view.BoxDay[i - 1, j].AddDays(7);
                            var mIdx = monthlyList.FindIndex(d => d.Date == view.BoxDay[i, j]);
                            if (mIdx >= 0)  // mIdx = -1 if match not found
                            {
                                var dIdx = driverList.FindIndex(d => d.Value == monthlyList[mIdx].DriverId);
                                var bdIdx = driverList.FindIndex(d => d.Value == monthlyList[mIdx].BackupDriverId);
                                var idx = j + 5 * (i - 1);
                                if (dIdx >= 0) view.BoxDriverName[idx] = driverList[dIdx].Text;
                                if (bdIdx >= 0) view.BoxBackupDriverName[idx] = driverList[bdIdx].Text;
                                view.BoxNote[idx] = monthlyList[mIdx].Note;
                            }
                        }
                    }
                }
            }

            var schedules = new List<DriverScheduleViewModel>();
            var dt = DateTime.MinValue;
            var skip = false;
            for (var i = 0; i < 27; i++)
            {
                if (i >= startDayOfWk)
                {
                    if (skip == false)
                    {
                        dt = startDt;
                        skip = true;
                    }
                }

                var schedule = new DriverScheduleViewModel
                {
                    Id = i,
                    Date = dt,
                    DriverId = "0",
                    BackupDriverId = "0",
                    MonthName = view.MonthName
                };
                if (dt > DateTime.MinValue)
                {
                    schedule.DayString = dt.Day.ToString("0");
                }
                schedules.Add(schedule);
                if (dt > DateTime.MinValue)
                {
                    dt = dt.AddDays(1);
                    if ((int)dt.DayOfWeek == 6)
                    {
                        dt = dt.AddDays(2);
                    }

                    if (dt > endDate) dt = DateTime.MinValue;
                }
            }

            view.DriversSchedule = schedules;

            return View(view);
        }

        // POST: DriverSchedule/Edit
        [HttpPost, Authorize(Roles = "Developer,Administrator,Staff,Scheduler")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(DriverScheduleViewModel schedule)
        {
            if (ModelState.IsValid)
            {
                var db = new BHelpContext();
                // Check if date exists & update
                var rec = db.DriverSchedules
                    .FirstOrDefault( d => d.Date == schedule.Date);
                if (rec != null)
                { // Update record
                    if (schedule.DriverId == "0") schedule.DriverId = null;
                    rec.DriverId = schedule.DriverId;
                    if (schedule.BackupDriverId == "0") schedule.BackupDriverId = null;
                    rec.BackupDriverId = schedule.BackupDriverId;
                    
                    rec.Note = schedule.Note;
                    db.SaveChanges();
                    return RedirectToAction("Edit", new { boxDate = schedule.Date });
                }

                // Add new record
                if (schedule.DriverId == "0") schedule.DriverId = null;
                if (schedule.BackupDriverId == "0") schedule.BackupDriverId = null;
                var newRec = new DriverSchedule
                {
                    Date = schedule.Date,
                    DriverId = schedule.DriverId,
                    BackupDriverId = schedule.BackupDriverId,
                    Note = schedule.Note
                };
                newRec.Note = newRec.Note;
                db.DriverSchedules.Add(newRec);
                db.SaveChanges();
                return RedirectToAction("Edit", new{boxDate =newRec.Date });
            }
            else
            {
                return RedirectToAction("Edit");
            }
        }

        public JsonResult SaveList(List<DriverScheduleViewModel> values)
        {
            return Json(new { Result = $"First item in list: '{values[0]}'" });
        }

        public ActionResult PreviousMonth(int month, int year)
            {
                month = month - 1;
                if (month < 1)
                {
                    month = 12;
                    year = year - 1;
                }
                var _boxDate = GetFirstWeekDay(month, year);
                return RedirectToAction("Edit", new{boxDate = _boxDate });
            }
        public ActionResult NextMonth(int month, int year)
        {
            month = month + 1;
            if (month > 12)
            {
                month = 1;
                year = year + 1;
            }
            var _boxDate = GetFirstWeekDay(month, year);
            return RedirectToAction("Edit", new { boxDate = _boxDate });
        }
        private static DateTime GetFirstWeekDay(int month, int year)
    {
        DateTime dt = new DateTime(year, month, 1);
        var dayOfWeek = (int) dt.DayOfWeek;
        if (dayOfWeek == 0) dt = dt.AddDays(1); // change from Sun to Mon 
        if (dayOfWeek == 6) dt = dt.AddDays(2); // change from Sat to Mon
        return dt;
    }
        private List<SelectListItem> GetDriverIdSelectList()
        {
            if (Session["DriverSelectList"] == null)
            {
                var driverList = new List<SelectListItem>();
                var _db = new BHelpContext();
                var userList = _db.Users.OrderBy(u => u.LastName).ToList();
                //var rolesList = _db.Roles.OrderBy(r => r.Name).ToList();
                var roleLookup = AppRoutines.UsersInRolesLookup();
                var driverRoleId = AppRoutines.GetRoleId("Driver");
                driverList.Add(new SelectListItem()
                {
                    Text = @"--select--",
                    Value = "0"
                });
                foreach (var user in userList)
                {
                    if(roleLookup .Any(r => r.UserId  == user.Id && r.RoleId == driverRoleId))
                    {
                        driverList.Add(new SelectListItem()
                        { 
                            Text = user.FirstName + @" " + user.LastName,
                            Value = user.Id,
                            Selected = false
                        });
                    };
                }
                Session["DriverSelectList"] = driverList;
                return driverList;
            }
            return (List<SelectListItem>)Session["DriverSelectList"];
        }

        private static List<DriverSchedule> GetMonthlyList(int month, int year)
        {
            var start =new DateTime(year, month, 1);
            var end = new DateTime(year, month, DateTime.DaysInMonth(year, month));
            var db = new BHelpContext();
            return db.DriverSchedules
                .Where(d => d.Date >= start && d.Date <= end).ToList();
        }
        public ActionResult Test()
        { 
            Utilities.test();
            return RedirectToAction("Index", "Home");
        }
    }
}