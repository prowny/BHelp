using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using BHelp.DataAccessLayer;
using BHelp.Models;
using BHelp.ViewModels;
using Castle.Core.Internal;
using ClosedXML.Excel;

namespace BHelp.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        // GET: Reports

        [Authorize(Roles = "Reports,Administrator,Developer")]
   
        public ActionResult ReportsMenu()
        {
            return View();
        }
        public ActionResult WeeklyInfoReport(DateTime? monday)
        {
            if (monday == null) { monday = GetMondaysDate(DateTime.Today); }
            
            var view = GetWeeklyInfoReportData((DateTime)monday);
         
            return View(view);
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
        public ActionResult ODMasterList()
        {
            var view = new UserViewModel { UserList = GetUserMasterList("OfficerOfTheDay") };
            return View(view);
        }
        public ActionResult MasterListToExcel(string role)
        {
            var listMaster = GetUserMasterList(role);
            if (role == "OfficerOfTheDay") role = "OD";
            var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add( role + " Master List");
            var activeRow = 1;
            ws.Columns("1").Width = 10;
            ws.Cell(activeRow, 1).SetValue("As of " + DateTime.Today.ToShortDateString()).Style.Font.SetBold(true);
            activeRow++;
            ws.Cell(activeRow, 2).SetValue(DateTime.Today.ToShortDateString()).Style.Font.SetBold(true);
            activeRow++;
            ws.Cell(activeRow,1).SetValue("Active " + role + "s").Style.Font.SetBold(true);
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
                { FileDownloadName = role + "MasterList" + DateTime.Today.ToString("MM-dd-yy") + ".xlsx" };
        }
        public ActionResult MasterListToCSV(string role)
        {
            var listMaster = GetUserMasterList(role);

            var sb = new StringBuilder();
            sb.Append("As of " + DateTime.Today.ToShortDateString());
            sb.AppendLine();
            if (role == "OfficerOfTheDay") { role = "OD"; }
            sb.Append(role + "s");
            sb.AppendLine();

            sb.Append("First Name" + ',');
            sb.Append("Last Name" + ',');
            sb.Append("Email" + ',');
            sb.Append("Phone" + ',');
            sb.Append("Phone 2");
            sb.AppendLine();

            foreach (var usr in listMaster)
            {
                sb.Append(usr.FirstName + ',');
                sb.Append(usr.LastName + ',');
                sb.Append(usr.Email + ',');
                sb.Append(usr.PhoneNumber + ',');
                sb.Append(usr.PhoneNumber2);
                sb.AppendLine();
            }

            var response = System.Web.HttpContext.Current.Response;
            response.BufferOutput = true;
            response.Clear();
            response.ClearHeaders();
            response.ContentEncoding = Encoding.Unicode;
            response.AddHeader("content-disposition", "attachment;filename="
                + role + "MasterList" + DateTime.Today.ToString("MM-dd-yy") + ".csv");
            response.ContentType = "text/plain";
            response.Write(sb);
            response.End();
            return null;
        }
        public ActionResult ActiveVolunteerDetails()
        {;
            var view = new UserViewModel { UserList = AppRoutines.GetActiveUserList() };
            foreach (var usr in view.UserList)
            {
                usr.AllRolesForUser = AppRoutines.GetStringAllRolesForUser(usr.Id);
            }
            return View(view);
        }
        public ActionResult ActiveVolunteerDetailsToCSV()
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
                    sb.Append(AppRoutines.GetStringAllRolesForUser(vol.Id) + ',');
                    sb.Append(vol.Notes);
                    sb.AppendLine();
                }

                var response = System.Web.HttpContext.Current.Response;
                response.BufferOutput = true;
                response.Clear();
                response.ClearHeaders();
                response.ContentEncoding = Encoding.Unicode;
                response.AddHeader("content-disposition", "attachment;filename="
                    + "Active Volunteers" + DateTime.Today.ToString("MM-dd-yy") + ".csv");
                response.ContentType = "text/plain";
                response.Write(sb);
                response.End();

            return null;
        }
        public ActionResult ActiveVolunteerDetailsToExcel()
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
                ws.Cell(activeRow, 8).SetValue(vol.Email);
                ws.Cell(activeRow, 9).SetValue(vol.PhoneNumber);
                ws.Cell(activeRow, 10).SetValue(vol.PhoneNumber2);
                var volRoles = AppRoutines.GetStringAllRolesForUser(vol.Id);
                ws.Cell(activeRow, 11).SetValue(volRoles);
                ws.Cell(activeRow, 12).SetValue(vol.Notes);
            }

            var ms = new MemoryStream();
            workbook.SaveAs(ms);
            ms.Position = 0;
            return new FileStreamResult(ms, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                { FileDownloadName = "Active Volunteers" + DateTime.Today.ToString("MM-dd-yy") + ".xlsx" };
         }

        public ActionResult GiftCardsReport(DateTime? startDate, DateTime? endDate)
        {
            using var db = new BHelpContext();
            var view = new DeliveryViewModel();

            if (startDate == null)
            {
                //default to last month
                startDate = HoursRoutines.GetPreviousMonthStartDate(DateTime.Today);
            }

            if (endDate == null)
            {
                //default to last month
                endDate = HoursRoutines.GetPreviousMonthEndDate(DateTime.Today);
            }

            view.HistoryStartDate = startDate;
            view.HistoryEndDate = endDate;
            List<Delivery> deliveries = db.Deliveries
                .Where(d => d.Status == 1
                            && d.DateDelivered >= view.HistoryStartDate
                            & d.DateDelivered <= view.HistoryEndDate)
                .OrderBy(d => d.DateDelivered)
                .ThenBy(d => d.DriverId).ToList();

            // Seed the report loops:
            var driverDeliveryCount = 0;
            var driverResidentsCount = 0;
            var driverFullBagCount = 0;
            var driverHalfBagCount = 0;
            var driverKidSnackCount = 0;
            var driverPounds = 0;
            var driverGiftCardCount = 0;
            var driverHolidayGiftCardCount = 0;

            var dateDeliveryCount = 0;
            var dateResidentsCount = 0;
            var dateFullBagCount = 0;
            var dateHalfBagCount = 0;
            var dateKidSnackCount = 0;
            var datePounds = 0;
            var dateGiftCardCount = 0;
            var dateHolidayGiftCardCount = 0;

            var totalDeliveryCount = 0;
            var totalResidentsCount = 0;
            var totalFullBagCount = 0;
            var totalHalfBagCount = 0;
            var totalKidSnackCount = 0;
            var totalPounds = 0;
            var totalGiftCardCount = 0;
            var totalHolidayGiftCardCount = 0;
            var reportDeliveryList = new List<DeliveryViewModel>(); // get giftcard totals by day/driver

            //===================================

            var start = (DateTime)startDate;
            var end = (DateTime)endDate;
            // Get group of deliveries by date:
            for (var date = start; date <= end; date = date.AddDays(1))
            {
                var singleDateList = deliveries.Where(d => d.DateDelivered == date).ToList();
                if (singleDateList.Count == 0) continue;
                {
                    // Get # of Residents for each delivery
                    foreach (var del in singleDateList)
                    {
                        del.HouseoldCount = del.Children + del.Adults + del.Seniors;
                    }
                    var currentDateDelivered = singleDateList[0].DateDelivered;
                    
                    var distinctDriverList = singleDateList
                        .Select(d => d.DriverId).Distinct();
                    foreach (var dtDrvId in distinctDriverList)
                    {
                        var driverList = singleDateList
                            .Where(d => d.DriverId == dtDrvId);
                        foreach (var del in driverList)
                        {
                            driverDeliveryCount += 1;
                            driverResidentsCount += del.HouseoldCount;
                            driverFullBagCount += del.FullBags;
                            driverHalfBagCount += del.HalfBags;
                            driverKidSnackCount += del.KidSnacks;
                            driverPounds += Convert.ToInt32(del.FullBags * 10 + del.HalfBags * 9);
                            driverGiftCardCount += del.GiftCards;
                            driverHolidayGiftCardCount += del.HolidayGiftCards;

                            dateDeliveryCount += 1;
                            dateResidentsCount += del.HouseoldCount;
                            dateFullBagCount += del.FullBags;
                            dateHalfBagCount += del.HalfBags;
                            dateKidSnackCount += del.KidSnacks; 
                            datePounds += Convert.ToInt32(del.FullBags * 10 + del.HalfBags * 9);
                            dateGiftCardCount += del.GiftCards;
                            dateHolidayGiftCardCount += del.HolidayGiftCards;
                            
                            totalDeliveryCount += 1;
                            totalResidentsCount += del.HouseoldCount;
                            totalFullBagCount += del.FullBags;
                            totalHalfBagCount += del.HalfBags;
                            totalKidSnackCount += del.KidSnacks;
                            totalPounds += Convert.ToInt32(del.FullBags * 10 + del.HalfBags * 9);
                            totalGiftCardCount += del.GiftCards;
                            totalHolidayGiftCardCount += del.HolidayGiftCards;
                        }

                        // Summarize driver:
                        var newDrv = new DeliveryViewModel
                        {
                            DateDelivered = currentDateDelivered,
                            DriverName = AppRoutines.GetDriverName(dtDrvId),
                            DeliveryCount = driverDeliveryCount,
                            HouseholdCount = driverResidentsCount,
                            FullBagCount =driverFullBagCount,
                            HalfBagCount = driverHalfBagCount,
                            KidSnackCount  =driverKidSnackCount,
                            PoundsOfFood = driverPounds, 
                            GiftCardCount = driverGiftCardCount,
                            HolidayGiftCardCount = driverHolidayGiftCardCount
                        };
                        reportDeliveryList.Add(newDrv);
                        driverDeliveryCount = 0;
                        driverResidentsCount = 0;
                        driverFullBagCount = 0;
                        driverHalfBagCount = 0;
                        driverKidSnackCount = 0;
                        driverPounds = 0;
                        driverGiftCardCount = 0;
                        driverHolidayGiftCardCount = 0;
                    }

                    // Summarize the date:
                    var newDt = new DeliveryViewModel
                    {
                        DateDelivered = null,
                        DriverName = "Totals for " + currentDateDelivered?.ToString("MM/dd/yyyy"),
                        DeliveryCount = dateDeliveryCount,
                        HouseholdCount = dateResidentsCount,
                        FullBagCount = dateFullBagCount, 
                        HalfBagCount = dateHalfBagCount,
                        KidSnackCount =dateKidSnackCount, 
                        PoundsOfFood = datePounds, 
                        GiftCardCount = dateGiftCardCount,
                        HolidayGiftCardCount = dateHolidayGiftCardCount
                    };
                    reportDeliveryList.Add(newDt);
                    newDt = new DeliveryViewModel() // blank line
                        { DateDelivered = null, DriverName = "", DeliveryCount = null, GiftCardCount = null };
                    reportDeliveryList.Add(newDt); // add blank line
                    dateDeliveryCount = 0;
                    dateResidentsCount = 0;
                    dateFullBagCount = 0;
                    dateHalfBagCount = 0;
                    dateKidSnackCount =0;
                    datePounds = 0;
                    dateGiftCardCount = 0;
                    dateHolidayGiftCardCount = 0;
                }
            }

            var totalDel = new DeliveryViewModel()
            {
                DateDelivered = null,
                DriverName = "Grand Totals",
                DeliveryCount = totalDeliveryCount,
                HouseholdCount = totalResidentsCount,
                FullBagCount = totalFullBagCount,
                HalfBagCount = totalHalfBagCount,
                KidSnackCount = totalKidSnackCount, 
                PoundsOfFood = totalPounds, 
                GiftCardCount = totalGiftCardCount,
                HolidayGiftCardCount = totalHolidayGiftCardCount
            };
            reportDeliveryList.Add(totalDel);

            view.GiftCardReportDeliveries = reportDeliveryList;
            TempData["StartDate"] = startDate; TempData["EndDate"] = endDate;
            TempData["GiftCardsReport"] = reportDeliveryList;
            return View(view);
        }

        public ActionResult GiftCardsReportToCSV()
        {
            var reportDeliveryList = (List<DeliveryViewModel>)(TempData["GiftCardsReport"]);
            if (reportDeliveryList == null) return RedirectToAction("GiftCardsReport");
            var sb = new StringBuilder();
            var startDate = (DateTime)(TempData["StartDate"]);
            var endDate = (DateTime)(TempData["EndDate"]);
            sb.Append("Start Date:" + "," + Convert .ToDateTime( startDate).ToShortDateString());
            sb.AppendLine();
            sb.Append("End Date:" + "," + Convert .ToDateTime( endDate).ToShortDateString());
            sb.AppendLine();

            // Headers
            sb.Append("Date,Driver,Deliveries,Residents,A Bags, B Bags, Kid Snacks," +
                      "Pounds of Food, Gift Cards, HolidayGiftCards");
            sb.AppendLine();

            foreach (var del in reportDeliveryList)
            {
                if (del.DateDelivered == null)
                {
                    sb.Append(",");
                }
                else
                {
                    sb.Append(Convert.ToDateTime(del.DateDelivered).ToString("MM/dd/yyyy") + ",");
                }

                sb.Append(del.DriverName + ",");
                sb.Append(del.DeliveryCount + ",");
                sb.Append(del.HouseholdCount + ",");
                sb.Append(del.FullBagCount + ",");
                sb.Append(del.HalfBagCount + ",");
                sb.Append(del.KidSnackCount + ",");
                sb.Append(del.PoundsOfFood + ","); 
                sb.Append(del.GiftCardCount  + ",");
                sb.Append(del.HolidayGiftCardCount + ",");
                sb.AppendLine();
            }

            var response = System.Web.HttpContext.Current.Response;
            response.BufferOutput = true;
            response.Clear();
            response.ClearHeaders();
            response.ContentEncoding = Encoding.Unicode;
            response.AddHeader("content-disposition", "attachment;filename="
                                                      + "GiftCards" + DateTime.Today.ToString("MM-dd-yy") + ".csv");
            response.ContentType = "text/plain";
            response.Write(sb);
            response.End();
            return null;
        }

        [Authorize(Roles = "Administrator,Staff,Developer,Reports")]
            public ActionResult QORKReport(string endingDate = "") // New QORK Report 02/22
            {
                DateTime endDate;
                if (endingDate.IsNullOrEmpty())
                {
                    // Ends on a Sunday - weekday Monday is 1, Saturday is 6, Sunday is 0
                    // If today is a  Sunday, default to this week
                    var weekDay = Convert.ToInt32(DateTime.Today.DayOfWeek);
                    if (weekDay == 0) // Default to this this Sunday, else Sunday last week
                    { endDate = DateTime.Today; }
                    else
                    {
                        var lastSunday = DateTime.Now.AddDays(-1);
                        while (lastSunday.DayOfWeek != DayOfWeek.Sunday) lastSunday = lastSunday.AddDays(-1);
                        endDate = lastSunday;
                    }
                }
                else
                {
                    endDate = Convert.ToDateTime(endingDate);
                }
                var view = GetQORKReportView(endDate);
                return View(view);
            }
            public ActionResult QORKReportToCSV(string endingDate = "")
            {
                DateTime endDate;
                if (endingDate.IsNullOrEmpty())
                {
                    // Ends on a Sunday - weekday Monday is 1, Saturday is 6, Sunday is 0
                    // If today is a  Sunday, default to this week
                    var weekDay = Convert.ToInt32(DateTime.Today.DayOfWeek);
                    if (weekDay == 0) // Default to this this Sunday, else Sunday last week
                    { endDate = DateTime.Today; }
                    else
                    {
                        var lastSunday = DateTime.Now.AddDays(-1);
                        while (lastSunday.DayOfWeek != DayOfWeek.Sunday) lastSunday = lastSunday.AddDays(-1);
                        endDate = lastSunday;
                    }
                }
                else
                {
                    endDate = Convert.ToDateTime(endingDate);
                }
                var view = GetQORKReportView(endDate);
                view.ReportTitle = "\"" + "Bethesda Help, Inc. QORK Report for week ending " + "\"" + endDate.ToString("MM/dd/yyyy");

                var result = AppRoutines.QORKReportToCSV(view);
                return result;
            }
            public ActionResult QORKReportToExcel(string endingDate)
            {
                DateTime endDate;
                if (endingDate.IsNullOrEmpty())
                {
                    // Ends on a Sunday - weekday Monday is 1, Saturday is 6, Sunday is 0
                    // If today is a  Sunday, default to this week
                    var weekDay = Convert.ToInt32(DateTime.Today.DayOfWeek);
                    if (weekDay == 0) // Default to this this Sunday, else Sunday last week
                    { endDate = DateTime.Today; }
                    else
                    {
                        var lastSunday = DateTime.Now.AddDays(-1);
                        while (lastSunday.DayOfWeek != DayOfWeek.Sunday) lastSunday = lastSunday.AddDays(-1);
                        endDate = lastSunday;
                    }
                }
                else
                {
                    endDate = Convert.ToDateTime(endingDate);
                }

                var view = GetQORKReportView(endDate);
                view.ReportTitle = "Bethesda Help, Inc. QORK Report for week ending " + endDate.ToString("MM/dd/yyyy");

                var workbook = new XLWorkbook();
                var ws = workbook.Worksheets.Add("QORK Report " + view.EndDateString);

                var activeRow = 1;
                ws.Cell(activeRow, 1).SetValue(view.ReportTitle);
                activeRow++;
                for (int i = 0; i < view.QORKTitles.Length; i++)
                {
                    ws.Cell(activeRow, i + 1).SetValue(view.QORKTitles[i]);
                }

                for (var i = 0; i < view.ZipCodes.Count; i++)
                {
                    activeRow++;
                    ws.Cell(activeRow, 1).SetValue(view.ZipCodes[i]);
                    for (var j = 0; j < view.QORKTitles.Length; j++)
                    {
                        if (j == 6)
                        {
                            ws.Cell(activeRow, j + 1).SetValue("N/A");
                            ws.Cell(activeRow, j + 2).SetValue(view.Counts[0, j + 1, i]);
                            break;
                        }
                        else
                        {
                            ws.Cell(activeRow, j + 2).SetValue(view.Counts[0, j + 1, i]);
                        }
                    }
                }

                activeRow++;
                ws.Cell(activeRow, 1).SetValue("Total Served:");
                for (var i = 0; i < view.QORKTitles.Length; i++)
                {
                    if (i == 6)
                    {
                        ws.Cell(activeRow, i + 1).SetValue("N/A");
                        ws.Cell(activeRow, i + 2).SetValue(view.Counts[0, i + 1, view.ZipCount]);
                        break;
                    }
                    else
                    {
                        ws.Cell(activeRow, i + 2).SetValue(view.Counts[0, i + 1, view.ZipCount]);
                    }
                }

                activeRow++;
                ws.Cell(activeRow, 1).SetValue("Food Program Hours:");
                ws.Cell(activeRow, 2).SetValue(view.HoursTotal[2, 2]);
                ws.Cell(activeRow, 3).SetValue("People Count:");
                ws.Cell(activeRow, 4).SetValue(view.HoursTotal[2, 1]);

                ws.Columns().AdjustToContents();
                var ms = new MemoryStream();
                workbook.SaveAs(ms);
                ms.Position = 0;
                return new FileStreamResult(ms, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                    { FileDownloadName = "QORK Report " + view.EndDateString + ".xlsx" };
            }

            private static List<ApplicationUser> GetUserMasterList(string role)
            {
                var listActiveUsers = AppRoutines.GetActiveUserList();
                var roleId = AppRoutines.GetRoleId(role);
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
                var holidays = HolidayRoutines.GetHolidays(monday.Year);

                using var db = new BHelpContext();
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

                return view;
            }
            private ReportsViewModel GetQORKReportView(DateTime endDate) // new QORK Report 02/22
            {
                using var db = new BHelpContext();
                DateTime startDate = endDate.AddDays(-6);
                var view = new ReportsViewModel()
                {
                    BeginDate = startDate,
                    EndDate = endDate
                };
                view.EndDateString = view.EndDate.ToString("M-d-yy");
                view.DateRangeTitle = startDate.ToShortDateString() + " - " + endDate.ToShortDateString();
                view.ReportTitle = view.EndDateString + " QORK Weekly Report";
                view.ZipCodes = AppRoutines.GetZipCodesList();
                // Load Counts - extra zip code is for totals row.
                view.Counts = new int[1, 9, view.ZipCodes.Count + 1]; // 0 (unused), Counts, Zipcodes
                view.ZipCount = view.ZipCodes.Count;
                var deliveries = db.Deliveries
                    .Where(d => d.Status == 1 && d.DateDelivered >= startDate
                                              && d.DateDelivered < endDate).ToList();

                foreach (var delivery in deliveries)
                {
                    var zipCount = view.ZipCodes.Count; // Extra zip code Row is for totals
                    for (var zip = 0; zip < view.ZipCodes.Count; zip++)
                    {
                        if (delivery.Zip == view.ZipCodes[zip]) // 0 (unused), Column, ZipCode
                        {
                            var c = Convert.ToInt32(delivery.Children);
                            var a = Convert.ToInt32(delivery.Adults);
                            var s = Convert.ToInt32(delivery.Seniors);
                            view.Counts[0, 1, zip] += c;
                            view.Counts[0, 1, zipCount] += c; //# children
                            view.Counts[0, 2, zip] += a;
                            view.Counts[0, 2, zipCount] += a; //# adults
                            view.Counts[0, 3, zip] += s;
                            view.Counts[0, 3, zipCount] += s; //# seniors
                            view.Counts[0, 4, zip]++;
                            view.Counts[0, 4, zipCount]++; //#  households served
                            var lbs = Convert.ToInt32(delivery.FullBags * 10 + delivery.HalfBags * 9);
                            view.Counts[0, 5, zip] += lbs;
                            view.Counts[0, 5, zipCount] += lbs; //pounds distributed
                            // column 6 - prepared meals served
                            view.Counts[0, 7, zip] += (a + c + s);
                            view.Counts[0, 7, zipCount] += (a + c + s); //# individuals served
                        }
                    }
                }

                // Section to add Volunteer Hours Summary ========================
                var hoursList = db.VolunteerHours.Where(h => h.Date >= startDate && h.Date <= endDate).ToList();
                double totalAHours = 0;
                double totalFHours = 0;
                double totalMHours = 0;
                var totalAPeople = 0;
                var totalFPeople = 0;
                var totalMPeople = 0;
                if (hoursList.Count != 0)
                {
                    foreach (var rec in hoursList)
                    {
                        switch (rec.Category)
                        {
                            case "A":
                                totalAHours += rec.Hours + rec.Minutes / 60f;
                                totalAPeople += rec.PeopleCount;
                                break;
                            case "F":
                                totalFHours += rec.Hours + rec.Minutes / 60f;
                                totalFPeople += rec.PeopleCount;
                                break;
                            case "M":
                                totalMHours += rec.Hours + rec.Minutes / 60f;
                                totalMPeople += rec.PeopleCount;
                                break;
                        }
                    }

                    view.HoursTotal = new string[3, 3]; // A-F-M, total volunteer hours 
                    view.HoursTotal[0, 0] = "Administration";
                    view.HoursTotal[0, 1] = totalAPeople.ToString();
                    view.HoursTotal[0, 2] = totalAHours.ToString("#.00");
                    view.HoursTotal[1, 0] = "Management";
                    view.HoursTotal[1, 1] = totalMPeople.ToString();
                    view.HoursTotal[1, 2] = totalMHours.ToString("#.00");
                    view.HoursTotal[2, 0] = "Food Program";
                    view.HoursTotal[2, 1] = totalFPeople.ToString();
                    view.HoursTotal[2, 2] = totalFHours.ToString("#.00");
                    view.ShowHoursTotals = true;
                }

                view.QORKTitles = new string[8];
                view.QORKTitles[0] = "Zip Code";
                view.QORKTitles[1] = "Children Served (<18)";
                view.QORKTitles[2] = "Adult Non-seniors Served(18-59)";
                view.QORKTitles[3] = "Seniors Served (60+)";
                view.QORKTitles[4] = "Households Served";
                view.QORKTitles[5] = "Pounds Distributed";
                view.QORKTitles[6] = "Prepared Meals Served";
                view.QORKTitles[7] = "Individuals Served";

                return view;
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
            private Holiday GetHolidayData(DateTime dt)
            {
                var holidays = HolidayRoutines.GetHolidays(dt.Year);
                return holidays.Find(h => h.CalculatedDate == dt);
            }

            [Authorize(Roles = "Reports,Administrator,Staff,Developer,Driver,OfficerOfTheDay")]
            public ActionResult ReturnToReportsMenu()
            {
                return RedirectToAction("ReportsMenu", "Reports");
            }
            public ActionResult SundayNext(DateTime sunday)
            {
                sunday = sunday.AddDays(7);
                return RedirectToAction("QORKReport", new { endingDate = sunday.ToShortDateString() });
            }
            public ActionResult SundayPrevious(DateTime sunday)
            {
                sunday = sunday.AddDays(-7);
                return RedirectToAction("QORKReport", new { endingDate = sunday.ToShortDateString() });
            }
            public ActionResult GiftCardsSetNextMonth(DateTime dt)
            {
                var startDate = HoursRoutines.GetNextMonthStartDate(dt);
                var endDate = HoursRoutines.GetNextMonthEndDate(startDate);
                return RedirectToAction("GiftCardsReport", new {startDate, endDate});
            }

            public ActionResult GiftCardsSetPreviousMonth(DateTime dt)
            {
                var startDate = HoursRoutines.GetPreviousMonthStartDate(dt);
                var endDate = HoursRoutines.GetPreviousMonthEndDate(dt);
                return RedirectToAction("GiftCardsReport", routeValues: new {startDate, endDate});
            }

    }
}