using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using BHelp.DataAccessLayer;
using BHelp.Models;
using BHelp.ViewModels;
using ClosedXML.Excel;

namespace BHelp.Controllers
{
    public class ReportsController : Controller
    {
        // GET: Reports

        [Authorize(Roles = "Administrator,Developer,Scheduler")]
        public ActionResult WeeklyInfoReport(DateTime? monday)
        {
            if (monday == null)
            { monday = GetMondaysDate(DateTime.Today); }

            if (Session["WeeklyInfoDate"] == null)
            {
                var mon = GetMondaysDate(DateTime.Today);
                Session["WeeklyInfoDate"] = mon.ToString("yyyy-MM-dd");
            }
            else
            { // Returning with Date change:
                var mon = (DateTime)monday;
                Session["WeeklyInfoDate"] = mon.ToString("yyyy-MM-dd");
            }

            if (Session["Holidays"] == null)
            { Session["Holidays"] = HolidayRoutines.GetHolidays(DateTime.Today.Year); }
            
            var view = GetWeeklyInfoReportData((DateTime)monday);
         
            return View(view);
        }

        private ReportsViewModel GetWeeklyInfoReportData(DateTime monday)
        {
            var view = new ReportsViewModel
            {
                BeginDate = monday,
                DateRangeTitle = monday.ToString("MM/dd/yyyy") + " - " + monday.AddDays(4).ToString("MM/dd/yyyy"),
                BoxDateDay = new string[15],
                BoxHoliday = new bool[15],
                BoxHolidayDescription = new string[15],
                BoxDriverId = new string[15],
                BoxDriverName = new string[15],
                BoxDriverPhone = new string[15],
                BoxDriverEmail = new string[15],
                BoxBackupDriverId = new string[15],
                BoxBackupDriverName = new string[15],
                BoxBackupDriverPhone = new string[15],
                BoxBackupDriverEmail = new string[15],
                BoxGroupDriverId = new string[15],
                BoxGroupName = new string[15],
                BoxGroupDriverName = new string[15],
                BoxGroupDriverPhone = new string[15],
                BoxGroupDriverEmail = new string[15],
                BoxODId = new string[15],
                BoxODName = new string[15],
                BoxODPhone = new string[15],
                BoxODEmail = new string[15],
                BoxODOddEvenMsg = new string[15]
            };

            var shortMonthList = AppRoutines.GetShortMonthArray();
            var shortWeekdayList = AppRoutines.GetShortWeekdayArray();
            var holidays = (List<Holiday>)Session["Holidays"];

            using (var db = new BHelpContext())
            {
                for (var row = 0; row < 5; row++) // Mon - Fri
                {
                    // First Column - Driver
                    var box = row * 3;
                    var boxDate = view.BeginDate.AddDays(row);
                    view.BoxDateDay[box] = shortWeekdayList[(int)boxDate.DayOfWeek] +
                                           " " + shortMonthList[boxDate.Month] + " " + boxDate.Day;
                    var isHoliday = HolidayRoutines.IsHoliday(boxDate, holidays);
                    view.BoxHoliday[box] = isHoliday;
                    if (isHoliday)
                    {
                        var holiday = GetHolidayData(boxDate);
                        view.BoxHolidayDescription[box] = holiday.Description;
                    }

                    var drSched = db.DriverSchedules
                        .SingleOrDefault(d => d.Date == boxDate);
                    if (drSched != null)
                    {
                        if (drSched.DriverId != null)
                        {
                            view.BoxDriverId[box] = drSched.DriverId;
                            var usr = db.Users.SingleOrDefault(d => d.Id == drSched.DriverId);
                            if (usr != null)
                            {
                                view.BoxDriverName[box] = usr.FullName;
                                view.BoxDriverPhone[box] = usr.PhoneNumber;
                                view.BoxDriverEmail[box] = usr.Email;
                            }
                        }

                        if (drSched.BackupDriverId != null)
                        {
                            view.BoxBackupDriverId[box] = drSched.BackupDriverId;
                            var usr = db.Users.SingleOrDefault(d => d.Id == drSched.BackupDriverId);
                            if (usr != null)
                            {
                                view.BoxBackupDriverName[box] = usr.FullName;
                                view.BoxBackupDriverPhone[box] = usr.PhoneNumber;
                                view.BoxBackupDriverEmail[box] = usr.Email;
                            }
                        }

                        if (drSched.GroupDriverId != null)
                        {
                            view.BoxGroupDriverId[box] = drSched.GroupDriverId;
                            var grp = db.GroupNames.SingleOrDefault(n => n.Id == drSched.GroupId);
                            if (grp != null) view.BoxGroupName[box] = grp.Name;
                            var usr = db.Users.SingleOrDefault(d => d.Id == drSched.GroupDriverId);
                            if (usr != null)
                            {
                                view.BoxGroupDriverName[box] = usr.FullName;
                                view.BoxGroupDriverPhone[box] = usr.PhoneNumber;
                                view.BoxGroupDriverEmail[box] = usr.Email;
                            }
                        }
                    }

                    // Second Column - OD
                    box++;
                    view.BoxDateDay[box] = view.BoxDateDay[box - 1]; // repeats first column date
                    view.BoxHoliday[box] = view.BoxHoliday[box - 1]; // repeats first column date
                    view.BoxHolidayDescription[box] = view.BoxHolidayDescription[box - 1];
                    var odSched = db.ODSchedules
                        .SingleOrDefault(d => d.Date == boxDate); // repeats first column date
                    if (odSched != null)
                    {
                        if (odSched.ODId != null)
                        {
                            view.BoxODId[box] = odSched.ODId;
                            var usr = db.Users.SingleOrDefault(d => d.Id == odSched.ODId);
                            if (usr != null)
                            {
                                view.BoxODName[box] = usr.FullName;
                                view.BoxODPhone[box] = usr.PhoneNumber;
                                view.BoxODEmail[box] = usr.Email;
                                if (boxDate.Day % 2 == 0)
                                {
                                    view.BoxODOddEvenMsg[box] = "Take Food Requests Only From EVEN Numbers";
                                }
                                else
                                {
                                    view.BoxODOddEvenMsg[box] = "Take Food Requests Only From ODD Numbers";
                                }
                            }
                        }
                    }

                    // Third Column - next day Driver
                    box++;
                    if ((int)boxDate.DayOfWeek == 5)
                    {
                        boxDate = boxDate.AddDays(3);
                    }
                    else
                    {
                        boxDate = boxDate.AddDays(1);
                    }

                    view.BoxDateDay[box] = shortWeekdayList[(int)boxDate.DayOfWeek] + " " +
                                           shortMonthList[boxDate.Month] + " " + boxDate.Day;
                    isHoliday = HolidayRoutines.IsHoliday(boxDate, holidays);
                    view.BoxHoliday[box] = isHoliday;
                    if (isHoliday)
                    {
                        var holiday = GetHolidayData(boxDate);
                        view.BoxHolidayDescription[box] = holiday.Description;
                    }

                    drSched = db.DriverSchedules
                        .SingleOrDefault(d => d.Date == boxDate);
                    if (drSched != null)
                    {
                        if (drSched.DriverId != null)
                        {
                            view.BoxDriverId[box] = drSched.DriverId;
                            var usr = db.Users.SingleOrDefault(d => d.Id == drSched.DriverId);
                            if (usr != null)
                            {
                                view.BoxDriverName[box] = usr.FullName;
                                view.BoxDriverPhone[box] = usr.PhoneNumber;
                                view.BoxDriverEmail[box] = usr.Email;
                            }
                        }

                        if (drSched.BackupDriverId != null)
                        {
                            view.BoxBackupDriverId[box] = drSched.BackupDriverId;
                            var usr = db.Users.SingleOrDefault(d => d.Id == drSched.BackupDriverId);
                            if (usr != null)
                            {
                                view.BoxBackupDriverName[box] = usr.FullName;
                                view.BoxBackupDriverPhone[box] = usr.PhoneNumber;
                                view.BoxBackupDriverEmail[box] = usr.Email;
                            }
                        }

                        if (drSched.GroupDriverId != null)
                        {
                            view.BoxGroupDriverId[box] = drSched.GroupDriverId;
                            var grp = db.GroupNames.SingleOrDefault(n => n.Id == drSched.GroupId);
                            if (grp != null) view.BoxGroupName[box] = grp.Name;
                            var usr = db.Users.SingleOrDefault(d => d.Id == drSched.GroupDriverId);
                            if (usr != null)
                            {
                                view.BoxGroupDriverName[box] = usr.FullName;
                                view.BoxGroupDriverPhone[box] = usr.PhoneNumber;
                                view.BoxGroupDriverEmail[box] = usr.Email;
                            }
                        }
                    }
                }
            }

            return view;
        }

