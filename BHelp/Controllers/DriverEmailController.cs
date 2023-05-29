using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using BHelp.DataAccessLayer;
using BHelp.Models;
using BHelp.ViewModels;
using Microsoft.AspNet.Identity;

namespace BHelp.Controllers
{
    public class DriverEmailController : Controller
    {
        // GET: DriverEmail
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult DriverEmailDisplay(int year, int month)
        {
            if (Session["DriverList"] == null) Session["DriverList"] = GetDriverIdSelectList();
            if (Session["GroupList"] == null)
            {
                using var db = new BHelpContext();
                var groupList = db.GroupNames.OrderBy(n => n.Name).ToList();
                var items = new List<SelectListItem> { new SelectListItem { Text = @"(none)", Value = "0" } };
                foreach (var item in groupList)
                {
                    items.Add(new SelectListItem { Text = item.Name, Value = item.Id.ToString() });
                }

                Session["DriverGroupList"] = items;
            }

            var _dt = new DateTime(year, month, 1);
                    var email = new DriverEmailViewModel
                    {
                        Title = "Email Drivers",
                        MonthYear = _dt.ToString("MMMM") + " " + _dt.Year.ToString()
                    };

                    var text = "Dear Drivers,";
                    text += "<br />Please check current driver assignments to be sure it is correct. Note the many ";
                    text += "TBDs. If you are able to fill any of them, please let me know.";
                    text += "<br />IMPORTANT: You can also view the current schedule with driver contact info ";
                    text += "included at bethesdahelpfd.com";

                    email.EmailText = text;
                    //var startDate = DateTime.Today;
                    //var endDate = DateTime.Today;

                    email.HtmlContent = GetDriverSchedule(year, month);
                   
                    return View(email);
        }
        

        private string GetDriverSchedule(int year, int month)
        {
            var view = GetDriverScheduleViewModel(year, month);

            var qt = (char)34; // Quote
            var hdrStyle = " style= " + qt + "text-align:center; " + qt + " width = " + qt + "150px" + qt;
            var content = "<table border=1 style =" + qt + "border-collapse:collapse;" + qt + ">";
            content += "<tr><td" + hdrStyle + "><b>MONDAY</b></td>";
            content += "<td" + hdrStyle + "><b>TUESDAY</b></td>";
            content += "<td" + hdrStyle + "><b>WEDNESDAY</b></td>";
            content += "<td" + hdrStyle + "><b>THURSDAY</b></td>";
            content += "<td" + hdrStyle + "><b>FRIDAY</b></td></tr>";

            var holidays = HolidayRoutines.GetHolidays(year);
            var row = 0;
            for (var i = 1; i < 6; i++)
            { 
                row++;
                content += "<tr>";
                for (var col = 1; col < 6; col++)
                {
                    var idx = col + 5 * (row - 1);
                    content += "<td style= " + qt + "vertical-align:top" + qt + ">";
                    if (view.BoxDay[row, col] > DateTime.MinValue)
                    {
                        content += (view.BoxDay[row, col].Day.ToString("0"));
                        if (HolidayRoutines.IsHoliday(view.BoxDay[row, col], holidays))
                        {
                            var holidayData = GetHolidayData(view.BoxDay[row, col]);
                            content += "<br />&nbsp;" + holidayData.Description + "<br />&nbsp;BH Closed";
                            continue;
                        }

                        if (view.BoxDriverName[idx] != null)
                        {
                            content += "&nbsp;" + view.BoxDriverName[idx];
                        }
                        else
                        {
                            content += "&nbsp;TBD";
                        }

                        if (view.BoxBackupDriverName[idx] != null)
                        {
                            content += "<br /><br />B)&nbsp;" + view.BoxBackupDriverName[idx];
                        }
                        else
                        {
                            content += "<br /><br />B)&nbsp;TBD";
                        }

                        if (view.BoxGroupName[idx] != null)
                        {
                            content += "<br /><br />&nbsp;" + view.BoxGroupName[idx];
                        }

                        if (view.BoxGroupDriverName[idx] != null)
                        {
                            content += "<br />&nbsp;" + view.BoxGroupDriverName[idx];
                        }
                        else
                        {
                            if (view.BoxGroupName[idx] != null)
                            {
                                content += "<br />&nbsp;TBD";
                            }
                        }

                        content += "</td>";
                    }
                }

                content += "</tr>";
            }

            content += "</table>";

            return (content);
        }

