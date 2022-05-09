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
    public class ODScheduleController : Controller
    {
        // GET: ODSchedule
        [AllowAnonymous]
        public ActionResult Edit(DateTime? boxDate, string odId)
        {
            var db = new BHelpContext();
           
            var view = new ODScheduleViewModel { CurrentUserId = User.Identity.GetUserId() };

            if (Session["ODScheduleDateData"] == null)
            {
                view.Month = DateTime.Today.Month;
                view.Year = DateTime.Today.Year;
                view.Date = AppRoutines.GetFirstWeekdayDate(view.Month, view.Year);
                view.MonthName = Strings.ToUpperCase(view.Date.ToString("MMMM"));
                Session["ODScheduleDateData"] = "01" + view.Month.ToString("00") + view.Year;
            }
            else  // returning to ODSchedule
            {
                // Returning with OD change:
                if (odId != null)
                {
                    var dateData = Session["ODScheduleDateData"].ToString();
                    var day = Convert.ToInt32(dateData.Substring(0, 2));
                    var month = Convert.ToInt32(dateData.Substring(2, 2));
                    var year = Convert.ToInt32(dateData.Substring(4, 4));
                    boxDate = new DateTime(year, month,day);
                }
                if (boxDate == null)
                {
                    view.Month = DateTime.Today.Month;
                    view.Year = DateTime.Today.Year;
                    view.Date = AppRoutines.GetFirstWeekdayDate(view.Month, view.Year);
                    view.MonthName = Strings.ToUpperCase(view.Date.ToString("MMMM"));
                    Session["ODScheduleDateData"] = view.Date.Day.ToString("00") + view.Month.ToString("00") + view.Year;
                }
                else  // boxDate has value
                {
                    var _day = boxDate.GetValueOrDefault().Day;
                    var _month = boxDate.GetValueOrDefault().Month;
                    var _year = boxDate.GetValueOrDefault().Year;
                    var tempDate = new DateTime(_year, _month, _day);
                    view.MonthName = Strings.ToUpperCase(tempDate.ToString("MMMM"));
                    view.Date = (DateTime)boxDate;
                    view.Month = view.Date.Month;
                    view.Year = view.Date.Year;
                    Session["ODScheduleDateData"] = view.Date.Day.ToString("00") + view.Month.ToString("00") + view.Year;
                }
            }

            if (User.IsInAnyRoles("Scheduler", "Developer", "Administrator"))
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
                    view.IsODOnly = true;
                }
            }

            var startDt = AppRoutines.GetFirstWeekdayDate(view.Month, view.Year);
            var endDate = new DateTime(view.Year, view.Month, DateTime.DaysInMonth(view.Year, view.Month));
            var startDayOfWk = (int)startDt.DayOfWeek;
            if (Session["ODList"] == null)
            {
                Session["ODList"] = GetODIdSelectList();  // also gets ODDataList, NonSchedulerODSelectList 
            }

            var odList = (List<SelectListItem>)Session["ODList"];  //GetODIdSelectList();
            var odDataList = (List<ApplicationUser>)Session["ODDataList"];
            if (view.IsScheduler)
            {
                view.ODList = odList;
            }
            else
            {
                view.ODList = (List<SelectListItem>)Session["NonSchedulerODSelectList"];
            }
            // Check for existing record
            var existingRec = db.ODSchedules.FirstOrDefault(r => r.Date == view.Date);
            if (existingRec != null)
            {
                if (existingRec.ODId != null)
                {
                    view.ODId = existingRec.ODId;
                    view.OldODId = existingRec.ODId; 
                    var odIdx = odList.FindIndex(d => d.Value == view.ODId);
                    if (odIdx > 0) view.ODName = odDataList[odIdx].FullName;
                }
                view.Note = existingRec.Note;
            }
            
            // Check for new OD id - returning from view having clicked the OD dropdownlist.
            if (odId != null) view.ODId = odId;

            view.BoxDay = new DateTime[6, 6];
            view.BoxODName = new string[26];
            view.BoxODId = new string[26];
            view.BoxODPhone = new string[26];
            view.BoxODPhone2 = new string[26];
            view.BoxODEmail = new string[26];
            view.BoxNote = new string[26];
            view.BoxODConfirmed = new bool[26];

            // Get all OD records for this month
            var monthlyList = GetMonthlyList(view.Month, view.Year);
           // var dummy = monthlyList;

            for (var i = 1; i < 6; i++)
            {
                for (var j = 1; j < 6; j++)
                {
                    var idx = j + 5 * (i - 1);  // index of Display Box 1 - 25
                    if (i == 1)
                    {
                        if (j < startDayOfWk) continue;
                        view.BoxDay[i, j] = startDt.AddDays(j - startDayOfWk);
                        var mIdx = monthlyList.FindIndex(d => d.Date == view.BoxDay[i, j]);
                        if (mIdx >= 0) // mIdx = -1 if match not found
                        {
                            var odIdx = odList.FindIndex(d => d.Value == monthlyList[mIdx].ODId);
                            if (odIdx >= 0)
                            {
                                view.BoxODName[idx] = odList[odIdx].Text;
                                if (odDataList[odIdx].PhoneNumber != null) view.BoxODPhone[idx] = odDataList[odIdx].PhoneNumber;
                                if (odDataList[odIdx].PhoneNumber2 != null) view.BoxODPhone2[idx] = odDataList[odIdx].PhoneNumber2;
                                if (odDataList[odIdx].Email != null) view.BoxODEmail[idx] = odDataList[odIdx].Email;
                                if (odList[odIdx].Value != null) view.BoxODId[idx] = odList[odIdx].Value;
                            }
                            view.BoxNote[idx] = monthlyList[mIdx].Note;
                            view.BoxODConfirmed[idx] = monthlyList[mIdx].ODConfirmed;
                        }
                        continue;
                    }

                    if (view.BoxDay[i - 1, j] == DateTime.MinValue)
                    {
                        view.BoxDay[i, j] = startDt.AddDays(7 + j - startDayOfWk);
                        var mIdx = monthlyList.FindIndex(d => d.Date == view.BoxDay[i, j]);
                        if (mIdx >= 0)  // mIdx = -1 if match not found
                        {
                            var odIdx = odList.FindIndex(d => d.Value == monthlyList[mIdx].ODId);
                            if (odIdx >= 0)
                            {
                                view.BoxODName[idx] = odList[odIdx].Text;
                                if (odDataList[odIdx].PhoneNumber != null) view.BoxODPhone[idx] = odDataList[odIdx].PhoneNumber;
                                if (odDataList[odIdx].PhoneNumber2 != null) view.BoxODPhone2[idx] = odDataList[odIdx].PhoneNumber2;
                                if (odDataList[odIdx].Email != null) view.BoxODEmail[idx] = odDataList[odIdx].Email;
                                if (odList[odIdx].Value != null) view.BoxODId[idx] = odList[odIdx].Value;
                            }
                            view.BoxNote[idx] = monthlyList[mIdx].Note;
                            view.BoxODConfirmed[idx] = monthlyList[mIdx].ODConfirmed;
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
                                var odIdx = odList.FindIndex(d => d.Value == monthlyList[mIdx].ODId);
                                if (odIdx >= 0)
                                {
                                    view.BoxODName[idx] = odList[odIdx].Text;
                                    if (odDataList[odIdx].PhoneNumber != null) view.BoxODPhone[idx] = odDataList[odIdx].PhoneNumber;
                                    if (odDataList[odIdx].PhoneNumber2 != null) view.BoxODPhone2[idx] = odDataList[odIdx].PhoneNumber2;
                                    if (odDataList[odIdx].Email != null) view.BoxODEmail[idx] = odDataList[odIdx].Email;
                                    if (odList[odIdx].Value != null) view.BoxODId[idx] = odList[odIdx].Value;
                                }
                                view.BoxNote[idx] = monthlyList[mIdx].Note;
                                view.BoxODConfirmed[idx] = monthlyList[mIdx].ODConfirmed;
                            }
                        }
                    }
                }
            }

            var schedules = new List<ODScheduleViewModel>();
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

                var schedule = new ODScheduleViewModel
                {
                    Id = i,
                    Date = dt,
                    ODId = "0",
                    ODConfirmed = false,
                    MonthName = view.MonthName,
                    Note =view.Note 
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

            view.ODsSchedule = schedules;

            return View(view);
        }

        // POST: DriverSchedule/Edit
        [HttpPost,AllowAnonymous,ValidateAntiForgeryToken]
        public ActionResult Edit(ODScheduleViewModel schedule)
        {
            if (ModelState.IsValid)
            {
                var db = new BHelpContext();
                // Check if date exists & update
                var rec = db.ODSchedules
                    .FirstOrDefault(d => d.Date == schedule.Date);
                if (rec != null)
                { // Update record
                    if (schedule.ODId  == "0") schedule.ODId  = null;
                    rec.ODId = schedule.ODId;
                    rec.Note = schedule.Note;
                    db.SaveChanges();
                    return RedirectToAction("Edit", new { boxDate = schedule.Date });
                }

                // Add new record
                if (schedule.ODId  == "0") schedule.ODId = null;
                var newRec = new ODSchedule()
                {
                    Date = schedule.Date,
                    ODId = schedule.ODId,
                    Note = schedule.Note
                };
                db.ODSchedules.Add(newRec);
                db.SaveChanges();
                return RedirectToAction("Edit", new { boxDate = newRec.Date });
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
            var _boxDate = AppRoutines.GetFirstWeekdayDate(month, year);
            return RedirectToAction("Edit", new { boxDate = _boxDate });
        }
        public ActionResult NextMonth(int month, int year)
        {
            month = month + 1;
            if (month > 12)
            {
                month = 1;
                year = year + 1;
            }
            var _boxDate = AppRoutines.GetFirstWeekdayDate(month, year);
            return RedirectToAction("Edit", new { boxDate = _boxDate });
        }

        private List<SelectListItem> GetODIdSelectList()
        {
            if (Session["ODSelectList"] == null)
            {
                var odList = new List<SelectListItem>();
                var odDataList = new List<ApplicationUser>();
                var _db = new BHelpContext();
                var userList = _db.Users.OrderBy(u => u.LastName).ToList();
                var roleLookup = AppRoutines.UsersInRolesLookup();
                var odRoleId = AppRoutines.GetRoleId("OfficerOfTheDay");
                
                odList.Add(new SelectListItem()
                {
                    Text = @"(nobody yet)",
                    Value = "0"
                });
                odDataList.Add(new ApplicationUser()
                {
                    Id = "0" // added so indexes of odDataList match odList 
                });
                foreach (var user in userList)
                {
                    if (roleLookup.Any(r => r.UserId == user.Id && r.RoleId == odRoleId))
                    {
                        odList.Add(new SelectListItem()
                        {
                            Text = user.FirstName + @" " + user.LastName,
                            Value = user.Id,
                            Selected = false
                        });
                        odDataList.Add(user);
                    };
                }
                Session["ODSelectList"] = odList;
                Session["ODDataList"] = odDataList;

                if (!User.IsInAnyRoles("Scheduler", "Developer", "Administrator")) // is NOT Scheduler
                {
                    var nonSchedulerODSelectList = new List<SelectListItem>
                    {
                        new SelectListItem()
                        {
                            Text = @"(nobody yet)",
                            Value = "0"
                        }
                    };
                    var currentUserId = User.Identity.GetUserId();
                    // get user's record from odDataList
                    var userData = odDataList.FirstOrDefault(i => i.Id == currentUserId);
                    if (userData != null)
                    {
                        var userDataSelectItem = new SelectListItem()
                        {
                            Text = userData.FullName,
                            Value = currentUserId
                        };
                        nonSchedulerODSelectList.Add(userDataSelectItem);
                    }
                    Session["NonSchedulerODSelectList"] = nonSchedulerODSelectList;
                }

                return odList;
            }

            return null;
        }

        private static List<ODSchedule> GetMonthlyList(int month, int year)
        {
            var start = new DateTime(year, month, 1);
            var end = new DateTime(year, month, DateTime.DaysInMonth(year, month));
            var db = new BHelpContext();
            return db.ODSchedules
                .Where(d => d.Date >= start && d.Date <= end).OrderBy(d => d.Date).ToList();
        }

        [Authorize(Roles = "Developer,Administrator,Staff,Scheduler,OfficerOfTheDay")]
        public ActionResult ODScheduleToExcel()
        {
            var dateData = Session["ODScheduleDateData"].ToString();
            var month = Convert.ToInt32(dateData.Substring(2, 2));
            var year = Convert.ToInt32(dateData.Substring(4, 4));
            var startDt = new DateTime(year, month, 1);
            var endDate = new DateTime(year, month, DateTime.DaysInMonth(year, month));
            var startDayOfWk = (int)startDt.DayOfWeek;
            var monthlyList = GetMonthlyList(month, year);
            var odList = (List<SelectListItem>)Session["ODList"];
            var odDataList = (List<ApplicationUser>)Session["ODDataList"];
            var BoxDay = new DateTime[6, 6];
            var BoxODName = new string[26];
            var BoxODPhone = new string[26];
            var BoxODPhone2 = new string[26];
            var BoxODEmail = new string[26];
            var BoxNote = new string[26];

            for (var i = 1; i < 6; i++)
            {
                for (var j = 1; j < 6; j++)
                {
                    var idx = j + 5 * (i - 1);
                    if (i == 1)
                    {
                        if (j < startDayOfWk) continue;
                        BoxDay[i, j] = startDt.AddDays(j - startDayOfWk);
                        var mIdx = monthlyList.FindIndex(d => d.Date == BoxDay[i, j]);
                        if (mIdx >= 0)  // mIdx = -1 if match not found
                        {
                            var odIdx = odList.FindIndex(d => d.Value == monthlyList[mIdx].ODId);
                            if (odIdx >= 0)
                            {
                                BoxODName[idx] = odList[odIdx].Text;
                                if (odDataList[odIdx].PhoneNumber != null) BoxODPhone[idx] = odDataList[odIdx].PhoneNumber;
                                if (odDataList[odIdx].PhoneNumber2 != null) BoxODPhone2[idx] = odDataList[odIdx].PhoneNumber2;
                                if (odDataList[odIdx].Email != null) BoxODEmail[idx] = odDataList[odIdx].Email;
                            }
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
                            var odIdx = odList.FindIndex(d => d.Value == monthlyList[mIdx].ODId);
                            if (odIdx >= 0)
                            {
                                BoxODName[idx] = odList[odIdx].Text;
                                if (odDataList[odIdx].PhoneNumber != null) BoxODPhone[idx] = odDataList[odIdx].PhoneNumber;
                                if (odDataList[odIdx].PhoneNumber2 != null) BoxODPhone2[idx] = odDataList[odIdx].PhoneNumber2;
                                if (odDataList[odIdx].Email != null) BoxODEmail[idx] = odDataList[odIdx].Email;
                            }
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
                                var odIdx = odList.FindIndex(d => d.Value == monthlyList[mIdx].ODId);
                                if (odIdx >= 0)
                                {
                                    BoxODName[idx] = odList[odIdx].Text;
                                    if (odDataList[odIdx].PhoneNumber != null) BoxODPhone[idx] = odDataList[odIdx].PhoneNumber;
                                    if (odDataList[odIdx].PhoneNumber2 != null) BoxODPhone2[idx] = odDataList[odIdx].PhoneNumber2;
                                    if (odDataList[odIdx].Email != null) BoxODEmail[idx] = odDataList[odIdx].Email;
                                }
                                BoxNote[idx] = monthlyList[mIdx].Note;
                            }
                        }
                    }
                }
            }

            var workbook = new XLWorkbook();
            IXLWorksheet ws = workbook.Worksheets.Add("OD Schedule");

            var tempDate = new DateTime(year, month, 1);
            var monthName = tempDate.ToString("MMMM");

            int row = 1;
            ws.Columns("1").Width = 20;
            ws.Columns("2").Width = 20;
            ws.Columns("3").Width = 20;
            ws.Columns("4").Width = 20;
            ws.Columns("5").Width = 20;
            ws.Cell(row, 1).SetValue("BHelp OD Schedule - " + monthName + " " + year);

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
                        if (BoxODName[idx] != null)
                        {
                            boxContents += Environment.NewLine + "OD:";
                            boxContents += Environment.NewLine + BoxODName[idx];
                        }

                        if (BoxODPhone[idx] != null)
                        {
                            boxContents += Environment.NewLine + BoxODPhone[idx];
                        }
                        if (BoxODPhone2[idx] != null)
                        {
                            boxContents += Environment.NewLine + BoxODPhone2[idx];
                        }

                        if (BoxODEmail[idx] != null)
                        {
                            boxContents += Environment.NewLine + BoxODEmail[idx];
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
            { FileDownloadName = "BHelpODSchedule " + tempDate.ToString("MM") + "-" + tempDate.ToString("yyyy") + ".xlsx" };
        }
    }
}