        public ActionResult WeeklyInfoReportToExcel()
        {
            var strDt = Session["WeeklyInfoDate"].ToString();
            var _yr = Convert.ToInt32(strDt.Substring(0, 4));
            var _mo = Convert.ToInt32(strDt.Substring(5, 2));
            var _dy = Convert.ToInt32(strDt.Substring(8, 2));
            var monday = new DateTime(_yr, _mo, _dy);
            var view = GetWeeklyInfoReportData(monday);
            
            var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Weekly Schedule");
            var row = 1;
            ws.Columns("1").Width = 75;
            ws.Columns("2").Width = 75;
            ws.Columns("3").Width = 75;

            ws.Cell(row, 1).SetValue("Weekly Info Report - " + view.DateRangeTitle);

            row++;
            var boxContents = "DRIVERS";
            boxContents += Environment.NewLine + "Use the relational database to enter your delivery";
            boxContents += Environment.NewLine + "report and your time asap";
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row,1).SetValue(boxContents);

            boxContents = "ODs";
            boxContents += Environment.NewLine + "Please inform the Next Day Drivers, Next Day";
            boxContents += Environment.NewLine + "OD and the Scheduler when you are done and";
            boxContents += Environment.NewLine + "tell Drivers they can get their delivery list in the";
            boxContents += Environment.NewLine + "database";
            ws.Cell(row, 2).Style.Font.Bold = true;
            ws.Cell(row, 2).SetValue(boxContents);