        private static List<DriverSchedule> GetMonthlyList(int month, int year)
        {
            var start = new DateTime(year, month, 1);
            var end = new DateTime(year, month, DateTime.DaysInMonth(year, month));
            using var db = new BHelpContext();
            return db.DriverSchedules
                .Where(d => d.Date >= start && d.Date <= end).OrderBy(d => d.Date).ToList();
        }

        private DriverScheduleViewModel GetDriverScheduleViewModel(int year, int month)
        {
            var view = new DriverScheduleViewModel
            {
                BoxDay = new DateTime[6, 6],
                BoxODId = new string[26],
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
                BoxHolidayDescription = new string[26],
                BoxNote = new string[26],
                Month = month,
                Year = year
            };

            var startDate = new DateTime(view.Year, view.Month, 1);
            var endDate = new DateTime(view.Year, view.Month, DateTime.DaysInMonth(view.Year, view.Month));
            var startDayOfWk = (int)startDate.DayOfWeek;
            var monthlyList = GetMonthlyList(view.Month, view.Year);
            var driverList = (List<SelectListItem>)Session["DriverList"];
            var driverDataList = (List<ApplicationUser>)Session["DriverDataList"];
            view.GroupList = (List<SelectListItem>)(Session["GroupList"]);
            for (var i = 1; i < 6; i++)
            {
                for (var j = 1; j < 6; j++)
                {
                    if (i == 1)
                    {
                        if (j < startDayOfWk) continue;
                        view.BoxDay[i, j] = startDate.AddDays(j - startDayOfWk);
                        var mIdx = monthlyList.FindIndex(d => d.Date == view.BoxDay[i, j]);
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

                            var grpId = monthlyList[mIdx].GroupId;
                            if (grpId != null)
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
                        continue;
                    }
                    if (view.BoxDay[i - 1, j] == DateTime.MinValue)
                    {
                        view.BoxDay[i, j] = startDate.AddDays(7 + j - startDayOfWk);
                        var mIdx = monthlyList.FindIndex(d => d.Date == view.BoxDay[i, j]);
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
                    else
                    {
                        if (view.BoxDay[i - 1, j].AddDays(7) <= endDate)
                        {
                            view.BoxDay[i, j] = view.BoxDay[i - 1, j].AddDays(7);
                            var mIdx = monthlyList.FindIndex(d => d.Date == view.BoxDay[i, j]);
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
                }
            }
            return view;
        }

        private List<SelectListItem> GetDriverIdSelectList()
        {
            if (Session["DriverSelectList"] == null)
            {
                var driverList = new List<SelectListItem>();
                var driverDataList = new List<ApplicationUser>();
                var roleLookup = AppRoutines.UsersInRolesLookup();
                var driverRoleId = AppRoutines.GetRoleId("Driver");

                driverList.Add(new SelectListItem()
                {
                    Text = @"(nobody yet)", Value = "0"
                });

                driverDataList.Add(new ApplicationUser()
                {
                    Id = "0" // added so indexes of driverDataList match driverList 
                });

                using var db = new BHelpContext();
                var userList = db.Users.OrderBy(u => u.LastName).ToList();
                foreach (var user in userList)
                {
                    if (roleLookup.Any(r => r.UserId == user.Id && r.RoleId == driverRoleId))
                    {
                        driverList.Add(new SelectListItem()
                        {
                            Text = user.FirstName + @" " + user.LastName,
                            Value = user.Id,
                            Selected = false
                        });
                        driverDataList.Add(user);
                    }
                }

                Session["DriverSelectList"] = driverList;
                Session["DriverDataList"] = driverDataList;

                if (!User.IsInAnyRoles("Scheduler", "Developer", "Administrator")) // is NOT Scheduler
                {
                    var nonSchedulerDriverSelectList = new List<SelectListItem>
                    {
                        new SelectListItem()
                        {
                            Text = @"(nobody yet)", Value = "0"
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

                if (Session["GroupList"] == null)
                {
                    var groupList = db.GroupNames.OrderBy(n => n.Name).ToList();
                    var items = new List<SelectListItem> { new SelectListItem { Text = @"(none)", Value = "0" } };
                    foreach (var item in groupList)
                    {
                        items.Add(new SelectListItem { Text = item.Name, Value = item.Id.ToString() });
                    }

                    Session["DriverGroupList"] = items;
                }

                return driverList;
            }

            return null;
        }

        private Holiday GetHolidayData(DateTime dt)
        {
            var holidays = HolidayRoutines.GetHolidays(dt.Year);
            foreach (var holiday in holidays)
            {
                if (dt == holiday.CalculatedDate)
                {
                    return holiday;
                }
            }

            return null;
        }
    }
}