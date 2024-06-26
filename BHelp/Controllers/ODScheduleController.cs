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
    public class ODScheduleController : Controller
    {
        // GET: ODSchedule
        [AllowAnonymous]
        public ActionResult Edit(DateTime? boxDate, string odId)
        {
            GetODLookUpLists(boxDate);
            if (Session["ODScheduleDateData"] == null)
            {
                var month = DateTime.Today.Month;
                var year = DateTime.Today.Year;
                var day = AppRoutines.GetFirstWeekdayDate(month, year);
                Session["ODScheduleDateData"] = day.Day.ToString("00") + month.ToString("00") + year;
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
                    boxDate = new DateTime(year, month, day);
                    Session["ODScheduleDateData"] = day.ToString("00") + month.ToString("00") + year;
                }
                if (boxDate == null)
                {
                    var month = DateTime.Today.Month;
                    var year = DateTime.Today.Year;
                    var date = AppRoutines.GetFirstWeekdayDate(month, year);
                    Session["ODScheduleDateData"] = date.Day.ToString("00") + month.ToString("00") + year;
                }
                else  // boxDate has value
                {
                    var date = (DateTime)boxDate;
                    var month = date.Month;
                    var year = date.Year;
                    Session["ODScheduleDateData"] = date.Day.ToString("00") + month.ToString("00") + year;
                }
            }

            var view = GetODScheduleViewModel();

            if (User.IsInAnyRoles("ODScheduler", "Developer", "Administrator"))
            {
                var cutOffDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                if (view.Date >= cutOffDate || User.IsInAnyRoles("Developer", "Administrator"))
                {
                    view.AllowEdit = true;
                }

                if (User.IsInAnyRoles("ODScheduler", "Developer", "Administrator"))
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
            if (Session["ODSelectList"] == null)
            {
                GetODLookUpLists(boxDate);  // also gets ODDataList, NonSchedulerODSelectList 
            }

            var odList = (List<SelectListItem>)Session["ODSelectList"];  //GetODLookUpLists();
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
            using var db = new BHelpContext();
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

        // POST: ODSchedule/Edit
        [HttpPost,AllowAnonymous,ValidateAntiForgeryToken]
        public ActionResult Edit(ODScheduleViewModel schedule)
        {
            if (ModelState.IsValid)
            {
                using var db = new BHelpContext();
                // Check if date exists & update
                var rec = db.ODSchedules
                    .FirstOrDefault(d => d.Date == schedule.Date);
                if (rec != null)
                {
                    // Update record
                    if (schedule.ODId == "0") schedule.ODId = null;
                    rec.ODId = schedule.ODId;
                    rec.Note = schedule.Note;
                    db.SaveChanges();

                    // update session variable:
                    var msUpdate = (List<ODSchedule>)Session["MonthlyODSchedule"];
                    foreach (var item in msUpdate.Where(s => s.Date == schedule.Date))
                    {
                        item.ODId = schedule.ODId;
                        item.Note = schedule.Note;
                    }

                    Session["MonthlyODSchedule"] = msUpdate;

                    return RedirectToAction("Edit", new { boxDate = schedule.Date, odid = schedule.ODId });
                }

                // Add new record
                if (schedule.ODId == "0") schedule.ODId = null;
                var newRec = new ODSchedule()
                {
                    Date = schedule.Date,
                    ODId = schedule.ODId,
                    Note = schedule.Note
                };
                db.ODSchedules.Add(newRec);
                db.SaveChanges();


                // update session variable:
                var msAdd = (List<ODSchedule>)Session["MonthlyODSchedule"];
                var newMsAdd = new ODSchedule()
                {
                    ODId = schedule.ODId,
                    Date = schedule.Date
                };
                msAdd.Add(newMsAdd);
                Session["MonthlyODSchedule"] = msAdd;

                return RedirectToAction("Edit", new { boxDate = newRec.Date, odId = schedule.ODId });
            }
            else
            {
                return RedirectToAction("Edit");
            }
        }

        // GET: ODSchedule/Individual Signup
        public ActionResult Individual(DateTime? boxDate)
        {
            if(boxDate == null)
            {
                var month = DateTime.Today.Month;
                var year = DateTime.Today.Year;
                var day = AppRoutines.GetFirstWeekdayDate(month, year);
                boxDate =DateTime.Today;
                Session["ODScheduleDateData"] = day.Day.ToString("00") + month.ToString("00") + year;
            }
            else
            {
                var month = boxDate.GetValueOrDefault().Month;
                var year = boxDate.GetValueOrDefault().Year;
                var day = AppRoutines.GetFirstWeekdayDate(month, year);
                Session["ODScheduleDateData"] = day.Day.ToString("00") + month.ToString("00") + year;
            }
            GetODLookUpLists(boxDate);
            var view = GetODScheduleViewModel();
                view.TodayYearMonth = DateTime.Today.Year * 100 + DateTime.Today.Month;
                var schedules = new List<ODScheduleViewModel>();
                var startDt = AppRoutines.GetFirstWeekDay(view.Month, view.Year);
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

                    var schedule = new ODScheduleViewModel
                    {
                        Id = i,
                        Date = dt,
                        ODId = "0",
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

                view.ODsSchedule = schedules;
                return View(view);
         }

            public ActionResult ODSignUp(int? idx, DateTime? date, bool? cancel)
            {
                var CurrentUserId = User.Identity.GetUserId();
                var text = AppRoutines.GetUserFullName(User.Identity.GetUserId());
                // check for existing ODSchedules record
                using var db = new BHelpContext();
                var rec = db.ODSchedules
                    .FirstOrDefault(od => od.Date == date);
                if (rec != null)
                {
                    if (cancel == true)
                    {
                        rec.ODId = null;
                        rec.Note = null;
                        text += " has canceled as OD for " + date?.ToString("MM/dd/yyyy");
                    }
                    else // cancel = false:
                    {
                        rec.ODId = CurrentUserId;
                        rec.Note = null;
                        text += " has signed up as OD for " + date?.ToString("MM/dd/yyyy");
                    }
                    db.SaveChanges();
                }
                else  // no existing rec:
                {
                    if (date != null)
                    {
                        var newRec = new ODSchedule() 
                        {
                            Date = (DateTime)date,
                            ODId  = CurrentUserId
                        };
                        db.ODSchedules.Add(newRec);
                        text += " has signed up as OD for " + date.Value.ToString("MM/dd/yyyy");
                    }
                    db.SaveChanges();
                }
                SendEmailToODScheduler(text);

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

            private void  GetODLookUpLists(DateTime? boxDate)
            {   // AND ODDataList AND NonSchedulerODSelectList
                var _dt = DateTime.Today;
                if (boxDate != null)
                {
                    _dt = (DateTime)boxDate;
                }
                var _mo = _dt.Month;
                var _yr = _dt.Year;
                SetMonthlyList(_mo, _yr); // sets Session["MonthlyODSchedule"]
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

                        if (!User.IsInAnyRoles("ODScheduler", "Developer", "Administrator")) // is NOT Scheduler
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
                }
            }

            private void SetMonthlyList(int month, int year)
            {
                var start = new DateTime(year, month, 1);
                var end = new DateTime(year, month, DateTime.DaysInMonth(year, month));
                //var ml = (List<ODSchedule>)Session["MonthlyODSchedule"];
                //if (Session["MonthlyODSchedule"] == null || ml.Count == 0)
                //{
                    var db = new BHelpContext();
                    var monthlyList = db.ODSchedules
                        .Where(d => d.Date >= start && d.Date <= end).OrderBy(d => d.Date).ToList();
                    Session["MonthlyODSchedule"] = monthlyList;
                //}
                //else
                //{  // use existing list:
                //    var monthlyList = (List<ODSchedule>)Session["MonthlyODSchedule"];
                //    if (monthlyList[0].Date.Month != month || monthlyList[0].Date.Year != year)
                //    {  // reload:
                //        var db = new BHelpContext();
                //        monthlyList = db.ODSchedules
                //            .Where(d => d.Date >= start && d.Date <= end).OrderBy(d => d.Date).ToList();
                //        Session["MonthlyODSchedule"] = monthlyList;
                //    }
                //}
            }

            [Authorize(Roles = "Developer,Administrator,Staff,ODScheduler,OfficerOfTheDay")]
            public ActionResult ODScheduleToExcel()
            {
                var view = GetODScheduleViewModel();

                var workbook = new XLWorkbook();
                IXLWorksheet ws = workbook.Worksheets.Add("OD Schedule");

                var tempDate = new DateTime(view.Year, view.Month , 1);
                var monthName = tempDate.ToString("MMMM");

                int row = 1;
                ws.Columns("1").Width = 20;
                ws.Columns("2").Width = 20;
                ws.Columns("3").Width = 20;
                ws.Columns("4").Width = 20;
                ws.Columns("5").Width = 20;
                ws.Cell(row, 1).SetValue("BHelp OD Schedule - " + monthName + " " + view.Year);

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
                        if (view.BoxDay[i, j] > DateTime.MinValue)
                        {
                            var idx = j + 5 * (i - 1);
                            var boxContents = (view.BoxDay[i, j].Day.ToString("0"));

                            if (view.BoxODName[idx] == null)
                            {
                                boxContents += Environment.NewLine + "OD: TBD";
                            }
                            else
                            {
                                boxContents += Environment.NewLine + "OD:";
                                boxContents += Environment.NewLine + view.BoxODName[idx];
                            }

                            if (view.BoxODPhone[idx] != null)
                            {
                                boxContents += Environment.NewLine + view.BoxODPhone[idx];
                            }
                            if (view.BoxODPhone2[idx] != null)
                            {
                                boxContents += Environment.NewLine + view.BoxODPhone2[idx];
                            }

                            if (view.BoxODEmail[idx] != null)
                            {
                                boxContents += Environment.NewLine + view.BoxODEmail[idx];
                            }

                            if (view.BoxNote[idx] != null)
                            {
                                boxContents += Environment.NewLine + "Note:";
                                boxContents += Environment.NewLine + view.BoxNote[idx];
                            }

                            if (view.BoxHoliday[idx])
                            {
                                boxContents += Environment.NewLine + view.BoxHolidayDescription[idx];
                            }

                            ws.Cell(row, j).Value = "Bold";
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
            private Holiday GetHolidayData(DateTime dt)
            {
                var holidays = HolidayRoutines.GetHolidays(dt.Year);
                return holidays.FirstOrDefault(holiday => dt == holiday.FixedDate);
            }

            private ODScheduleViewModel GetODScheduleViewModel()
            {
                var view = new ODScheduleViewModel()
                {
                    CurrentUserId = User.Identity.GetUserId(),
                    BoxDay = new DateTime[6, 6],
                    BoxODId = new string[26],
                    BoxODName = new string[26],
                    BoxODPhone = new string[26],
                    BoxODPhone2 = new string[26],
                    BoxODEmail = new string[26],
                    BoxNote = new string[26],
                    BoxHoliday = new bool[26],
                    BoxHolidayDescription = new string[26]
                };
                var dateData = Session["ODScheduleDateData"].ToString();
                view.Month = Convert.ToInt32(dateData.Substring(2, 2));
                view.Year = Convert.ToInt32(dateData.Substring(4, 4));
                view.Date = new DateTime(view.Year, view.Month, Convert.ToInt32(dateData.Substring(0, 2)));
                view.MonthName = Strings.ToUpperCase(view.Date.ToString("MMMM"));
                var startDate = new DateTime(view.Year, view.Month, 1);
                var endDate = new DateTime(view.Year, view.Month, DateTime.DaysInMonth(view.Year, view.Month));
                var startDayOfWk = (int)startDate.DayOfWeek;
                if (startDayOfWk == 6) startDayOfWk = -1;
                var monthlyList = (List<ODSchedule>)Session["MonthlyODSchedule"];
                var odList = (List<SelectListItem>)Session["ODSelectList"];
                var odDataList = (List<ApplicationUser>)Session["ODDataList"];
                var holidayList = HolidayRoutines.GetHolidays(view.Year);
                for (var i = 1; i < 6; i++)
                {
                    for (var j = 1; j < 6; j++)
                    {
                        view.BoxDay[i, j] = startDate.AddDays(7 * (i - 1) + j - startDayOfWk);
                        if (view.BoxDay[i, j] < startDate || view.BoxDay[i, j] > endDate) continue;
                        var idx = j + 5 * (i - 1);
                        if (HolidayRoutines.IsHoliday(view.BoxDay[i, j], holidayList))
                        {
                            view.BoxHoliday[idx] = true;
                            var holidayData = GetHolidayData(view.BoxDay[i, j]);
                            view.BoxHolidayDescription[idx] = holidayData.Description + Environment.NewLine + "BH Closed";
                        }

                        var mIdx = monthlyList.FindIndex(d => d.Date == view.BoxDay[i, j]);
                        if (mIdx >= 0) // mIdx = -1 if match not found
                        {
                            var odIdx = odList.FindIndex(d => d.Value == monthlyList[mIdx].ODId);
                            if (odIdx >= 0)
                            {
                                view.BoxODName[idx] = odList[odIdx].Text;
                                view.BoxODId[idx] = odList[odIdx].Value;
                                view.BoxODPhone[idx] = odDataList[odIdx].PhoneNumber;
                                view.BoxODPhone2[idx] = odDataList[odIdx].PhoneNumber2;
                                view.BoxODEmail[idx] = odDataList[odIdx].Email;
                            }
                            view.BoxNote[idx] = monthlyList[mIdx].Note;
                        }
                    }
                }

                return view;
            }

            public ActionResult PreviousMonthIndividualOD(int month, int year)
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

            public ActionResult NextMonthIndividualOD(int month, int year)
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

            private static void SendEmailToODScheduler(string text)
            {
                var roleId = AppRoutines.GetRoleId("ODScheduler");
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
                msg.From = new MailAddress("ODScheduler@BethesdaHelpFD.org", "BHELP OD Scheduler");
                msg.To.Add(new MailAddress(address, "BHELP OD"));
                msg.Subject = "BHELP - OD Schedule";
                msg.Body = text;

                msg.Priority = MailPriority.Normal;
                using var mailClient = new SmtpClient("BethesdaHelpFD.org", 587);
                mailClient.Credentials = new NetworkCredential("ODScheduler@BethesdaHelpFD.org", "nq!aeyu9Gc_Ebm2aoP@vNNnPi");
                mailClient.Send(msg);
            }
    }
}