using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public ActionResult Edit( DateTime? boxDate)
        {
            var db = new BHelpContext(); 
            var view = new DriverScheduleViewModel() { CurrentUserId = User.Identity.GetUserId() }; ;
            
            if (Session["DriverScheduleDateData"] == null)
            {
                view.Month = DateTime.Today.Month;
                view.Year = DateTime.Today.Year;
                view.Date = AppRoutines.GetFirstWeekdayDate(view.Month, view.Year);
                view.MonthName = Strings.ToUpperCase(view.Date.ToString("MMMM"));
                Session["DriverScheduleDateData"] = "01" + view.Month.ToString("00") + view.Year;
            }
            else  // returning to DriverSchedule
            {
                if (boxDate == null)
                {
                    view.Month = DateTime.Today.Month;
                    view.Year = DateTime.Today.Year;
                    view.Date = AppRoutines.GetFirstWeekdayDate(view.Month, view.Year);
                    view.MonthName = Strings.ToUpperCase(view.Date.ToString("MMMM"));
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

            if (User.IsInAnyRoles("Scheduler","Developer", "Administrator"))
            {
                var cutOffDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                if (view.Date >= cutOffDate || User.IsInAnyRoles("Developer", "Administrator"))
                    //if (view.Date >= cutOffDate)
                {
                    view.AllowEdit = true;
                }
                if (User.IsInAnyRoles("Scheduler", "Developer", "Administrator"))
                {
                    view.IsScheduler = true;
                }
                else
                {
                    view.IsDriverOnly = true;
                }
            }

            var startDt = GetFirstWeekDay(view.Month, view.Year);
            var endDate = new DateTime(view.Year, view.Month, DateTime.DaysInMonth(view.Year, view.Month));
            var startDayOfWk = (int)startDt.DayOfWeek;
            if (Session["DriverList"] == null)
            {
                Session["DriverList"] = GetDriverIdSelectList();
            }

            var driverList = (List <SelectListItem>)Session["DriverList"];
            var driverDataList = (List<ApplicationUser>)Session["DriverDataList"];
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
                view.Note = existngRec.Note;
            }
            else
            {
                view.DriverList = (List<SelectListItem>)Session["DriverList"]; 
                view.BackupDriverList = (List<SelectListItem>)Session["DriverList"]; 
            }
            
            view.BoxDay = new DateTime[6, 6];
            view.BoxDriverId = new string[26];
            view.BoxDriverName = new string[26];
            view.BoxBackupDriverName = new string[26];
            view.BoxDriverPhone = new string[26];
            view.BoxDriverPhone2 = new string[26];
            view.BoxDriverEmail = new string[26];

            view.BoxBackupDriverId = new string[26];
            view.BoxBackupDriverName = new string[26];
            view.BoxBackupDriverPhone = new string[26];
            view.BoxBackupDriverPhone2 = new string[26];
            view.BoxBackupDriverEmail = new string[26];
            view.BoxNote = new string[26];

            view.BoxDriverConfirmed = new bool[26];

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
                        if (mIdx >= 0)  // (mIdx = -1 if match not found)
                        {
                            var idx = j + 5 * (i - 1);
                            var drIdx = driverList.FindIndex(d => d.Value == monthlyList[mIdx].DriverId);
                            if (drIdx >= 0)
                            {
                                view.BoxDriverName[idx] = driverList[drIdx].Text;
                                view.BoxDriverPhone[idx] = driverDataList[drIdx].PhoneNumber;
                                view.BoxDriverPhone2[idx] = driverDataList[drIdx].PhoneNumber2;
                                view.BoxDriverEmail[idx] = driverDataList[drIdx].Email;
                            }

                            var bdrIdx = driverList.FindIndex(d => d.Value == monthlyList[mIdx].BackupDriverId);
                            if (bdrIdx >= 0)
                            {
                                view.BoxBackupDriverName[idx] = driverList[bdrIdx].Text;
                                view.BoxBackupDriverPhone[idx] = driverDataList[bdrIdx].PhoneNumber;
                                view.BoxBackupDriverPhone2[idx] = driverDataList[bdrIdx].PhoneNumber2;
                                view.BoxBackupDriverEmail[idx] = driverDataList[bdrIdx].Email;
                            }
                            
                            view.BoxNote[idx] = monthlyList[mIdx].Note;
                        }
                        continue;
                    }

                    if (view.BoxDay[i - 1, j] == DateTime.MinValue)
                    {
                        view.BoxDay[i, j] = startDt.AddDays(7 + j - startDayOfWk);
                        var mIdx = monthlyList.FindIndex(d => d.Date == view.BoxDay[i, j]);
                        if (mIdx >= 0)  // (mIdx = -1 if match not found)
                        {
                            var idx = j + 5 * (i - 1);
                            var drIdx = driverList.FindIndex(d => d.Value == monthlyList[mIdx].DriverId);
                            if (drIdx >= 0)
                            {
                                view.BoxDriverName[idx] = driverList[drIdx].Text;
                                view.BoxDriverPhone[idx] = driverDataList[drIdx].PhoneNumber;
                                view.BoxDriverPhone2[idx] = driverDataList[drIdx].PhoneNumber2;
                                view.BoxDriverEmail[idx] = driverDataList[drIdx].Email;
                            }

                            var bdrIdx = driverList.FindIndex(d => d.Value == monthlyList[mIdx].BackupDriverId);
                            if (bdrIdx >= 0)
                            {
                                view.BoxBackupDriverName[idx] = driverList[bdrIdx].Text;
                                view.BoxBackupDriverPhone[idx] = driverDataList[bdrIdx].PhoneNumber;
                                view.BoxBackupDriverPhone2[idx] = driverDataList[bdrIdx].PhoneNumber2;
                                view.BoxBackupDriverEmail[idx] = driverDataList[bdrIdx].Email;
                            }

                            view.BoxNote[idx] = monthlyList[mIdx].Note;
                        }
                    }
                    else
                    {
                        if (view.BoxDay[i - 1, j].AddDays(7) <= endDate)
                        {
                            view.BoxDay[i, j] = view.BoxDay[i - 1, j].AddDays(7);
                            var mIdx = monthlyList.FindIndex(d => d.Date == view.BoxDay[i, j]);
                            if (mIdx >= 0) // (mIdx = -1 if match not found)
                            {
                                var idx = j + 5 * (i - 1);
                                var drIdx = driverList.FindIndex(d => d.Value == monthlyList[mIdx].DriverId);
                                if (drIdx >= 0)
                                {
                                    view.BoxDriverName[idx] = driverList[drIdx].Text;
                                    view.BoxDriverPhone[idx] = driverDataList[drIdx].PhoneNumber;
                                    view.BoxDriverPhone2[idx] = driverDataList[drIdx].PhoneNumber2;
                                    view.BoxDriverEmail[idx] = driverDataList[drIdx].Email;
                                }

                                var bdrIdx = driverList.FindIndex(d => d.Value == monthlyList[mIdx].BackupDriverId);
                                if (bdrIdx >= 0)
                                {
                                    view.BoxBackupDriverName[idx] = driverList[bdrIdx].Text;
                                    view.BoxBackupDriverPhone[idx] = driverDataList[bdrIdx].PhoneNumber;
                                    view.BoxBackupDriverPhone2[idx] = driverDataList[bdrIdx].PhoneNumber2;
                                    view.BoxBackupDriverEmail[idx] = driverDataList[bdrIdx].Email;
                                }

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
                var driverDataList = new List<ApplicationUser>();
                var _db = new BHelpContext();
                var userList = _db.Users.OrderBy(u => u.LastName).ToList();
                var roleLookup = AppRoutines.UsersInRolesLookup();
                var driverRoleId = AppRoutines.GetRoleId("Driver");

                driverList.Add(new SelectListItem()
                {
                    Text = @"(nobody yet)",
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

                if (!User.IsInAnyRoles("Scheduler", "Developer", "Administrator")) // is NOT Scheduler
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

        private static List<DriverSchedule> GetMonthlyList(int month, int year)
        {
            var start =new DateTime(year, month, 1);
            var end = new DateTime(year, month, DateTime.DaysInMonth(year, month));
            var db = new BHelpContext();
            return db.DriverSchedules
                .Where(d => d.Date >= start && d.Date <= end).OrderBy(d => d.Date).ToList();
        }

        [Authorize(Roles = "Developer,Administrator,Staff,Scheduler,Driver")]
        public ActionResult DriverScheduleToExcel()
        {
            var dateData = Session["DriverScheduleDateData"].ToString();
            var month = Convert.ToInt32(dateData.Substring(2, 2));
            var year = Convert.ToInt32(dateData.Substring(4, 4));
            var startDt = new DateTime(year, month, 1);
            var endDate = new DateTime(year, month, DateTime.DaysInMonth(year, month));
            var startDayOfWk = (int)startDt.DayOfWeek;
            var monthlyList = GetMonthlyList(month, year);
            var driverList = (List<SelectListItem>)Session["DriverList"];
            var driverDataList  = (List<ApplicationUser>)Session["DriverDataList"];
            var BoxDay = new DateTime[6, 6];
            var BoxDriverName = new string[26];
            var BoxDriverPhone = new string[26];
            var BoxDriverPhone2 = new string[26];
            var BoxDriverEmail = new string[26];
            var BoxBackupDriverName = new string[26];
            var BoxBackupDriverPhone = new string[26];
            var BoxBackupDriverPhone2 = new string[26];
            var BoxBackupDriverEmail = new string[26];
            var BoxNote = new string[26];

            for(var i = 1; i < 6; i++)
            {
                for (var j = 1; j < 6; j++)
                {
                    if (i == 1)
                    {
                        if (j < startDayOfWk) continue;
                        BoxDay[i, j] = startDt.AddDays(j - startDayOfWk);
                        var mIdx = monthlyList.FindIndex(d => d.Date == BoxDay[i, j]);
                        if (mIdx >= 0)  // mIdx = -1 if match not found
                        {
                            var dIdx = driverList.FindIndex(d => d.Value == monthlyList[mIdx].DriverId);
                            var bdIdx = driverList.FindIndex(d => d.Value == monthlyList[mIdx].BackupDriverId);
                            var idx = j + 5 * (i - 1);
                            if (dIdx >= 0)
                            {
                                BoxDriverName[idx] = driverList[dIdx].Text;
                               
                            }
                            if (bdIdx >= 0) BoxBackupDriverName[idx] = driverList[bdIdx].Text;
                            BoxNote[idx] = monthlyList[mIdx].Note;
                        }
                        continue;
                    }
                    if (BoxDay[i - 1, j] == DateTime.MinValue)
                    {
                        BoxDay[i, j] = startDt.AddDays(7 + j - startDayOfWk);
                        var mIdx = monthlyList.FindIndex(d => d.Date == BoxDay[i, j]);
                        if (mIdx >= 0)  // mIdx = -1 if match not found
                        {
                            var dIdx = driverList.FindIndex(d => d.Value == monthlyList[mIdx].DriverId);
                            var bdIdx = driverList.FindIndex(d => d.Value == monthlyList[mIdx].BackupDriverId);
                            var idx = j + 5 * (i - 1);
                            if (dIdx >= 0) BoxDriverName[idx] = driverList[dIdx].Text;
                            if (bdIdx >= 0) BoxBackupDriverName[idx] = driverList[bdIdx].Text;
                            BoxNote[idx] = monthlyList[mIdx].Note;
                        }
                    }
                    else
                    {
                        if (BoxDay[i - 1, j].AddDays(7) <= endDate)
                        {
                            BoxDay[i, j] = BoxDay[i - 1, j].AddDays(7);
                            var mIdx = monthlyList.FindIndex(d => d.Date == BoxDay[i, j]);
                            if (mIdx >= 0)  // mIdx = -1 if match not found
                            {
                                var dIdx = driverList.FindIndex(d => d.Value == monthlyList[mIdx].DriverId);
                                var bdIdx = driverList.FindIndex(d => d.Value == monthlyList[mIdx].BackupDriverId);
                                var idx = j + 5 * (i - 1);
                                if (dIdx >= 0) BoxDriverName[idx] = driverList[dIdx].Text;
                                if (bdIdx >= 0) BoxBackupDriverName[idx] = driverList[bdIdx].Text;
                                BoxNote[idx] = monthlyList[mIdx].Note;
                            }
                        }
                    }
                }
            }

            var workbook = new XLWorkbook();
            IXLWorksheet ws = workbook.Worksheets.Add("Driver Schedule");

            var tempDate = new DateTime(year, month, 1);
            var monthName = tempDate.ToString("MMMM");

            int row = 1;
            ws.Columns("1").Width = 20;
            ws.Columns("2").Width = 20;
            ws.Columns("3").Width = 20;
            ws.Columns("4").Width = 20;
            ws.Columns("5").Width = 20;
            ws.Cell(row, 1).SetValue("BHelp Drivers Schedule - " + monthName + " " + year);

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
                    if (BoxDay[i, j] > DateTime.MinValue)
                    {
                        var boxContents = (BoxDay[i, j].Day.ToString("0"));
                        var idx = j + 5 * (i - 1);
                        if (BoxDriverName[idx] != null)
                        {
                            boxContents += Environment.NewLine + "Driver:";
                            boxContents += Environment.NewLine + BoxDriverName[idx];
                        }

                        if (BoxBackupDriverName[idx] != null)
                        {
                            boxContents += Environment.NewLine + "Backup Driver:";
                            boxContents += Environment.NewLine + BoxBackupDriverName[idx];
                        }

                        if (BoxNote[idx] != null)
                        {
                            boxContents += Environment.NewLine + "Notes:";
                            boxContents += Environment.NewLine + BoxNote[idx];
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
                { FileDownloadName = "BHDriverSchedule " + tempDate.ToString("MM") + "-" + tempDate.ToString("yyyy") + ".xlsx" };
        }
        private DriverScheduleViewModel GetDriverScheduleViewModel()
        {
            var view = new DriverScheduleViewModel()
            {
                BoxDriverId = new string[26],
                BoxDriverName = new string[26],
                BoxDriverPhone = new string[26],
                BoxDriverPhone2 = new string[26],
                BoxDriverEmail = new string[26],
                BoxBackupDriverId = new string[26],
                BoxBackupDriverName = new string[26],
                BoxBackupDriverPhone = new string[26],
                BoxBackupDriverPhone2 = new string[26],
                BoxBackupDriverEmail = new string[26],
                BoxNote = new string[26]
            };
            var dateData = Session["DriverScheduleDateData"].ToString();
            var month = Convert.ToInt32(dateData.Substring(2, 2));
            var year = Convert.ToInt32(dateData.Substring(4, 4));
            var startDt = new DateTime(year, month, 1);
            var endDate = new DateTime(year, month, DateTime.DaysInMonth(year, month));
            var startDayOfWk = (int)startDt.DayOfWeek;
            var monthlyList = GetMonthlyList(month, year);
            var driverList = (List<SelectListItem>)Session["DriverList"];
            var driverDataList = (List<ApplicationUser>)Session["DriverDataList"];
            var BoxDay = new DateTime[6, 6];
            for (var i = 1; i < 6; i++)
            {
                for (var j = 1; j < 6; j++)
                {
                    if (i == 1)
                    {
                        if (j < startDayOfWk) continue;
                        BoxDay[i, j] = startDt.AddDays(j - startDayOfWk);
                        var mIdx = monthlyList.FindIndex(d => d.Date == BoxDay[i, j]);
                        if (mIdx >= 0)  // mIdx = -1 if match not found
                        {
                            var idx = j + 5 * (i - 1);
                            var drIdx = driverList.FindIndex(d => d.Value == monthlyList[mIdx].DriverId);
                            if (drIdx >= 0)
                            {
                                view.BoxDriverName[idx] = driverList[drIdx].Text;
                                view.BoxDriverPhone[idx] = driverDataList[drIdx].PhoneNumber;
                                view.BoxDriverPhone2[idx] = driverDataList[drIdx].PhoneNumber2;
                                view.BoxDriverEmail[idx] = driverDataList[drIdx].Email;
                            }

                            var bdrIdx = driverList.FindIndex(d => d.Value == monthlyList[mIdx].BackupDriverId);
                            if (bdrIdx >= 0)
                            {
                                view.BoxBackupDriverName[idx] = driverList[bdrIdx].Text;
                                view.BoxBackupDriverPhone[idx] = driverDataList[bdrIdx].PhoneNumber;
                                view.BoxBackupDriverPhone2[idx] = driverDataList[bdrIdx].PhoneNumber2;
                                view.BoxBackupDriverEmail[idx] = driverDataList[bdrIdx].Email;
                            }

                            view.BoxNote[idx] = monthlyList[mIdx].Note;
                        }
                        continue;
                    }
                    if (BoxDay[i - 1, j] == DateTime.MinValue)
                    {
                        BoxDay[i, j] = startDt.AddDays(7 + j - startDayOfWk);
                        var mIdx = monthlyList.FindIndex(d => d.Date == BoxDay[i, j]);
                        if (mIdx >= 0)  // mIdx = -1 if match not found
                        {
                            var idx = j + 5 * (i - 1);
                            var drIdx = driverList.FindIndex(d => d.Value == monthlyList[mIdx].DriverId);
                            if (drIdx >= 0)
                            {
                                view.BoxDriverName[idx] = driverList[drIdx].Text;
                                view.BoxDriverPhone[idx] = driverDataList[drIdx].PhoneNumber;
                                view.BoxDriverPhone2[idx] = driverDataList[drIdx].PhoneNumber2;
                                view.BoxDriverEmail[idx] = driverDataList[drIdx].Email;
                            }

                            var bdrIdx = driverList.FindIndex(d => d.Value == monthlyList[mIdx].BackupDriverId);
                            if (bdrIdx >= 0)
                            {
                                view.BoxBackupDriverName[idx] = driverList[bdrIdx].Text;
                                view.BoxBackupDriverPhone[idx] = driverDataList[bdrIdx].PhoneNumber;
                                view.BoxBackupDriverPhone2[idx] = driverDataList[bdrIdx].PhoneNumber2;
                                view.BoxBackupDriverEmail[idx] = driverDataList[bdrIdx].Email;
                            }

                            view.BoxNote[idx] = monthlyList[mIdx].Note;
                        }
                    }
                    else
                    {
                        if (BoxDay[i - 1, j].AddDays(7) <= endDate)
                        {
                            BoxDay[i, j] = BoxDay[i - 1, j].AddDays(7);
                            var mIdx = monthlyList.FindIndex(d => d.Date == BoxDay[i, j]);
                            if (mIdx >= 0)  // mIdx = -1 if match not found
                            {
                                var idx = j + 5 * (i - 1);
                                var drIdx = driverList.FindIndex(d => d.Value == monthlyList[mIdx].DriverId);
                                if (drIdx >= 0)
                                {
                                    view.BoxDriverName[idx] = driverList[drIdx].Text;
                                    view.BoxDriverPhone[idx] = driverDataList[drIdx].PhoneNumber;
                                    view.BoxDriverPhone2[idx] = driverDataList[drIdx].PhoneNumber2;
                                    view.BoxDriverEmail[idx] = driverDataList[drIdx].Email;
                                }

                                var bdrIdx = driverList.FindIndex(d => d.Value == monthlyList[mIdx].BackupDriverId);
                                if (bdrIdx >= 0)
                                {
                                    view.BoxBackupDriverName[idx] = driverList[bdrIdx].Text;
                                    view.BoxBackupDriverPhone[idx] = driverDataList[bdrIdx].PhoneNumber;
                                    view.BoxBackupDriverPhone2[idx] = driverDataList[bdrIdx].PhoneNumber2;
                                    view.BoxBackupDriverEmail[idx] = driverDataList[bdrIdx].Email;
                                }

                                view.BoxNote[idx] = monthlyList[mIdx].Note;
                            }
                        }
                    }
                }
            }
            return view;
        }
        public ActionResult Test()
        { 
            Utilities.test();
            return RedirectToAction("Index", "Home");
        }

    }
}