            boxContents = Environment.NewLine + Environment.NewLine;
            boxContents += Environment.NewLine + Environment.NewLine;
            boxContents += "NEXT DAY DRIVER(s)";
            ws.Cell(row, 3).Style.Font.Bold = true;
            ws.Cell(row, 3).SetValue(boxContents);
            for (var i = 0; i < 5; i++)
            {
                row++;
                var boxNo = i * 3;
                // *********** First Column ************
                boxContents = view.BoxDateDay[boxNo] + Environment.NewLine;
                if (view.BoxHoliday[boxNo])
                {
                    boxContents += view.BoxHolidayDescription[boxNo];
                    boxContents += Environment.NewLine + "BHELP Closed";
                }
                else
                {
                    if (view.BoxDriverId[boxNo] != null)
                    {
                        boxContents += "Driver: " + view.BoxDriverName[boxNo] + Environment.NewLine;
                        boxContents += view.BoxDriverPhone[boxNo] + " " + view.BoxDriverEmail[boxNo];
                    }

                    if (view.BoxBackupDriverId[boxNo] != null)
                    {
                        boxContents += Environment.NewLine;
                        boxContents += "Backup Driver: " + view.BoxBackupDriverName[boxNo] + Environment.NewLine;
                        boxContents += view.BoxBackupDriverPhone[boxNo] + " " + view.BoxBackupDriverEmail[boxNo];
                    }

                    if (view.BoxGroupDriverId[boxNo] != null)
                    {
                        boxContents += Environment.NewLine;
                        boxContents += view.BoxGroupName[boxNo] + " " + "Group Driver: " +
                                       view.BoxGroupDriverName[boxNo];
                        boxContents += Environment.NewLine;
                        boxContents += view.BoxGroupDriverPhone[boxNo] + " " + view.BoxGroupDriverEmail[boxNo];
                    }
                }
                ws.Cell(row, 1).SetValue(boxContents);

                // *********** Second Column ************
                boxNo += 1;
                boxContents = view.BoxDateDay[boxNo] + Environment.NewLine;
                if (view.BoxHoliday[boxNo])
                {
                    boxContents += view.BoxHolidayDescription[boxNo];
                    boxContents += Environment.NewLine + "BHELP Closed";
                }
                else
                {
                    if (view.BoxODId[boxNo] != null)
                    {
                        boxContents += "OD: " + view.BoxODName[boxNo] + Environment.NewLine;
                        boxContents += view.BoxODPhone[boxNo] + " " + view.BoxODEmail[boxNo];
                        boxContents += Environment.NewLine + view.BoxODOddEvenMsg[boxNo];
                    }
                }
                ws.Cell(row, 2).SetValue(boxContents);

                // *********** Third Column ************
                boxNo += 1;
                boxContents = view.BoxDateDay[boxNo] + Environment.NewLine;
                if (view.BoxHoliday[boxNo])
                {
                    boxContents += view.BoxHolidayDescription[boxNo];
                    boxContents += Environment.NewLine + "BHELP Closed";
                }
                else
                {
                    if (view.BoxDriverId[boxNo] != null)
                    {
                        boxContents += "Driver: " + view.BoxDriverName[boxNo] + Environment.NewLine;
                        boxContents += view.BoxDriverPhone[boxNo] + " " + view.BoxDriverEmail[boxNo];
                    }

                    if (view.BoxBackupDriverId[boxNo] != null)
                    {
                        boxContents += Environment.NewLine;
                        boxContents += "Backup Driver: " + view.BoxBackupDriverName[boxNo] + Environment.NewLine;
                        boxContents += view.BoxBackupDriverPhone[boxNo] + " " + view.BoxBackupDriverEmail[boxNo];
                    }

                    if (view.BoxGroupDriverId[boxNo] != null)
                    {
                        boxContents += Environment.NewLine;
                        boxContents += view.BoxGroupName[boxNo] + " " + "Group Driver: " +
                                       view.BoxGroupDriverName[boxNo];
                        boxContents += Environment.NewLine;
                        boxContents += view.BoxGroupDriverPhone[boxNo] + " " + view.BoxGroupDriverEmail[boxNo];
                    }
                }
                ws.Cell(row, 3).SetValue(boxContents);
            }

