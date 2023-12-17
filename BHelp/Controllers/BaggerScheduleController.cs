using BHelp.DataAccessLayer;
using BHelp.Models;
using BHelp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using ClosedXML.Excel;
using System.IO;
using Strings = Org.BouncyCastle.Utilities.Strings;
using System.Net.Mail;
using System.Net;

namespace BHelp.Controllers
{
    public class BaggerScheduleController : Controller
    {
        // GET: BaggerSchedule
        [AllowAnonymous]
        public ActionResult Edit(DateTime? boxDate, string baggerId)
        {
            GetBaggerLookUpLists(boxDate);
            if (Session["BaggerScheduleDateData"] == null)
            {
                var month = DateTime.Today.Month;
                var year = DateTime.Today.Year;
                var day = AppRoutines.GetFirstWeekdayDate(month, year);
                Session["BaggerScheduleDateData"] = day.Day.ToString("00") + month.ToString("00") + year;
            }
            else  // returning to BaggerSchedule
            {
                if (baggerId != null) // Returning with Bagger change:
                {
                    var dateData = Session["BaggerScheduleDateData"].ToString();
                    var day = Convert.ToInt32(dateData.Substring(0, 2));
                    var month = Convert.ToInt32(dateData.Substring(2, 2));
                    var year = Convert.ToInt32(dateData.Substring(4, 4));
                    boxDate = new DateTime(year, month, day);
                    Session["BaggerScheduleDateData"] = day.ToString("00") + month.ToString("00") + year;
                }
                if (boxDate == null)
                {
                    var month = DateTime.Today.Month;
                    var year = DateTime.Today.Year;
                    var date = AppRoutines.GetFirstWeekdayDate(month, year);
                    Session["BaggerScheduleDateData"] = date.Day.ToString("00") + month.ToString("00") + year;
                }
                else  // boxDate has value
                {
                    var date = (DateTime)boxDate;
                    var month = date.Month;
                    var year = date.Year;
                    Session["BaggerScheduleDateData"] = date.Day.ToString("00") + month.ToString("00") + year;
                }
            }

            var view = GetBaggerScheduleViewModel();

            if (User.IsInAnyRoles("DriverScheduler", "Developer", "Administrator"))
            {
                var cutOffDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                if (view.Date >= cutOffDate || User.IsInAnyRoles("Developer", "Administrator"))
                {
                    view.AllowEdit = true;
                }

                if (User.IsInAnyRoles("BaggerScheduler", "Developer", "Administrator"))
                {
                    view.IsScheduler = true;
                }
                else
                {
                    view.IsBaggerOnly = true;
                }
            }

            var startDt = AppRoutines.GetFirstWeekdayDate(view.Month, view.Year);
            var endDate = new DateTime(view.Year, view.Month, DateTime.DaysInMonth(view.Year, view.Month));
            var startDayOfWk = (int)startDt.DayOfWeek;
            if (Session["BaggerSelectList"] == null)
            {
                GetBaggerLookUpLists(boxDate);  // also gets BaggerDataList, NonSchedulerBaggerSelectList 
            }

            var baggerList = (List<SelectListItem>)Session["BaggerSelectList"];  //GetBaggerLookUpLists();
            var baggerDataList = (List<ApplicationUser>)Session["BaggerDataList"];
            if (view.IsScheduler)
            {
                view.BaggerList = baggerList;
            }
            else
            {
                view.BaggerList = (List<SelectListItem>)Session["NonSchedulerBaggerSelectList"];
            }
            // Check for existing record
            using var db = new BHelpContext();
            var existingRec = db.BaggerSchedules.FirstOrDefault(r => r.Date == view.Date);
            if (existingRec != null)
            {
                if (existingRec.BaggerId != null)
                {
                    view.BaggerId = existingRec.BaggerId;
                    view.PartnerId = existingRec.PartnerId;
                    view.OldBaggerId = existingRec.BaggerId;
                    var baggerIdx = baggerList.FindIndex(d => d.Value == view.BaggerId);
                    if (baggerIdx > 0) view.BaggerName = baggerDataList[baggerIdx].FullName;
                }

                view.Note = existingRec.Note;
            }

            var schedules = new List<BaggerScheduleViewModel>();
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

                var monthlyList = (List<BaggerSchedule>)Session["MonthlyBaggerSchedule"];
                var schedule = new BaggerScheduleViewModel
                {
                    Id = i,
                    Date = dt,
                    BaggerId = "0",
                    PartnerId = "0",
                    MonthName = view.MonthName,
                    Note = view.Note
                };
                if (dt > DateTime.MinValue)
                {
                    schedule.DayString = dt.Day.ToString("0"); // default
                    if (IsFriSatSun(dt))  // substitute FriSatSun for day of week
                    {
                        // check for existing Fri-Sat-Sun:
                        var chkDt = dt;
                        var mIdx = monthlyList.FindIndex(d => d.Date == chkDt); //Friday
                        if (mIdx >= 0)
                        {
                            schedule.DayString = chkDt.ToString("dddd" + " " + chkDt.ToString("MM/dd/yyyy"));
                        }

                        if (mIdx == -1)
                        {
                            chkDt = chkDt.AddDays(1); // Saturday
                            mIdx = monthlyList.FindIndex(d => d.Date == chkDt);
                            if (mIdx >= 0)
                            {
                                schedule.DayString = chkDt.ToString("dddd" + " " + chkDt.ToString("MM/dd/yyyy"));
                            }
                        }

                        if (mIdx == -1)
                        {
                            chkDt = chkDt.AddDays(1); // Sunday
                            mIdx = monthlyList.FindIndex(d => d.Date == chkDt);
                            if (mIdx >= 0)
                            {
                                schedule.DayString = chkDt.ToString("dddd" + " " + chkDt.ToString("MM/dd/yyyy"));
                            }
                        }
                    }
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

            view.BaggersSchedule = schedules;

            return View(view);
        }

        // POST: BaggerSchedule/Edit
        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public ActionResult Edit(BaggerScheduleViewModel schedule)
        {
            if (ModelState.IsValid)
            {
                using var db = new BHelpContext();
                // Check if date exists & update
                var rec = db.BaggerSchedules
                    .FirstOrDefault(d => d.Date == schedule.Date);
                if (rec != null)
                {

                    if (schedule.BaggerId == "0") // Remove record
                    {
                        db.BaggerSchedules.Remove(rec);
                        db.SaveChanges();
                        // update session variable:
                        var msRemove = (List<BaggerSchedule>)Session["MonthlyBaggerSchedule"];
                        var removeIdx = msRemove.FindIndex(d => d.Date == schedule.Date);
                        if (removeIdx >= 0) msRemove.RemoveAt(removeIdx);
                        Session["MonthlyBaggerSchedule"] = msRemove;
                        return RedirectToAction("Edit", new { boxDate = schedule.Date, baggerid = "0" });
                    }
                    else // Update record
                    {
                        rec.Date = schedule.Date;
                        rec.BaggerId = schedule.BaggerId;
                        rec.PartnerId = schedule.PartnerId;
                        rec.Note = schedule.Note;
                        db.SaveChanges();
                        // update session variable:
                        var msUpdate = (List<BaggerSchedule>)Session["MonthlyBaggerSchedule"];
                        foreach (var item in msUpdate.Where(s => s.Date == schedule.Date))
                        {
                            item.BaggerId = schedule.BaggerId;
                            item.PartnerId = schedule.PartnerId;
                            item.Note = schedule.Note;
                        }

                        Session["MonthlyBaggerSchedule"] = msUpdate;
                    }
                }

                // Add new record
                var newRec = new BaggerSchedule()
                {
                Date = schedule.Date,
                BaggerId = schedule.BaggerId,
                PartnerId = schedule.PartnerId,
                Note = schedule.Note
                };
                db.BaggerSchedules.Add(newRec);
                db.SaveChanges();

                // if new record is a Fri-Sat-Sun, erase any previous Fri-Sat-Sun records:
                if (IsFriSatSun(schedule.Date))
                {
                    var prevFri = schedule.Date.AddDays(-1);
                    var prevSat = schedule.Date.AddDays(-2);
                    var prevSun = schedule.Date.AddDays(-3);
                    var prevFriRec = db.BaggerSchedules.FirstOrDefault(d => d.Date == prevFri);
                    if (prevFriRec != null && prevFriRec.Date != schedule.Date) db.BaggerSchedules.Remove(prevFriRec);
                    var prevSatRec = db.BaggerSchedules.FirstOrDefault(d => d.Date == prevSat);
                    if (prevSatRec != null && prevSatRec.Date != schedule.Date) db.BaggerSchedules.Remove(prevSatRec);
                    var prevSunRec = db.BaggerSchedules.FirstOrDefault(d => d.Date == prevSun);
                    if (prevSunRec != null && prevSunRec.Date != schedule.Date) db.BaggerSchedules.Remove(prevSunRec);
                    db.SaveChanges();
                }
                
                // update session variable:
                var msAdd = (List<BaggerSchedule>)Session["MonthlyBaggerSchedule"];
                var newMsAdd = new BaggerSchedule()
                {
                    BaggerId = schedule.BaggerId,
                    PartnerId = schedule.PartnerId,
                    Date = schedule.Date
                };
                msAdd.Add(newMsAdd);
                Session["MonthlyBaggerSchedule"] = msAdd;

                return RedirectToAction("Edit", new { boxDate = newRec.Date, baggerId = schedule.BaggerId });
            }
            else
            {
                return RedirectToAction("Edit");
            }
        }

        // GET: zbsggerSchedule/Individual Signup
        public ActionResult Individual(DateTime? boxDate)
        {
            if (boxDate == null)
            {
                var month = DateTime.Today.Month;
                var year = DateTime.Today.Year;
                var day = AppRoutines.GetFirstWeekdayDate(month, year);
                boxDate = DateTime.Today;
                Session["BaggerScheduleDateData"] = day.Day.ToString("00") + month.ToString("00") + year;
            }
            else
            {
                var month = boxDate.GetValueOrDefault().Month;
                var year = boxDate.GetValueOrDefault().Year;
                var day = AppRoutines.GetFirstWeekdayDate(month, year);
                Session["BaggerScheduleDateData"] = day.Day.ToString("00") + month.ToString("00") + year;
            }
            GetBaggerLookUpLists(boxDate);
            var view = GetBaggerScheduleViewModel();
            view.TodayYearMonth = DateTime.Today.Year * 100 + DateTime.Today.Month;
            var schedules = new List<BaggerScheduleViewModel>();
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

                var schedule = new BaggerScheduleViewModel
                {
                    Id = i,
                    Date = dt,
                    BaggerId = "0",
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

            view.BaggersSchedule = schedules;
            return View(view);
        }

        public ActionResult BaggerSignUp(int? idx, DateTime? date, bool? cancel)
        {
            var CurrentUserId = User.Identity.GetUserId();
            var text = AppRoutines.GetUserFullName(User.Identity.GetUserId());
            // check for existing BaggerSchedules record
            using var db = new BHelpContext();
            var rec = db.BaggerSchedules
                .FirstOrDefault(od => od.Date == date);
            if (rec != null)
            {
                if (cancel == true)
                {
                    rec.BaggerId = null;
                    rec.Note = null;
                    text += " has canceled as Bagger for " + date?.ToString("MM/dd/yyyy");
                }
                else // cancel = false:
                {
                    rec.BaggerId = CurrentUserId;
                    rec.Note = null;
                    text += " has signed up as Bagger for " + date?.ToString("MM/dd/yyyy");
                }
                db.SaveChanges();
            }
            else  // no existing rec:
            {
                if (date != null)
                {
                    var newRec = new BaggerSchedule()
                    {
                        Date = (DateTime)date,
                        BaggerId = CurrentUserId
                    };
                    db.BaggerSchedules.Add(newRec);
                    text += " has signed up as Bagger for " + date.Value.ToString("MM/dd/yyyy");
                }
                db.SaveChanges();
            }
            SendEmailToBaggerScheduler(text);

            return RedirectToAction("Individual", new { boxDate = date });
        }

        private static void SendEmailToBaggerScheduler(string text)
        {
            var roleId = AppRoutines.GetRoleId("BaggerScheduler");
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
            msg.From = new MailAddress("BaggerScheduler@BethesdaHelpFD.org", "BHELP Bagger Scheduler");
            msg.To.Add(new MailAddress(address, "BHELP Bagger"));
            msg.Subject = "BHELP - Bagger Schedule";
            msg.Body = text;

            msg.Priority = MailPriority.Normal;
            using var mailClient = new SmtpClient("BethesdaHelpFD.org", 587);
            mailClient.Credentials = new NetworkCredential("BaggerScheduler@BethesdaHelpFD.org", "nq!aeyu9Gc_Ebm2aoP@vNNnPi");
            mailClient.Send(msg);
        }

        private void GetBaggerLookUpLists(DateTime? boxDate)
        {   // AND BaggerDataList AND NonSchedulerBaggerSelectList
            var _dt = DateTime.Today;
            if (boxDate != null)
            {
                _dt = (DateTime)boxDate;
            }
            var _mo = _dt.Month;
            var _yr = _dt.Year;
            SetMonthlyList(_mo, _yr); // sets Session["MonthlyBaggerSchedule"]
            if (Session["BaggerSelectList"] == null)
            {
                var baggerList = new List<SelectListItem>();
                var baggerDataList = new List<ApplicationUser>();
                var _db = new BHelpContext();
                var userList = _db.Users.OrderBy(u => u.LastName).ToList();
                var roleLookup = AppRoutines.UsersInRolesLookup();
                var baggerRoleId = AppRoutines.GetRoleId("Bagger");

                baggerList.Add(new SelectListItem()
                {
                    Text = "(nobody yet)",
                    Value = "0"
                });
                baggerDataList.Add(new ApplicationUser()
                {
                    Id = "0" // added so indexes of baggerDataList match baggerList 
                });
                foreach (var user in userList)
                {
                    if (roleLookup.Any(r => r.UserId == user.Id && r.RoleId == baggerRoleId))
                    {
                        baggerList.Add(new SelectListItem()
                        {
                            Text = user.FirstName + " " + user.LastName,
                            Value = user.Id,
                            Selected = false
                        });
                        baggerDataList.Add(user);
                    };
                }
                Session["BaggerSelectList"] = baggerList;
                Session["BaggerDataList"] = baggerDataList;

                if (!User.IsInAnyRoles("BaggerScheduler", "Developer", "Administrator")) // is NOT Scheduler
                {
                    var nonSchedulerBaggerSelectList = new List<SelectListItem>
                            {
                                new SelectListItem()
                                {
                                    Text = "(nobody yet)",
                                    Value = "0"
                                }
                            };
                    var currentUserId = User.Identity.GetUserId();
                    // get user's record from baggerDataList
                    var userData = baggerDataList.FirstOrDefault(i => i.Id == currentUserId);
                    if (userData != null)
                    {
                        var userDataSelectItem = new SelectListItem()
                        {
                            Text = userData.FullName,
                            Value = currentUserId
                        };
                        nonSchedulerBaggerSelectList.Add(userDataSelectItem);
                    }
                    Session["NonSchedulerBaggerSelectList"] = nonSchedulerBaggerSelectList;
                }
            }
        }

        private void SetMonthlyList(int month, int year)
        {
            var start = new DateTime(year, month, 1);
            var end = new DateTime(year, month, DateTime.DaysInMonth(year, month));
            var db = new BHelpContext();
            var monthlyList = db.BaggerSchedules
                .Where(d => d.Date >= start && d.Date <= end).OrderBy(d => d.Date).ToList();
            Session["MonthlyBaggerSchedule"] = monthlyList;
        }

        private BaggerScheduleViewModel GetBaggerScheduleViewModel()
        {
            var view = new BaggerScheduleViewModel()
            {
                CurrentUserId = User.Identity.GetUserId(),
                BoxDay = new DateTime[6, 6],
                BoxBaggerId = new string[26],
                BoxBaggerName = new string[26],
                BoxBaggerPhone = new string[26],
                BoxBaggerPhone2 = new string[26],
                BoxBaggerEmail = new string[26],

                BoxPartnerId = new string[26],
                BoxPartnerName = new string[26],
                BoxPartnerPhone = new string[26],
                BoxPartnerPhone2 = new string[26],
                BoxPartnerEmail = new string[26],

                BoxNote = new string[26],
                BoxHoliday = new bool[26],
                BoxFriSatSun = new bool[26],
                BoxHolidayDescription = new string[26]
            };
            var dateData = Session["BaggerScheduleDateData"].ToString();
            view.Month = Convert.ToInt32(dateData.Substring(2, 2));
            view.Year = Convert.ToInt32(dateData.Substring(4, 4));
            view.Date = new DateTime(view.Year, view.Month, Convert.ToInt32(dateData.Substring(0, 2)));
            view.MonthName = Strings.ToUpperCase(view.Date.ToString("MMMM"));
            var startDate = new DateTime(view.Year, view.Month, 1);
            var endDate = new DateTime(view.Year, view.Month, DateTime.DaysInMonth(view.Year, view.Month));
            var startDayOfWk = (int)startDate.DayOfWeek;
            if (startDayOfWk == 6) startDayOfWk = -1;
            var monthlyList = (List<BaggerSchedule>)Session["MonthlyBaggerSchedule"];
            var baggerList = (List<SelectListItem>)Session["BaggerSelectList"];
            var baggerDataList = (List<ApplicationUser>)Session["BaggerDataList"];
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

                    if (view.BoxDay[i, j].DayOfWeek == (DayOfWeek)5 && mIdx == -1) // This is a Friday - put Fri-Sat-Sun records in this box ('idx')
                    {
                        var _boxday = view.BoxDay[i, j].AddDays(1); // Saturday
                        mIdx = monthlyList.FindIndex(d => d.Date == _boxday);
                        if (mIdx >= 0) { view.BoxDay[i, j] = _boxday;}

                        if (mIdx == -1) // look for Sunday record: 
                        {
                            _boxday = _boxday.AddDays(1); // Sunday
                            mIdx = monthlyList.FindIndex(d => d.Date == _boxday);
                            if (mIdx >= 0) { view.BoxDay[i, j] = _boxday; }
                        }
                    }

                    if (mIdx >= 0) // match found -  (mIdx = -1 if match not found)
                    {
                        var baggerIdx = baggerList.FindIndex(d => d.Value == monthlyList[mIdx].BaggerId);
                        if (baggerIdx >= 0)
                        {
                            view.BoxBaggerName[idx] = baggerList[baggerIdx].Text;
                            view.BoxBaggerId[idx] = baggerList[baggerIdx].Value;
                            view.BoxBaggerPhone[idx] = baggerDataList[baggerIdx].PhoneNumber;
                            view.BoxBaggerPhone2[idx] = baggerDataList[baggerIdx].PhoneNumber2;
                            view.BoxBaggerEmail[idx] = baggerDataList[baggerIdx].Email;
                        }

                        var partnerIdx = baggerList.FindIndex(d => d.Value == monthlyList[mIdx].PartnerId);
                        if (partnerIdx >= 0)
                        {
                            view.BoxPartnerName[idx] = baggerList[partnerIdx].Text;
                            view.BoxPartnerId[idx] = baggerList[partnerIdx].Value;
                            view.BoxPartnerPhone[idx] = baggerDataList[partnerIdx].PhoneNumber;
                            view.BoxPartnerPhone2[idx] = baggerDataList[partnerIdx].PhoneNumber2;
                            view.BoxPartnerEmail[idx] = baggerDataList[partnerIdx].Email;
                        }

                        view.BoxNote[idx] = monthlyList[mIdx].Note;
                    }
                }
            }

            return view;
        }

