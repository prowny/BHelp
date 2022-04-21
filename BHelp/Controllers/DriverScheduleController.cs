using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using BHelp.DataAccessLayer;
using BHelp.ViewModels;
using Org.BouncyCastle.Utilities;

namespace BHelp.Controllers
{
    public class DriverScheduleController : Controller
    {
        // GET: DriverSchedule
        public ActionResult Edit( DateTime? boxDate)
        {
            var view = new DriverScheduleViewModel();
            if (Session["DriverScheduleDateData"] == null)
            {
                view.Month = DateTime.Today.Month;
                view.Year = DateTime.Today.Year;
                view.Date = new DateTime(view.Year, view.Month, 1);
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
                    Session["DriverScheduleDateData"] = view.Date.Day.ToString("00") + view.Month.ToString("00") + view.Year;
                }
                else  // boxDate has value
                {
                    //var _day = (boxDate.GetValueOrDefault()).Day;
                    //view.MonthName = Strings.ToUpperCase(tempDate.ToString("MMMM"));
                    view.Date = (DateTime)boxDate;
                    view.Month = view.Date.Month;
                    view.Year = view.Date.Year;
                    Session["DriverScheduleDateData"] = view.Date.Day.ToString("00") + view.Month.ToString("00") + view.Year;
                }
                //var scd = Session["DriverScheduleDateData"].ToString();
                //view.Month = Convert.ToInt32(scd.Substring(2, 2));
                //view.Year = Convert.ToInt32(scd.Substring(4, 4));
                //var scdDay = Convert.ToInt32(scd.Substring(0, 2));
                //view.Date = new DateTime(view.Year, view.Month, scdDay);
                //Session["DriverScheduleDateData"] = scdDay.ToString("00") + view.Month.ToString("00") + view.Year;
            }
            
            var startDt = GetFirstWeekDay(view.Month, view.Year);
            var endDate = new DateTime(view.Year, view.Month, DateTime.DaysInMonth(view.Year, view.Month));
            var startDayOfWk = (int)startDt.DayOfWeek;

            var driverList = GetDriverIdSelectList();
            view.DriverList = driverList;
            view.BackupDriverList = driverList;
            view.BoxDay = new DateTime[6, 6];
            view.BoxDDL = new object[6, 6, 3]; // row, col, ddl1/ddl2
            view.BoxDDLDriverId = new string[6, 6, 3];
            view.BoxIndexDriverId = new string[51];
            view.BoxNote = new string[26];
            for (var i = 1; i < 6; i++)
            {
                for (var j = 1; j < 6; j++)
                {
                    if (i == 1)
                    {
                        if (j < startDayOfWk) continue;
                        view.BoxDay[i, j] = startDt.AddDays(j - startDayOfWk);
                        view.BoxDDL[i, j, 1] = driverList;
                        view.BoxDDLDriverId[i, j, 1] = "0";
                        var idx = j + 5 * (i - 1);
                        view.BoxIndexDriverId[idx] = "0";
                        view.BoxDDL[i, j, 2] = driverList;
                        view.BoxDDLDriverId[i, j, 2] = "0";
                        view.BoxIndexDriverId[idx * 2] = "0";
                        view.BoxNote[idx] = "";
                        continue;
                    }

                    if (view.BoxDay[i - 1, j] == DateTime.MinValue)
                    {
                        view.BoxDay[i, j] = startDt.AddDays(7 + j - startDayOfWk);
                        view.BoxDDL[i, j, 1] = driverList;
                        view.BoxDDLDriverId[i, j, 1] = "0";
                        var idx = j + 5 * (i - 1);
                        view.BoxIndexDriverId[idx] = "0";
                        view.BoxDDL[i, j, 2] = driverList;
                        view.BoxDDLDriverId[i, j, 2] = "0";
                        view.BoxIndexDriverId[idx * 2] = "0";
                        view.BoxNote[idx] = "";
                    }
                    else
                    {
                        if (view.BoxDay[i - 1, j].AddDays(7) <= endDate)
                        {
                            view.BoxDay[i, j] = view.BoxDay[i - 1, j].AddDays(7);
                            view.BoxDDL[i, j, 1] = driverList;
                            view.BoxDDLDriverId[i, j, 1] = "0";
                            var idx = j + 5 * (i - 1);
                            view.BoxIndexDriverId[idx] = "0";
                            view.BoxDDL[i, j, 2] = driverList;
                            view.BoxDDLDriverId[i, j, 2] = "0";
                            view.BoxIndexDriverId[idx * 2] = "0";
                            view.BoxNote[idx] = "";
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
                    //Note = "THis isa multiline note xxxxxxxxxxxx",
                    DriverList = driverList,
                    BackupDriverList = driverList,
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
                var x = schedule.Note;
            }

            return RedirectToAction( "Edit");
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
        public ActionResult Test()
        { 
            Utilities.test();
            return RedirectToAction("Index", "Home");
        }
    }
}