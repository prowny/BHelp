﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Web.Mvc;
using BHelp.DataAccessLayer;
using BHelp.Models;
using BHelp.ViewModels;
using ClosedXML.Excel;
using Microsoft.AspNet.Identity;
using Org.BouncyCastle.Utilities;

namespace BHelp.Controllers
{
    [Authorize]
    public class DriverScheduleController : Controller
    {
        // GET: DriverSchedule
        [AllowAnonymous]
        public ActionResult Edit(DateTime? boxDate)
        {
            var db = new BHelpContext();
            GetSessionLookupLists(boxDate); // DriverList, GroupList, Holidays, DriverScheduleDateData

            var view = GetDriverScheduleViewModel();
           
            if (User.IsInAnyRoles("DriverScheduler","Developer", "Administrator"))
            {
                var cutOffDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                if (view.Date >= cutOffDate || User.IsInAnyRoles("Developer", "Administrator"))
                {
                    view.AllowEdit = true;
                }
                if (User.IsInAnyRoles("DriverScheduler", "Developer", "Administrator"))
                {
                    view.IsScheduler = true;
                }
                else
                {
                    view.IsDriverOnly = true;
                }
            }

            var startDt = AppRoutines.GetFirstWeekDay(view.Month, view.Year);
            var endDate = new DateTime(view.Year, view.Month, DateTime.DaysInMonth(view.Year, view.Month));
            var startDayOfWk = (int)startDt.DayOfWeek;
            var driverList = (List <SelectListItem>)Session["DriverList"];

            if (view.IsScheduler)
            {
                view.DriverList = driverList;
            }
            else
            {
                view.DriverList = (List<SelectListItem>)Session["NonSchedulerDriverSelectList"];
            }
            // Check for existing record
            var existngRec = db.DriverSchedules.FirstOrDefault( r => r.Date == view.Date);
            if (existngRec != null)
            {
                view.DriverList = driverList; 
                view.DriverId = existngRec.DriverId;
                view.BackupDriverList = driverList; 
                view.BackupDriverId = existngRec.BackupDriverId;
                view.BackupDriver2Id = existngRec.BackupDriver2Id;
                view.BackupDriver2List = new List<SelectListItem>();
                foreach (var drvr in driverList)
                {
                    var newItem = new SelectListItem
                    {
                        Value = drvr.Value,
                        Text = drvr.Text
                    };
                    view.BackupDriver2List.Add(newItem);
                }
                view.BackupDriver2List[0].Text = "(none)"; // means no "TBD" shows if selected
                if (existngRec.GroupId == null)
                {
                    view.GroupId = 0;
                }
                else
                {
                    view.GroupId = (int)existngRec.GroupId;
                }
                view.GroupDriverId = existngRec.GroupDriverId; 
                view.Note = existngRec.Note;
            }
            else
            {
                view.DriverList = (List<SelectListItem>)Session["DriverList"]; 
                view.BackupDriverList = (List<SelectListItem>)Session["DriverList"];
                view.BackupDriver2List = new List<SelectListItem>();
                foreach (var drvr in driverList)
                {
                    var newItem = new SelectListItem
                    {
                        Value = drvr.Value,
                        Text = drvr.Text
                    };
                    view.BackupDriver2List.Add(newItem);
                }
                view.BackupDriver2List[0].Text = "(none)"; // means no "TBD" shows if selected
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
                    BackupDriver2Id = "0",
                    MonthName = view.MonthName,
                    Note = view.Note
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
        [HttpPost, Authorize(Roles = "Developer,Administrator,Staff,DriverScheduler")]
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
                    if (schedule.BackupDriver2Id == "0") schedule.BackupDriver2Id = null;
                    rec.BackupDriver2Id = schedule.BackupDriver2Id;
                    if (schedule.GroupId == 0)
                    {
                        rec.GroupId = null;
                    }
                    else 
                    {
                        rec.GroupId = schedule.GroupId;
                    }
                    if (schedule.GroupDriverId == "0") schedule.GroupDriverId = null;
                    rec.GroupDriverId = schedule.GroupDriverId; 
                    rec.Note = schedule.Note;

                    // update session variable:
                    var msUpdate = (List<DriverSchedule>)Session["MonthlySchedule"];
                    foreach (var item in msUpdate.Where(s => s.Date == schedule.Date ) )
                    {
                        item.DriverId = schedule.DriverId;
                        item.BackupDriverId = schedule.BackupDriverId;
                        item.BackupDriver2Id = schedule.BackupDriver2Id;
                        item.GroupId = schedule.GroupId;
                        item.GroupDriverId = schedule.GroupDriverId;
                        item.Note = schedule.Note;
                    }
                    Session["MonthlySchedule"] = msUpdate;

                    db.SaveChanges();
                    
                    return RedirectToAction("Edit", new { boxDate = schedule.Date });
                }

                // Add new record
                if (schedule.DriverId == "0") schedule.DriverId = null;
                if (schedule.BackupDriverId == "0") schedule.BackupDriverId = null;
                if (schedule.BackupDriver2Id == "0") schedule.BackupDriver2Id = null;
                if (schedule.GroupDriverId == "0") schedule.GroupDriverId = null;
                var gpDriverId = schedule.GroupDriverId;
                if (gpDriverId == "0") gpDriverId = null;
                var newRec = new DriverSchedule
                {
                    Date = schedule.Date,
                    DriverId = schedule.DriverId,
                    BackupDriverId = schedule.BackupDriverId,
                    BackupDriver2Id = schedule.BackupDriver2Id,
                    GroupId = schedule.GroupId, 
                    GroupDriverId = gpDriverId,
                    Note = schedule.Note
                };
                newRec.Note = newRec.Note;
                db.DriverSchedules.Add(newRec);

                // update session variable:
                var msAdd = (List<DriverSchedule>)Session["MonthlySchedule"];
                var newMsAdd = new DriverSchedule()
                {
                    Date = schedule.Date,
                    DriverId = schedule.DriverId,
                    BackupDriverId = schedule.BackupDriverId,
                    BackupDriver2Id = schedule.BackupDriver2Id,
                    GroupId = schedule.GroupId,
                    GroupDriverId = schedule.GroupDriverId
                };
                msAdd.Add(newMsAdd);
                Session["MonthlySchedule"] = msAdd;

                db.SaveChanges();
                return RedirectToAction("Edit", new{boxDate = newRec.Date });
            }
            else
            {
                return RedirectToAction("Edit");
            }
        }

