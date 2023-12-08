using BHelp.DataAccessLayer;
using BHelp.Models;
using BHelp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Org.BouncyCastle.Utilities;

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
                // Returning with Bagger change:
                if (baggerId != null)
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
                    // Update record
                    if (schedule.BaggerId == "0") schedule.BaggerId = null;
                    rec.BaggerId = schedule.BaggerId;
                    rec.Note = schedule.Note;
                    db.SaveChanges();

                    // update session variable:
                    var msUpdate = (List<BaggerSchedule>)Session["MonthlyBaggerSchedule"];
                    foreach (var item in msUpdate.Where(s => s.Date == schedule.Date))
                    {
                        item.BaggerId = schedule.BaggerId;
                        item.Note = schedule.Note;
                    }

                    Session["MonthlyBaggerSchedule"] = msUpdate;

                    return RedirectToAction("Edit", new { boxDate = schedule.Date, baggerid = schedule.BaggerId });
                }

                // Add new record
                if (schedule.BaggerId == "0") schedule.BaggerId = null;
                var newRec = new BaggerSchedule()
                {
                    Date = schedule.Date,
                    BaggerId = schedule.BaggerId,
                    PartnerId = schedule.PartnerId,
                    Note = schedule.Note
                };
                db.BaggerSchedules.Add(newRec);
                db.SaveChanges();

                // update session variable:
                var msAdd = (List<BaggerSchedule>)Session["MonthlyBaggerSchedule"];
                var newMsAdd = new BaggerSchedule()
                {
                    BaggerId = schedule.BaggerId,
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
                    Text = @"(nobody yet)",
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
                            Text = user.FirstName + @" " + user.LastName,
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
                                    Text = @"(nobody yet)",
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
                    if (mIdx >= 0) // mIdx = -1 if match not found
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

    }
}