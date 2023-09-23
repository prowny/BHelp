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
    public class ODEmailController : Controller
    {
        // GET: ODEmail
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ODEmailDisplay(int year, int month)
        {
            if (Session["ODList"] == null) Session["ODList"] = GetODIdSelectList();
          
            var _dt = new DateTime(year, month, 1);
            var email = new ODEmailViewModel
            {
                Title = "Email ODs",
                MonthYear = _dt.ToString("MMMM") + " " + _dt.Year.ToString()
            };

            var text = "Dear ODs,";
            text += "<br />Please check current OD assignments to be sure it is correct. Note the many ";
            text += "TBDs. If you are able to fill any of them, please let me know.";
            text += "<br />IMPORTANT: You can also view the current schedule with OD contact info ";
            text += "included at bethesdahelpfd.com";

            email.EmailText = text;

            email.HtmlContent = GetODSchedule(year, month);

            return View(email);
        }

        private string GetODSchedule(int year, int month)
        {
            var view = GetODScheduleViewModel(year, month);

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

                        if (view.BoxODName[idx] != null)
                        {
                            content += "&nbsp;" + view.BoxODName[idx];
                        }
                        else
                        {
                            content += "&nbsp;TBD";
                        }

                        content += "</td>";
                    }
                }

                content += "</tr>";
            }

            content += "</table>";

            return (content);
        }

        private static List<ODSchedule> GetMonthlyList(int month, int year)
        {
            var start = new DateTime(year, month, 1);
            var end = new DateTime(year, month, DateTime.DaysInMonth(year, month));
            using var db = new BHelpContext();
            return db.ODSchedules
                .Where(d => d.Date >= start && d.Date <= end).OrderBy(d => d.Date).ToList();
        }

        private ODScheduleViewModel GetODScheduleViewModel(int year, int month)
        {
            var view = new ODScheduleViewModel
            {
                BoxDay = new DateTime[6, 6],
                BoxODId = new string[26],
                BoxODName = new string[26],
                BoxODPhone = new string[26],
                BoxODPhone2 = new string[26],
                BoxODEmail = new string[26],
                BoxHolidayDescription = new string[26],
                BoxNote = new string[26],
                Month = month,
                Year = year
            };

            var startDate = new DateTime(view.Year, view.Month, 1);
            var endDate = new DateTime(view.Year, view.Month, DateTime.DaysInMonth(view.Year, view.Month));
            var startDayOfWk = (int)startDate.DayOfWeek;
            var monthlyList = GetMonthlyList(view.Month, view.Year);
            var odList = (List<SelectListItem>)Session["ODList"];
            var odDataList = (List<ApplicationUser>)Session["ODDataList"];
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
                            var odIdx = odList.FindIndex(d => d.Value == monthlyList[mIdx].ODId);
                            if (odIdx >= 0)
                            {
                                view.BoxODName[idx] = odList[odIdx].Text;
                                view.BoxODPhone[idx] = odDataList[odIdx].PhoneNumber;
                                view.BoxODPhone2[idx] = odDataList[odIdx].PhoneNumber2;
                                view.BoxODEmail[idx] = odDataList[odIdx].Email;
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
                            var odIdx = odList.FindIndex(d => d.Value == monthlyList[mIdx].ODId);
                            if (odIdx >= 0)
                            {
                                view.BoxODName[idx] = odList[odIdx].Text;
                                view.BoxODPhone[idx] = odDataList[odIdx].PhoneNumber;
                                view.BoxODPhone2[idx] = odDataList[odIdx].PhoneNumber2;
                                view.BoxODEmail[idx] = odDataList[odIdx].Email;
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
                                var drIdx = odList.FindIndex(d => d.Value == monthlyList[mIdx].ODId);
                                if (drIdx >= 0)
                                {
                                    view.BoxODName[idx] = odList[drIdx].Text;
                                    view.BoxODPhone[idx] = odDataList[drIdx].PhoneNumber;
                                    view.BoxODPhone2[idx] = odDataList[drIdx].PhoneNumber2;
                                    view.BoxODEmail[idx] = odDataList[drIdx].Email;
                                }

                                view.BoxNote[idx] = monthlyList[mIdx].Note;
                            }
                        }
                    }
                }
            }
            return view;
        }

        private List<SelectListItem> GetODIdSelectList()
        {
            if (Session["ODSelectList"] == null)
            {
                var odList = new List<SelectListItem>();
                var odDataList = new List<ApplicationUser>();
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

                using var db = new BHelpContext();
                var userList = db.Users.OrderBy(u => u.LastName).ToList();
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
                    }
                }

                Session["ODSelectList"] = odList;
                Session["ODDataList"] = odDataList;

                if (!User.IsInAnyRoles("DriverScheduler", "Developer", "Administrator")) // is NOT Scheduler
                {
                    var nonSchedulerODSelectList = new List<SelectListItem>
                    {
                        new SelectListItem()
                        {
                            Text = @"(nobody yet)", Value = "0"
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

                if (Session["GroupList"] == null)
                {
                    var groupList = db.GroupNames.OrderBy(n => n.Name).ToList();
                    var items = new List<SelectListItem> { new SelectListItem { Text = @"(none)", Value = "0" } };
                    foreach (var item in groupList)
                    {
                        items.Add(new SelectListItem { Text = item.Name, Value = item.Id.ToString() });
                    }

                    Session["GroupList"] = items;
                }

                return odList;
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