            var ms = new MemoryStream();
            workbook.SaveAs(ms);
            ms.Position = 0;
            return new FileStreamResult(ms, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            { FileDownloadName = "WeeklyInfoReport" + monday.ToString("MM-dd-yy") + ".xlsx" };
        }

        public ActionResult DriverMasterList()
        {
            var view = new UserViewModel {UserList = GetUserMasterList("Driver")};
            return View(view);
        }

        private List<ApplicationUser> GetUserMasterList(string roleName)
        {
            var listActiveUsers = AppRoutines.GetActiveUserList();
            var roleId = AppRoutines.GetRoleId(roleName);
            var listUserIdsInRole = AppRoutines.GetUserIdsInRole(roleId);
            var listMaster = new List<ApplicationUser>();
            foreach (var activeUser in listActiveUsers)
            {
                foreach (var userIdInRole in listUserIdsInRole)
                {
                    if (userIdInRole == activeUser.Id)
                    {
                        var addUsr = new ApplicationUser
                        {
                            Id = activeUser.Id,
                            FirstName = activeUser.FirstName,
                            LastName = activeUser.LastName,
                            Email = activeUser.Email,
                            PhoneNumber = activeUser.PhoneNumber,
                            PhoneNumber2 = activeUser.PhoneNumber2
                        };
                        listMaster.Add(addUsr);
                    }
                }
            }
            return listMaster;
        }