        private static Holiday GetHolidayData(DateTime dt)
        {
            var holidays = HolidayRoutines.GetHolidays(dt.Year);
            return holidays.FirstOrDefault(holiday => dt == holiday.FixedDate);
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

        [Authorize(Roles = "Developer,Administrator,Staff,BaggerScheduler,OfficerOfTheDay")]
        public ActionResult BaggerScheduleToExcel()
        {
            var view = GetBaggerScheduleViewModel();

            var workbook = new XLWorkbook();
            IXLWorksheet ws = workbook.Worksheets.Add("Bagger Schedule");

            var tempDate = new DateTime(view.Year, view.Month, 1);
            var monthName = tempDate.ToString("MMMM");

            int row = 1;
            ws.Columns("1").Width = 20;
            ws.Columns("2").Width = 20;
            ws.Columns("3").Width = 20;
            ws.Columns("4").Width = 20;
            ws.Columns("5").Width = 20;
            ws.Cell(row, 1).SetValue("BHelp Bagger Schedule - " + monthName + " " + view.Year);

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
            ws.Cell(row, 5).SetValue("FRI-SAT-SUN");

            for (var i = 1; i < 6; i++)
            {
                row++;
                for (var j = 1; j < 6; j++)
                {
                    if (view.BoxDay[i, j] > DateTime.MinValue)
                    {
                        var idx = j + 5 * (i - 1);
                        var boxContents = (view.BoxDay[i, j].Day.ToString("0"));

                        if (view.BoxBaggerName[idx] == null)
                        {
                            boxContents += Environment.NewLine + "Bagger: TBD";
                        }
                        else
                        {
                            boxContents += Environment.NewLine + "Bagger:";
                            boxContents += Environment.NewLine + view.BoxBaggerName[idx];
                        }

                        if (view.BoxBaggerPhone[idx] != null)
                        {
                            boxContents += Environment.NewLine + view.BoxBaggerPhone[idx];
                        }
                        if (view.BoxBaggerPhone2[idx] != null)
                        {
                            boxContents += Environment.NewLine + view.BoxBaggerPhone2[idx];
                        }

                        if (view.BoxBaggerEmail[idx] != null)
                        {
                            boxContents += Environment.NewLine + view.BoxBaggerEmail[idx];
                        }

                        if (view.BoxPartnerName[idx] != null)
                        {
                            boxContents += Environment.NewLine + "Partner:";
                            boxContents += Environment.NewLine + view.BoxPartnerName[idx];
                            if (view.BoxPartnerPhone[idx] != null)
                            {
                                boxContents += Environment.NewLine + view.BoxPartnerPhone[idx];
                            }
                            if (view.BoxPartnerPhone2[idx] != null)
                            {
                                boxContents += Environment.NewLine + view.BoxPartnerPhone2[idx];
                            }

                            if (view.BoxPartnerEmail[idx] != null)
                            {
                                boxContents += Environment.NewLine + view.BoxPartnerEmail[idx];
                            }

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
            { FileDownloadName = "BHelpBaggerSchedule " + tempDate.ToString("MM") + "-" + tempDate.ToString("yyyy") + ".xlsx" };
        }

        private static bool IsFriSatSun(DateTime dt)
        {
            return dt.DayOfWeek is DayOfWeek.Friday or DayOfWeek.Saturday or DayOfWeek.Sunday;
        }
    }
}
