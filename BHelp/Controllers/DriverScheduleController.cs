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
            GetSessionLookupLists(boxDate); // DriverList, GroupList, Holidays, DriverScheduleDateData

            var view = GetDriverScheduleViewModel();
           
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
                        item.GroupId = schedule.GroupId;
                        item.GroupDriverId = schedule.GroupDriverId;
                    }
                    Session["MonthlySchedule"] = msUpdate;

                    db.SaveChanges();
                    
                    return RedirectToAction("Edit", new { boxDate = schedule.Date });
                }

                // Add new record
                if (schedule.DriverId == "0") schedule.DriverId = null;
                if (schedule.BackupDriverId == "0") schedule.BackupDriverId = null;
                if (schedule.GroupDriverId == "0") schedule.GroupDriverId = null;
                var gpDriverId = schedule.GroupDriverId;
                if (gpDriverId == "0") gpDriverId = null;
                var newRec = new DriverSchedule
                {
                    Date = schedule.Date,
                    DriverId = schedule.DriverId,
                    BackupDriverId = schedule.BackupDriverId,
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
                    DriverId = schedule.DriverId,
                    BackupDriverId = schedule.BackupDriverId,
                    GroupId = schedule.GroupId,
                    GroupDriverId = schedule.GroupDriverId
                };
                msAdd.Add(newMsAdd);
                Session["MonthlySchedule"] = msAdd;

                db.SaveChanges();
                return RedirectToAction("Edit", new{boxDate =newRec.Date });
            }
            else
            {
                return RedirectToAction("Edit");
            }
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

        private void SetMonthlyList(int month, int year)
        {
            var start =new DateTime(year, month, 1);
            var end = new DateTime(year, month, DateTime.DaysInMonth(year, month));
            var ml = (List<DriverSchedule>)Session["MonthlySchedule"];
            if (Session["MonthlySchedule"] == null || ml.Count == 0)
            {
                var db = new BHelpContext();
                var monthlyList = db.DriverSchedules
                    .Where(d => d.Date >= start && d.Date <= end).OrderBy(d => d.Date).ToList();
                Session["MonthlySchedule"] = monthlyList;
            }
            else
            {  // use existing list:
                var monthlyList = (List<DriverSchedule>)Session["MonthlySchedule"];
                if (monthlyList[0].Date.Month != month || monthlyList[0].Date.Year != year)
                {  // reload:
                    var db = new BHelpContext();
                    monthlyList = db.DriverSchedules
                        .Where(d => d.Date >= start && d.Date <= end).OrderBy(d => d.Date).ToList();
                    Session["MonthlySchedule"] = monthlyList;
                }
            }
        }

        [Authorize(Roles = "Developer,Administrator,Staff,Scheduler,Driver")]
        public ActionResult DriverScheduleToExcel()
        {
            var view = GetDriverScheduleViewModel();
            var workbook = new XLWorkbook();
            IXLWorksheet ws = workbook.Worksheets.Add("Driver Schedule");

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
                BoxGroupName = new string[26],
                BoxGroupDriverName = new string[26],
                BoxGroupDriverPhone = new string[26],
                BoxGroupDriverPhone2 = new string[26],
                BoxGroupDriverEmail = new string[26],
                BoxNote = new string[26],
                BoxHoliday = new bool[26],
                BoxDriverConfirmed = new bool[26]
        };
            var dateData = Session["DriverScheduleDateData"].ToString();
            view.Month = Convert.ToInt32(dateData.Substring(2, 2));
            view.Year = Convert.ToInt32(dateData.Substring(4, 4));
            view.Date = new DateTime(view.Year, view.Month, Convert.ToInt32(dateData.Substring(0, 2)));
            view.MonthName = Strings.ToUpperCase(view.Date.ToString("MMMM"));
            var startDate = new DateTime(view.Year, view.Month, 1);
            var endDate = new DateTime(view.Year,view.Month, DateTime.DaysInMonth(view.Year, view.Month));
            var startDayOfWk = (int)startDate.DayOfWeek;
            var monthlyList = (List<DriverSchedule>)Session["MonthlySchedule"];
            var driverList = (List<SelectListItem>)Session["DriverList"];
            var driverDataList = (List<ApplicationUser>)Session["DriverDataList"];
            view.GroupList = (List<SelectListItem>)(Session["GroupList"]);
            List<HolidayViewModel> holidayList = AppRoutines.GetFederalHolidays(view.Year);
            for (var i = 1; i < 6; i++)
            {
                for (var j = 1; j < 6; j++)
                {
                    view.BoxDay[i, j] = startDate.AddDays(7*(i-1) + j - startDayOfWk);
                    if (view.BoxDay[i,j] < startDate || view.BoxDay[i, j] > endDate) continue;
                    var idx = j + 5 * (i - 1);
                    if (AppRoutines.IsHoliday(view.BoxDay[i, j], holidayList))  //(List<HolidayViewModel>)Session["Holidays"]))
                    {
                        view.BoxHoliday[idx] = true;
                        var holidayData = GetHolidayData(view.BoxDay[i, j]);
                        view.BoxNote[idx] = holidayData.Name + Environment.NewLine
                            + "BH Closed";
                    }

                    var mIdx = monthlyList.FindIndex(d => d.Date == view.BoxDay[i, j]);
                        if (mIdx >= 0)  // mIdx = -1 if match not found
                        {
                            
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

                            view.BoxNote[idx] = monthlyList[mIdx].Note;
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
                // Sets Session["DriverSelectList"], Session["DriverDataList"],
                // and Session["NonSchedulerDriverSelectList"]:
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

            if (Session["Holidays"] == null)
            {
                Session["Holidays"] = AppRoutines.GetFederalHolidays(DateTime.Today.Year);
            }
            // check holidays for proper year:

            var holidays = (List<HolidayViewModel>)Session["Holidays"];
            var july4th = holidays.FirstOrDefault(h => h.Date.Month == 7
                                                       && h.Date.Day == 4);
            if (boxDate != null)
            {
                var _year = boxDate.GetValueOrDefault().Year;
                if (july4th != null)
                {
                    if (july4th.Date.Year !=_year) // need to reloadholidays (year change)
                    {
                        holidays = AppRoutines.GetFederalHolidays(_year);
                        Session["Holidays"] = holidays;
                    }
                }

                var _month = boxDate.GetValueOrDefault().Month;
                var _day = boxDate.GetValueOrDefault().Day; 
                Session["DriverScheduleDateData"] = _day.ToString("00") + _month.ToString("00") + _year;
            }
            else
            {
                Session["DriverScheduleDateData"] =DateTime.Today.Day.ToString("00") + DateTime .Today.Month .ToString("00") + DateTime.Today.Year;
            }
        }
        
        private HolidayViewModel GetHolidayData(DateTime dt)
        {
            var holidays = (List<HolidayViewModel>)Session["Holidays"];
            foreach (var holiday in holidays)
            {
                if (dt == holiday.Date)
                {
                    return holiday;
                }
            }

            return null;
        }

        public ActionResult Test()
        { 
            //var Holidays =AppRoutines.GetFederalHolidays(DateTime.Today.Year);
            return RedirectToAction("Index", "Home");
        }

    }
}