        public ActionResult DriverMasterListToExcel()
        {
            var listMaster = GetUserMasterList("Driver");
            var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Driver Master List");
            var activeRow = 1;
            ws.Columns("1").Width = 10;
            ws.Cell(activeRow, 1).SetValue("As of " + DateTime.Today.ToShortDateString()).Style.Font.SetBold(true);
            activeRow++;
            ws.Cell(activeRow, 2).SetValue(DateTime.Today.ToShortDateString()).Style.Font.SetBold(true);
            activeRow++;
            ws.Cell(activeRow,1).SetValue("Active Drivers").Style.Font.SetBold(true);
            activeRow++;
            ws.Columns("1").Width = 15;
            ws.Cell(activeRow, 1).SetValue("First Name").Style.Font.SetBold(true);
            ws.Columns("2").Width = 15;
            ws.Cell(2, 2).SetValue("Last Name").Style.Font.SetBold(true);
            ws.Columns("3").Width = 40;
            ws.Cell(activeRow, 3).SetValue("Email").Style.Font.SetBold(true);
            ws.Columns("4").Width = 12;
            ws.Cell(activeRow, 4).SetValue("Phone").Style.Font.SetBold(true);
            ws.Columns("5").Width = 12;
            ws.Cell(activeRow, 5).SetValue("Phone 2").Style.Font.SetBold(true);

            foreach (var usr in listMaster)
            {
                activeRow++;
                ws.Cell(activeRow, 1).SetValue(usr.FirstName);
                ws.Cell(activeRow, 2).SetValue(usr.LastName);
                ws.Cell(activeRow, 3).SetValue(usr.Email);
                ws.Cell(activeRow, 4).SetValue(usr.PhoneNumber);
                ws.Cell(activeRow, 5).SetValue(usr.PhoneNumber2);
            }
            var ms = new MemoryStream();
            workbook.SaveAs(ms);
            ms.Position = 0;
            return new FileStreamResult(ms, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                { FileDownloadName = "DriverMasterList" + DateTime.Today.ToString("MM-dd-yy") + ".xlsx" };
        }

        public ActionResult ActiveVolunteerDetailsToCSV()
        {
            using (var context = new BHelpContext())
            {
                var activeVolunteersList = AppRoutines.GetActiveUserList();

                var sb = new StringBuilder();
                sb.Append("Active Volunteers" + ',');
                sb.Append(DateTime.Today.ToShortDateString());
                sb.AppendLine();

                sb.Append("Last Name" + ',');
                sb.Append("First Name" + ',');
                sb.Append("Title" + ',');
                sb.Append("Address" + ',');
                sb.Append("City" + ',');
                sb.Append("State" + ',');
                sb.Append("Zip Code" + ',');
                sb.Append("Email" + ',');
                sb.Append("Phone 1" + ',');
                sb.Append("Phone 2" + ',');
                sb.Append("Roles" + ',');
                sb.Append("Notes");
                sb.AppendLine();

                foreach (var vol in activeVolunteersList)
                {
                    sb.Append(vol.LastName + ',');
                    sb.Append(vol.FirstName + ',');
                    sb.Append("\"" + vol.Title + "\"" + ',');
                    sb.Append("\"" + vol.Address + "\"" + ',');
                    sb.Append(vol.City + ',');
                    sb.Append(vol.State + ',');
                    sb.Append(vol.Zip + ',');
                    sb.Append(vol.Email + ',');
                    sb.Append(vol.PhoneNumber + ',');
                    sb.Append(vol.PhoneNumber2 + ',');
                    sb.Append(context.GetStringAllRolesForUser(vol.Id) + ',');
                    sb.Append(vol.Notes);
                    sb.AppendLine();
                }

                var response = System.Web.HttpContext.Current.Response;
                response.BufferOutput = true;
                response.Clear();
                response.ClearHeaders();
                response.ContentEncoding = Encoding.Unicode;
                response.AddHeader("content-disposition", "attachment;filename="
                                                          + "Active Volunteers" + DateTime.Today.ToString("MM-dd-yy")
                                                          + ".csv");
                response.ContentType = "text/plain";
                response.Write(sb);
                response.End();
            }

            return null;
        }

        public ActionResult ActiveVolunteerDetailsToExcel()
        {
            using (var context = new BHelpContext())
            {
                var activeVolunteersList = AppRoutines.GetActiveUserList();

                var workbook = new XLWorkbook();
                var ws = workbook.Worksheets.Add("Active Volunteers");
                ws.Columns("1").Width = 10;
                ws.Cell(1, 1).SetValue("Active Volunteers").Style.Font.SetBold(true);
                ws.Cell(1, 1).Style.Alignment.WrapText = true;
                ws.Cell(1, 2).SetValue(DateTime.Today.ToShortDateString()).Style.Font.SetBold(true);
                ws.Cell(2, 1).SetValue("Last Name").Style.Font.SetBold(true);
                ws.Columns("2").Width = 15;
                ws.Cell(2, 2).SetValue("First Name").Style.Font.SetBold(true);
                ws.Columns("3").Width = 15;
                ws.Cell(2, 3).SetValue("Title").Style.Font.SetBold(true);
                ws.Columns("4").Width = 40;
                ws.Cell(2, 4).SetValue("Address").Style.Font.SetBold(true);
                ws.Cell(2, 5).SetValue("City").Style.Font.SetBold(true);
                ws.Cell(2, 6).SetValue("State").Style.Font.SetBold(true);
                ws.Cell(2, 7).SetValue("Zip Code").Style.Font.SetBold(true);
                ws.Columns("8").Width = 40;
                ws.Cell(2, 8).SetValue("Email").Style.Font.SetBold(true);
                ws.Columns("9").Width = 12;
                ws.Cell(2, 9).SetValue("Phone 1").Style.Font.SetBold(true);
                ws.Columns("10").Width = 12;
                ws.Cell(2, 10).SetValue("Phone 2").Style.Font.SetBold(true);
                ws.Cell(2, 11).SetValue("Roles").Style.Font.SetBold(true);
                ws.Cell(2, 11).SetValue("Notes").Style.Font.SetBold(true);

                var activeRow = 2;
                foreach (var vol in activeVolunteersList)
                {
                    activeRow++;
                    ws.Cell(activeRow, 1).SetValue(vol.LastName);
                    ws.Cell(activeRow, 2).SetValue(vol.FirstName);
                    ws.Cell(activeRow, 3).SetValue(vol.Title);
                    ws.Cell(activeRow, 4).SetValue(vol.Address);
                    ws.Cell(activeRow, 5).SetValue(vol.City);
                    ws.Cell(activeRow, 6).SetValue(vol.State);
                    ws.Cell(activeRow, 7).SetValue(vol.Zip);
                    ws.Cell(activeRow, 8).SetValue( vol.Email);  
                    ws.Cell(activeRow, 9).SetValue(vol.PhoneNumber);
                    ws.Cell(activeRow, 10).SetValue(vol.PhoneNumber2);
                    var volRoles = context.GetStringAllRolesForUser(vol.Id);
                    ws.Cell(activeRow, 11).SetValue(volRoles);
                    ws.Cell(activeRow, 12).SetValue(vol.Notes);
                }

                var ms = new MemoryStream();
                workbook.SaveAs(ms);
                ms.Position = 0;
                return new FileStreamResult(ms, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                    { FileDownloadName = "Active Volunteers" + DateTime.Today.ToString("MM-dd-yy") + ".xlsx" };
            }
        }
        private Holiday GetHolidayData(DateTime dt)
        {
            var holidays = (List<Holiday>)Session["Holidays"];
            if (holidays.Count == 0)
            {
                holidays = HolidayRoutines.GetHolidays(dt.Year);
                Session["Holidays"] = holidays;
            }
            // check if ANY calculated date in proper year:
            foreach (var hol in holidays)
            {
                if (hol.CalculatedDate.Year == dt.Year) { break; }
                // else load requested year's holidays:
                holidays = HolidayRoutines.GetHolidays(dt.Year);
                Session["Holidays"] = holidays;
            }
            return holidays.Find(h => h.CalculatedDate == dt);
        }
        public ActionResult WeekPrevious(DateTime monday)
            {
                monday = monday.AddDays(-7);
                return RedirectToAction("WeeklyInfoReport", new { monday });
            }
        public ActionResult WeekNext(DateTime monday)
            {
                monday = monday.AddDays(7);
                return RedirectToAction("WeeklyInfoReport", new { monday });
            }
        private static DateTime GetMondaysDate(DateTime date)
            {
                var returnDate = date;
                // if Sat or Sun (6 or 0), get next Monday
                var dow = (int)date.DayOfWeek;
                switch (dow)
                {
                    case 0:
                        returnDate = date.AddDays(1);
                        break;
                    case 2:
                        returnDate = date.AddDays(-1);
                        break;
                    case 3:
                        returnDate = date.AddDays(-2);
                        break;
                    case 4:
                        returnDate = date.AddDays(-3);
                        break;
                    case 5:
                        returnDate = date.AddDays(-4);
                        break;
                    case 6:
                        returnDate = date.AddDays(2);
                        break;
                }

                return returnDate;
            }
    }
}