        // GET: DriverSchedule/Individual Signup
        public ActionResult Individual(DateTime? boxDate)
        {
            GetSessionLookupLists(boxDate);
            var view =  GetDriverScheduleViewModel();
            view.TodayYearMonth = DateTime.Today.Year * 100 + DateTime.Today.Month;
            var schedules = new List<DriverScheduleViewModel>();
            var startDt = AppRoutines .GetFirstWeekDay(view.Month, view.Year);
            var endDate = new DateTime(view.Year, view.Month, DateTime.DaysInMonth(view.Year, view.Month));
            var startDayOfWk = (int)startDt.DayOfWeek;
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
                    BackupDriver2Id = "0",
                    MonthName = view.MonthName,
                    Note = view.Note
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

        // POST: Submit DriverScheduleViewModel
        [HttpPost, Authorize(Roles = "Developer,Administrator,Staff,Driver,DriverScheduler")]
        [ValidateAntiForgeryToken]
        public ActionResult Individual(DriverScheduleViewModel schedule)
        {
            //return RedirectToAction("Individual");
            if (!ModelState.IsValid) return RedirectToAction("Individual");

            return RedirectToAction("Individual");
        }

       
        public ActionResult DriverSignUp(int? idx, DateTime? date, bool? cancel)
        {
            var CurrentUserId = User.Identity.GetUserId();
            var text = AppRoutines.GetUserFullName(User.Identity.GetUserId());

            // check for existing DriversSchedules record
            using var db = new BHelpContext();
            var rec = db.DriverSchedules
                .FirstOrDefault(d => d.Date == date);
            if (rec != null)
            {
                if (cancel == true)
                {
                    rec.DriverId = null;
                    text += " has canceled as DRIVER for " + date?.ToString("MM/dd/yyyy");
                }
                else // cancel = false:
                {
                    rec.DriverId = CurrentUserId;
                    text += " has signed up as DRIVER for " + date?.ToString("MM/dd/yyyy");
                }
                db.SaveChanges();
            }
            else  // no existing rec:
            {
                if (date != null)
                {
                    var newRec = new DriverSchedule
                    {
                        Date = (DateTime)date,
                        DriverId = CurrentUserId
                    };
                    db.DriverSchedules.Add(newRec);
                    text += " has signed up as DRIVER for " + date.Value.ToString("MM/dd/yyyy");
                }
                db.SaveChanges();
            }
            SendEmailToDriverScheduler(text);

            return RedirectToAction("Individual", new { boxDate = date });
        }
        public ActionResult BackupDriverSignUp(int? idx, DateTime? date, bool? cancel)
        {
            var CurrentUserId = User.Identity.GetUserId();
            var text = AppRoutines.GetUserFullName(User.Identity.GetUserId());

            // check for existing DriversSchedules record
            using var db = new BHelpContext();
            var rec = db.DriverSchedules
                .FirstOrDefault(d => d.Date == date);
            if (rec != null)
            {
                if (cancel == true)
                {
                    rec.BackupDriverId = null;
                    text += " has canceled as BACKUP DRIVER for " + date?.ToString("MM/dd/yyyy");
                }
                else // cancel = false:
                {
                    rec.BackupDriverId = CurrentUserId;
                    text += " has signed up as BACKUP DRIVER for " + date?.ToString("MM/dd/yyyy");
                }
                db.SaveChanges();
            }
            else  // rec is null:
            {
                if (date != null)
                {
                    var newRec = new DriverSchedule
                    {
                        Date = (DateTime)date,
                        BackupDriverId = CurrentUserId,
                    };
                    text += " has signed up as BACKUP DRIVER for " + date.Value.ToString("MM/dd/yyyy");
                    db.DriverSchedules.Add(newRec);
                }

                db.SaveChanges();
            }
            SendEmailToDriverScheduler(text);

            return RedirectToAction("Individual", new { boxDate = date });
        }

        public ActionResult PreviousMonth(int month, int year)
            {
                month = month - 1;
                if (month < 1)
                {
                    month = 12;
                    year = year - 1;
                }
                var _boxDate = AppRoutines.GetFirstWeekDay(month, year);
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
            var _boxDate = AppRoutines.GetFirstWeekDay(month, year);
            return RedirectToAction("Edit", new { boxDate = _boxDate });
        }

        public ActionResult PreviousMonthIndividualDriver(int month, int year)
        {
            month = month - 1;
            if (month < 1)
            {
                month = 12;
                year = year - 1;
            }
            var _boxDate = AppRoutines.GetFirstWeekDay(month, year);
            return RedirectToAction("Individual", new { boxDate = _boxDate });
        }

        public ActionResult NextMonthIndividualDriver(int month, int year)
        {
            month = month + 1;
            if (month > 12)
            {
                month = 1;
                year = year + 1;
            }
            var _boxDate = AppRoutines.GetFirstWeekDay(month, year);
            return RedirectToAction("Individual", new { boxDate = _boxDate });
        }

        private List<SelectListItem> GetDriverIdSelectList()
        {
           
            if (Session["DriverSelectList"] == null)
            {
                var driverList = new List<SelectListItem>();
                var driverDataList = new List<ApplicationUser>();
                var _db = new BHelpContext();
                var userList = _db.Users.OrderBy(u => u.LastName).ToList();
                var roleLookup = AppRoutines.UsersInRolesLookup();
                var driverRoleId = AppRoutines.GetRoleId("Driver");

                driverList.Add(new SelectListItem()
                {
                    Text 
                        = @"(nobody yet)",
                    Value = "0"
                });
                driverDataList.Add(new ApplicationUser()
                {
                    Id = "0" // added so indexes of driverDataList match driverList 
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
                        driverDataList.Add(user);
                    };
                }
                Session["DriverSelectList"] = driverList;
                Session["DriverDataList"] = driverDataList;

                if (!User.IsInAnyRoles("DriverScheduler", "Developer", "Administrator")) // is NOT Scheduler
                {
                    var nonSchedulerDriverSelectList = new List<SelectListItem>
                    {
                        new SelectListItem()
                        {
                            Text = @"(nobody yet)",
                            Value = "0"
                        }
                    };
                    var currentUserId = User.Identity.GetUserId();
                    // get user's record from driverDataList
                    var userData = driverDataList.FirstOrDefault(i => i.Id == currentUserId);
                    if (userData != null)
                    {
                        var userDataSelectItem = new SelectListItem()
                        {
                            Text = userData.FullName,
                            Value = currentUserId
                        };
                        nonSchedulerDriverSelectList.Add(userDataSelectItem);
                    }
                    Session["NonSchedulerDriverSelectList"] = nonSchedulerDriverSelectList;
                }

                return driverList;
            }

            return null;
        }

        private void SetMonthlyList(int month, int year)
        {
            var start =new DateTime(year, month, 1);
            var end = new DateTime(year, month, DateTime.DaysInMonth(year, month));
            //var ml = (List<DriverSchedule>)Session["MonthlySchedule"];
            //if (Session["MonthlySchedule"] == null || ml.Count == 0)
            //{
                var db = new BHelpContext();
                var monthlyList = db.DriverSchedules
                    .Where(d => d.Date >= start && d.Date <= end).OrderBy(d => d.Date).ToList();
                Session["MonthlySchedule"] = monthlyList;
            //}
            //else
            //{  // use existing list:
            //    var monthlyList = (List<DriverSchedule>)Session["MonthlySchedule"];
            //    if (monthlyList[0].Date.Month != month || monthlyList[0].Date.Year != year)
            //    {  // reload:
            //        var db = new BHelpContext();
            //        monthlyList = db.DriverSchedules
            //            .Where(d => d.Date >= start && d.Date <= end).OrderBy(d => d.Date).ToList();
            //        Session["MonthlySchedule"] = monthlyList;
            //    }
            //}
        }

        [Authorize(Roles = "Developer,Administrator,Staff,DriverScheduler,Driver")]
        public ActionResult DriverScheduleToExcel()
        {
            var view = GetDriverScheduleViewModel();
            var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Driver Schedule");

            var dateData = Session["DriverScheduleDateData"].ToString();
            var month = Convert.ToInt32(dateData.Substring(2, 2));
            var year = Convert.ToInt32(dateData.Substring(4, 4));
            var date = new DateTime(year, month, Convert.ToInt32(dateData.Substring(0, 2)));
            var monthName = Strings.ToUpperCase(view.Date.ToString("MMMM"));
            var startDate = new DateTime(view.Year, view.Month, 1);
            var endDate = new DateTime(view.Year, view.Month, DateTime.DaysInMonth(view.Year, view.Month));

            int row = 1;
            ws.Columns("1").Width = 30;
            ws.Columns("2").Width = 30;
            ws.Columns("3").Width = 30;
            ws.Columns("4").Width = 30;
            ws.Columns("5").Width = 30;
            ws.Cell(row, 1).SetValue("BHelp Drivers Schedule - " + monthName + " " + view.Year);

            row++;
            ws.Cell(row, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Cell(row, 1).SetValue("MONDAY");
            ws.Cell(row, 2).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Cell(row, 2).SetValue("TUESDAY");
            ws.Cell(row, 3).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Cell(row, 3).SetValue("WEDNESDAY");
            ws.Cell(row, 4).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Cell(row, 4).SetValue("THURSDAY");
            ws.Cell(row, 5).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Cell(row, 5).SetValue("FRIDAY");
            
            for (var i = 1; i < 6; i++)
            {
                row++;
                for (var j = 1; j < 6; j++)
                {
                    if (view.BoxDay[i, j] < startDate || view.BoxDay[i, j] > endDate) continue; 
                    if (view.BoxDay[i, j] > DateTime.MinValue)
                    {
                        var idx = j + 5 * (i - 1);
                        var boxContents = (view.BoxDay[i, j].Day.ToString("0"));
                        boxContents += Environment.NewLine + "Driver:";
                        if (view.BoxDriverName[idx] == null)
                        {
                            boxContents +="TBD";
                        }
                        else
                        {
                            boxContents += Environment.NewLine + view.BoxDriverName[idx];
                        }

                        if(view.BoxDriverPhone[idx] !=null) 
                        {boxContents += Environment.NewLine + view.BoxDriverPhone[idx]; }

                        if (view.BoxDriverPhone2[idx] != null)
                        { boxContents += Environment.NewLine + view.BoxDriverPhone2[idx]; }

                        if (view.BoxDriverEmail[idx] != null)
                        { boxContents += Environment.NewLine + view.BoxDriverEmail[idx]; }

                        boxContents += Environment.NewLine + "Backup Driver:";
                        if (view.BoxBackupDriverName[idx] == null)
                        {
                            boxContents += "TBD";
                        }
                        else
                        {
                            boxContents += Environment.NewLine + view.BoxBackupDriverName[idx];
                        }

                        if (view.BoxBackupDriverPhone[idx] != null)
                        { boxContents += Environment.NewLine + view.BoxBackupDriverPhone[idx]; }

                        if (view.BoxBackupDriverPhone2[idx] != null)
                        { boxContents += Environment.NewLine + view.BoxBackupDriverPhone2[idx]; }

                        if (view.BoxBackupDriverEmail[idx] != null)
                        { boxContents += Environment.NewLine + view.BoxBackupDriverEmail[idx]; }

                        if (view.BoxBackupDriver2Name[idx] != null)
                        {
                            boxContents += Environment.NewLine + "Backup Driver 2:";
                            boxContents += Environment.NewLine + view.BoxBackupDriver2Name[idx];
                            
                            if (view.BoxBackupDriver2Phone[idx] != null)
                            {
                                boxContents += Environment.NewLine + view.BoxBackupDriver2Phone[idx];
                            }

                            if (view.BoxBackupDriver2Phone2[idx] != null)
                            {
                                boxContents += Environment.NewLine + view.BoxBackupDriver2Phone2[idx];
                            }

                            if (view.BoxBackupDriver2Email[idx] != null)
                            {
                                boxContents += Environment.NewLine + view.BoxBackupDriver2Email[idx];
                            }
                        }

                        if (view.BoxGroupName[idx] != null)
                        {
                            boxContents += Environment.NewLine + "Group:";
                            boxContents += Environment.NewLine + view.BoxGroupName[idx];
                            if (view.BoxGroupDriverName[idx] == null)
                            {
                                boxContents += Environment.NewLine + "Group Driver: TBD";
                            }
                        }

                        if (view.BoxGroupDriverName[idx] != null)
                        {
                            boxContents += Environment.NewLine + "Group Driver:";
                            boxContents += Environment.NewLine + view.BoxGroupDriverName[idx];
                        }

                        if (view.BoxNote[idx] != null)
                        {
                            boxContents += Environment.NewLine + "Notes:";
                            boxContents += Environment.NewLine + view.BoxNote[idx];
                        }

                        if (view.BoxHoliday[idx])
                        {
                            boxContents += Environment.NewLine + view.BoxHolidayDescription[idx];
                        }

                        ws.Cell(row, j).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Top);
                        ws.Cell(row, j).SetValue(boxContents);
                    }
                }
            }

            var ms = new MemoryStream();
            workbook.SaveAs(ms);
            ms.Position = 0;
            return new FileStreamResult(ms, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                { FileDownloadName = "BHDriverSchedule " + date.ToString("MM") + "-" + date.ToString("yyyy") + ".xlsx" };
        }
        private DriverScheduleViewModel GetDriverScheduleViewModel()
        {
            var view = new DriverScheduleViewModel()
            {
                CurrentUserId = User.Identity.GetUserId(),
                BoxDay = new DateTime[6, 6],
                TodayYearMonth = DateTime.Today.Year * 100 + DateTime.Today.Month
        };

            var dateData = Session["DriverScheduleDateData"].ToString();
            view.Month = Convert.ToInt32(dateData.Substring(2, 2));
            view.Year = Convert.ToInt32(dateData.Substring(4, 4));
            view.Date = new DateTime(view.Year, view.Month, Convert.ToInt32(dateData.Substring(0, 2)));
            view.MonthName = Strings.ToUpperCase(view.Date.ToString("MMMM"));
            view.CurrentDate= DateTime.Today;
            var startDate = new DateTime(view.Year, view.Month, 1);
            var endDate = new DateTime(view.Year,view.Month, DateTime.DaysInMonth(view.Year, view.Month));
            var startDayOfWk = (int)startDate.DayOfWeek;
            if (startDayOfWk == 6) startDayOfWk = -1;
            var monthlyList = (List<DriverSchedule>)Session["MonthlySchedule"];
            var driverList = (List<SelectListItem>)Session["DriverList"];
            var driverDataList = (List<ApplicationUser>)Session["DriverDataList"];
            view.GroupList = (List<SelectListItem>)(Session["GroupList"]);
            var holidayList = HolidayRoutines.GetHolidays(view.Year);
            for (var i = 1; i < 6; i++)
            {
                for (var j = 1; j < 6; j++)
                {
                    view.BoxDay[i, j] = startDate.AddDays(7 * (i - 1) + j - startDayOfWk);
                    if (view.BoxDay[i,j] < startDate || view.BoxDay[i, j] > endDate) continue;
                    var idx = j + 5 * (i - 1);
                    
                    if (HolidayRoutines.IsHoliday(view.BoxDay[i, j], holidayList))
                    {
                        view.BoxHoliday[idx] = true;
                        var holidayData = GetHolidayData(view.BoxDay[i, j]);
                        view.BoxHolidayDescription[idx] = holidayData.Description + Environment.NewLine
                            + "BH Closed";
                    }

                    var mIdx = monthlyList.FindIndex(d => d.Date == view.BoxDay[i, j]);
                    if (mIdx >= 0)  // match found  (mIdx = -1 if match not found)
                    {
                        view.BoxNote[idx] = monthlyList[mIdx].Note;
                        var drIdx = driverList.FindIndex(d => d.Value == monthlyList[mIdx].DriverId);
                        if (drIdx >= 0)
                        {
                            view.BoxDriverName[idx] = driverList[drIdx].Text;
                            view.BoxDriverId[idx] = driverList[drIdx].Value;
                            view.BoxDriverPhone[idx] = driverDataList[drIdx].PhoneNumber;
                            view.BoxDriverPhone2[idx] = driverDataList[drIdx].PhoneNumber2;
                            view.BoxDriverEmail[idx] = driverDataList[drIdx].Email;
                        }

                        var bdrIdx = driverList.FindIndex(d => d.Value == monthlyList[mIdx].BackupDriverId);
                        if (bdrIdx >= 0)
                        {
                            view.BoxBackupDriverName[idx] = driverList[bdrIdx].Text;
                            view.BoxBackupDriverId[idx] = driverList[bdrIdx].Value;
                            view.BoxBackupDriverPhone[idx] = driverDataList[bdrIdx].PhoneNumber;
                            view.BoxBackupDriverPhone2[idx] = driverDataList[bdrIdx].PhoneNumber2;
                            view.BoxBackupDriverEmail[idx] = driverDataList[bdrIdx].Email;
                        }

                        var bdr2Idx = driverList.FindIndex(d => d.Value == monthlyList[mIdx].BackupDriver2Id);
                        if (bdr2Idx >= 0)
                        {
                            view.BoxBackupDriver2Name[idx] = driverList[bdr2Idx].Text;
                            view.BoxBackupDriver2Phone[idx] = driverDataList[bdr2Idx].PhoneNumber;
                            view.BoxBackupDriver2Phone2[idx] = driverDataList[bdr2Idx].PhoneNumber2;
                            view.BoxBackupDriver2Email[idx] = driverDataList[bdr2Idx].Email;
                        }

                        var grpId = monthlyList[mIdx].GroupId;
                        if (grpId > 0)
                        {
                            var gpItem = view.GroupList.FirstOrDefault(g => g.Value == grpId.ToString());
                            if (gpItem != null)
                            {
                                view.BoxGroupName[idx] = gpItem.Text;
                            }
                        } 

                        var gpdrIdx = driverList.FindIndex(d => d.Value == monthlyList[mIdx].GroupDriverId);
                        if (gpdrIdx >= 0)
                        {
                            view.BoxGroupDriverName[idx] = driverList[gpdrIdx].Text;
                            view.BoxGroupDriverPhone[idx] = driverDataList[gpdrIdx].PhoneNumber;
                            view.BoxGroupDriverPhone2[idx] = driverDataList[gpdrIdx].PhoneNumber2;
                            view.BoxGroupDriverEmail[idx] = driverDataList[gpdrIdx].Email;
                        }
                    }
                }
            }
            return view;
        }

        private void  GetSessionLookupLists(DateTime? boxDate)
        {
            // Get / Update Session Lookup Data
            if (Session["DriverList"] == null)
            {
                // Sets Session["DriverSelectList"], Session["DriverDataList"]
                //  and Session["NonSchedulerDriverSelectList"]:
                Session["DriverList"] = GetDriverIdSelectList();
            }

            if (boxDate == null)
            {
                SetMonthlyList(DateTime.Today.Month, DateTime.Today.Year);
            }
            else
            {
                var _month = boxDate.GetValueOrDefault().Month;
                var _year = boxDate.GetValueOrDefault().Year;
                SetMonthlyList(_month, _year);
            }
            
            if (Session["GroupList"] == null)
            {
                var db = new BHelpContext(); 
                var groupList = db.GroupNames.OrderBy(n => n.Name).ToList();
                var items = new List<SelectListItem> { new SelectListItem { Text = @"(none)", Value = "0" } };
                foreach (var item in groupList)
                {
                    items.Add(new SelectListItem { Text = item.Name, Value = item.Id.ToString() });
                }

                Session["GroupList"] = items;
            }

            if (boxDate != null)
            {
                var _month = boxDate.GetValueOrDefault().Month;
                var _day = boxDate.GetValueOrDefault().Day;
                var _year = boxDate.GetValueOrDefault().Year;
                Session["DriverScheduleDateData"] = _day.ToString("00") + _month.ToString("00") + _year;
            }
            else // boxDate == null;  
            {
                var _month = DateTime.Today.Month;
                var _year = DateTime.Today.Year; 
                var _day = AppRoutines.GetFirstWeekdayDate(_month, _year).Day;
                Session["DriverScheduleDateData"] = _day.ToString("00") + DateTime .Today.Month.ToString("00") + DateTime.Today.Year;
            }
        }
        
        private static Holiday GetHolidayData(DateTime dt)
        {
            var holidays = HolidayRoutines.GetHolidays(dt.Year);
            return holidays.Find(h => h.FixedDate == dt);
        }

        private static void SendEmailToDriverScheduler(string text)
        {
            var roleId = AppRoutines.GetRoleId("DriverScheduler");
            var listUserIdsInRole = AppRoutines.GetUserIdsInRole(roleId);
            
            foreach (var user in listUserIdsInRole)
            {
                var email = AppRoutines.GetUserEmail(user);
                SendEmail(email, text);
            }
        }

        private static void SendEmail(string address, string text)
        {
            if (AppRoutines.IsDebug())
            {
                address = "prowny@aol.com"; // for testing !!!!!!!!!!!!!!!!!!!!!!!
            }
            using var msg = new MailMessage();
            msg.From = new MailAddress("DriverScheduler@BethesdaHelpFD.org", "BHELP Driver Scheduler");
            msg.To.Add(new MailAddress(address, "BHELP Driver"));
            msg.Subject = "BHELP - Driver Schedule";
            msg.Body = text;
            
            msg.Priority = MailPriority.Normal;
            using var mailClient = new SmtpClient("BethesdaHelpFD.org", 587);
            mailClient.Credentials = new NetworkCredential("DriverScheduler@BethesdaHelpFD.org", "nCig!yv2u*mwPa63_xDya*@V");
             mailClient.Send(msg);
        }
    }
}