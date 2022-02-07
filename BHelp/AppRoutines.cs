using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Web.Mvc;
using BHelp.DataAccessLayer;
using BHelp.Models;
using BHelp.ViewModels;
using Castle.Core.Internal;
using ClosedXML.Excel;

namespace BHelp
{
    public static class AppRoutines
    {
        public static DateTime GetLastDeliveryDate(int clientId)
        {
            var dt = DateTime.MinValue;
            var db = new BHelpContext();
            var delivery = db.Deliveries.Where(i => i.ClientId == clientId
                                   && i.Status == 1 && i.DateDelivered != null)
                                   .OrderByDescending(d => d.DateDelivered).FirstOrDefault();
                if (delivery?.DateDelivered != null) return (DateTime)delivery.DateDelivered;
            
            return dt;
        }
        public static DateTime GetPriorDeliveryDate(int clientId, DateTime callLogDate)
        {
            var dt = DateTime.MinValue;
            var db = new BHelpContext();
            var delivery = db.Deliveries.Where(i => i.ClientId == clientId
                                                    && i.Status == 1 && i.DateDelivered <= callLogDate)
                                      .OrderByDescending(d => d.DateDelivered).FirstOrDefault();
            if (delivery?.DateDelivered != null) return (DateTime)delivery.DateDelivered;
            return dt;
        }
        public static DateTime GetDateLastGiftCard(int clientId)
        {
            var dt = DateTime.MinValue;
            var db = new BHelpContext();
            var delivery = db.Deliveries.Where(i => i.ClientId == clientId && i.GiftCards > 0
                                  && i.Status == 1).OrderByDescending(d => d.DateDelivered).FirstOrDefault();
            if (delivery?.DateDelivered != null) return (DateTime)delivery.DateDelivered;
            return dt;
        }
        public static int GetGiftCardsEligible(int clientId, DateTime dt)
        {
            // GIFT CARDS ELIGIBLE, based on DesiredDeliveryDate:
            // 1 per household of 3 or fewer; 1 per household per calendar month max
            // 2 per household of 4 or more; 2 per household per calendar month max
            var eligible = 0;
            var numberInHousehold = GetFamilyMembers(clientId).Count;
            var firstOfMonth = new DateTime(dt.Year, dt.Month, 1);
            var totalThisMonth = GetAllGiftCardsThisMonth(clientId, firstOfMonth);
            if (numberInHousehold <= 3)   // 1 per household of 3 or fewer
            {
                eligible = 1;
                if (eligible + totalThisMonth > 1) eligible = 0;
            }
            if (numberInHousehold >= 4)    // 2 per household of 4 or more
            {
                eligible = 2;
                if (eligible + totalThisMonth > 2) eligible = 0;
            }

            return eligible;}
        public static List<SelectListItem> GetZipCodesSelectList()
        {
            List<SelectListItem> getZipCodesSelectList = new List<SelectListItem>();
            string[] lines =
                File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory + "/App_Data/BHelpZipCodes.txt");
            foreach (var line in lines)
            {
                if (line.Substring(0, 1) != "/")
                {
                    var selListItem = new SelectListItem() { Value = line, Text = line };
                    getZipCodesSelectList.Add(selListItem);
                }
            }
            return getZipCodesSelectList;
        }
        public static int GetDeliveriesCountThisMonth(int clientId, DateTime dt)
        {
            var startDate = new DateTime(dt.Year, dt.Month, 1);
            var endDate = new DateTime(dt.Year, dt.Month, DateTime.DaysInMonth(dt.Year, dt.Month));

            var db = new BHelpContext();
            var dtm= db.Deliveries.Count(i => i.ClientId == clientId && i.Status == 1 
                             && i.DateDelivered >= startDate && i.DateDelivered <= endDate);
            return dtm;
        }
        private static int GetAllGiftCardsThisMonth(int clientId, DateTime dt )
        {
            var giftCardCount = 0;
            var delList = GetAllDeliveriesThisMonth(clientId, dt);
            foreach (var del in delList)
            {
                var cards = Convert.ToInt32(del.GiftCards);
                giftCardCount += cards;
            }
            return giftCardCount;
        }
        public static DateTime GetDateLastGiftCard(int clientId, DateTime toDate)
        {
            var db = new BHelpContext();
            var delList = db.Deliveries.Where(d => d.ClientId == clientId 
                               && d.Status == 1 && d.DateDelivered < toDate
                               && d.GiftCards > 0).OrderByDescending(d => d.DateDelivered).ToList();
            if (delList.Count != 0)
            {
                var delivery = delList[0];
                if (delivery.DateDelivered.HasValue)
                {
                    return (DateTime)delivery.DateDelivered;
                }
            }
            return DateTime.MinValue;
        }
        public static int GetPriorGiftCardsThisMonth(int clientId, DateTime dt)
        {
            var giftCardCount = 0;
            var delList = GetAllDeliveriesThisMonth(clientId, dt);
            foreach (var del in delList)
            {
                if (del.DateDelivered < dt)
                {
                    var cards = del.GiftCards;
                    giftCardCount += cards;
                }
            }
            return giftCardCount;
        }
        private static List<Delivery> GetAllDeliveriesThisMonth(int clientId, DateTime dt)
        {
            var startDate = new DateTime(dt.Year, dt.Month, 1);
            var endDate = new DateTime(dt.Year, dt.Month, DateTime.DaysInMonth(dt.Year, dt.Month));
            var db = new BHelpContext();
            return db.Deliveries.Where(i => i.ClientId == clientId && i.Status == 1 
                && i.DateDelivered >= startDate && i.DateDelivered <= endDate).ToList();
        }
        public static DateTime GetNextEligibleDeliveryDate(int clientId, DateTime dt)
        {
            var deliveriesThisMonth = GetDeliveriesCountThisMonth(clientId, dt);
            var lastDeliveryDate = GetLastDeliveryDate(clientId);
            DateTime nextEligibleDate;
            if (lastDeliveryDate == DateTime.MinValue)
            {
                nextEligibleDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            }
            else
            {
                nextEligibleDate = lastDeliveryDate.AddDays(7);
            }

            if (deliveriesThisMonth >= 3)    // if already 3 this month, no more
            {
                nextEligibleDate = lastDeliveryDate.AddMonths(1);
                nextEligibleDate = new DateTime( nextEligibleDate.Year,
                        nextEligibleDate.Month,1); // move it to 1st of next month, unless less than 7 days
                if (nextEligibleDate < lastDeliveryDate.AddDays(7))
                {
                    nextEligibleDate = lastDeliveryDate.AddDays(7);
                }
            }
      
            return nextEligibleDate;
        }
        public static DateTime GetNextGiftCardEligibleDate(int clientId, DateTime dt)
        {
            // GIFT CARDS ELIGIBLE:
            // 1 per household of 3 or fewer; 1 per household per calendar month max
            // 2 per household of 4 or more; 2 per household per calendar month max;
            var monthlyEligible = 0;
            var numberInHousehold = GetFamilyMembers(clientId).Count;
            if (dt == DateTime.MinValue) {  dt = DateTime.Today; } 
            var firstOfMonth = new DateTime(dt.Year, dt.Month, 1);
            var totalThisMonth = GetAllGiftCardsThisMonth(clientId, firstOfMonth);
            if (numberInHousehold <= 3)   // 1 per household of 3 or fewer
            {
                monthlyEligible = 1;
                if (monthlyEligible + totalThisMonth > 1) monthlyEligible = 0;
            }
            if (numberInHousehold >= 4)    // 2 per household of 4 or more
            {
                monthlyEligible = 2;
                if (monthlyEligible + totalThisMonth > 2) monthlyEligible = 0;
            }

            var lastGiftCardDate = GetDateLastGiftCard(clientId);
            var nextEligibleGiftCardDate = lastGiftCardDate.AddDays(7);
            if (monthlyEligible == 0)   // move eligibility to 1st of next month
            {
                nextEligibleGiftCardDate = nextEligibleGiftCardDate.AddMonths(1);
                nextEligibleGiftCardDate = new DateTime(nextEligibleGiftCardDate.Year,
                    nextEligibleGiftCardDate.Month, 1); // move it to next month
            }

            if (lastGiftCardDate == DateTime.MinValue){ lastGiftCardDate= DateTime.Today.AddMonths(-1); }
            var succeedingMonthDate = lastGiftCardDate.AddMonths(1);
            var firstOfSucceedingMonth = new DateTime(succeedingMonthDate.Year, succeedingMonthDate.Month, 1);
            if (lastGiftCardDate < firstOfSucceedingMonth)
            {
                nextEligibleGiftCardDate = firstOfSucceedingMonth;
            }

            return nextEligibleGiftCardDate;
        }
        public static List<string> GetZipCodesList()
        {
            List<string> getZipCodesList = new List<string>();
            string[] lines =
                File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory + "/App_Data/BHelpZipCodes.txt");
            foreach (var line in lines)
            {
                if (line.Substring(0, 1) != "/")
                {
                    getZipCodesList.Add(line);
                }
            }
            return getZipCodesList;
        }
        public static int GetAge(DateTime dob, [Optional] DateTime today)
        {
            if (today.ToString(CultureInfo.CurrentCulture).IsNullOrEmpty() || today == DateTime.MinValue)
            { today = DateTime.Now; };
            TimeSpan span = today - dob;
            // Because we start at year 1 for the Gregorian
            // calendar, we must subtract a year here.
            int years = (DateTime.MinValue + span).Year - 1;
            return years;
        }
        public static string GetNamesAgesOfAllInHousehold(int clientId)
        {
            string NamesAges = "";
            var familyMembers = GetFamilyMembers(clientId);
            foreach (var familyMember in familyMembers)
            {
                var age = GetAge(familyMember.DateOfBirth, DateTime.Today).ToString();
                NamesAges += familyMember.FirstName + " " + familyMember.LastName + "/" + age + ", ";
            }

            NamesAges = NamesAges.Substring(0, NamesAges.Length - 2); // remove last ", "
            return NamesAges;
        }
        public static List<SelectListItem> GetFamilySelectList(int clientId)
        {
            List<SelectListItem> householdList = new List<SelectListItem>();
            var db = new BHelpContext();
            var client = db.Clients.Find(clientId);
            if (client != null)
            {
                client.FamilyMembers = GetFamilyMembers(clientId);
                foreach (var mbr in client.FamilyMembers)
                {
                    var text = mbr.FirstName + " " + mbr.LastName + "/" +
                               GetAge(mbr.DateOfBirth, DateTime.Today);
                    var selListItem = new SelectListItem() { Value = mbr.FirstName, Text = text };
                    householdList.Add(selListItem);
                }
            }
            return (householdList);
        }
        public static List<FamilyMember> GetFamilyMembers(int clientId)
        {
            var familyMembers = new List<FamilyMember>(); // For editiing
            var db = new BHelpContext();
            var client = db.Clients.Find(clientId);
            if (client != null)
            {
                FamilyMember headOfHousehold = new FamilyMember()
                {
                    FirstName = client.FirstName,
                    LastName = client.LastName,
                    DateOfBirth = client.DateOfBirth,
                    Age = GetAge(client.DateOfBirth, DateTime.Today)
                };
                familyMembers.Add(headOfHousehold);
            }
            var familyList = db.FamilyMembers.Where(d => d.Active && d.ClientId == clientId).ToList();
            foreach (FamilyMember member in familyList)
            {
                member.Age = GetAge(member.DateOfBirth, DateTime.Today);
                member.NameAge = member.FirstName + " " + member.LastName + "/" + member.Age;
                familyMembers.Add(member);
            }
            return familyMembers;
        }
        public static List<SelectListItem> GetDriversSelectList()
        {
            List<SelectListItem> driverList = new List<SelectListItem>();
            var db = new BHelpContext();
            var userList = db.Users.OrderBy(u => u.LastName).Where(a => a.Active).ToList();
            var selListItem = new SelectListItem() { Value = "0", Text = @"(nobody yet)" };
            driverList.Add(selListItem);
            foreach (var user in userList)
            {
                if (UserIsInRole(user.Id, "Driver"))
                {
                    var newListItem = new SelectListItem() { Value = user.Id, Text = user.FullName };
                    driverList.Add(newListItem);
                }
            }
            return (driverList);
        }
        public static List<SelectListItem> GetODSelectList()
        {
            List<SelectListItem> odList = new List<SelectListItem>();
            var db = new BHelpContext();
            var userList = db.Users.OrderBy(u => u.LastName).Where(a => a.Active).ToList();
            var selListItem = new SelectListItem() { Value = "0", Text = @"(nobody yet)" };
            odList.Add(selListItem);
            foreach (var user in userList)
            {
                if (UserIsInRole(user.Id, "OfficerOfTheDay"))
                {
                    var newListItem = new SelectListItem() { Value = user.Id, Text = user.FullName };
                    odList.Add(newListItem);
                }
            }
            return (odList);
        }
        public static Boolean UserIsInRole(string userId, string roleName)
        {
            var sqlString = "SELECT Id FROM AspNetRoles WHERE Name = '" + roleName + "'";
            var context = new BHelpContext();
            var roleId = context.Database.SqlQuery<string>(sqlString).FirstOrDefault();
            if (roleId == null) return false;
            
            sqlString = "SELECT UserId FROM AspNetUserRoles WHERE ";
            sqlString += "UserId = '" + userId + "' AND RoleId ='" + roleId + "'";
            var success = context.Database.SqlQuery<string>(sqlString).FirstOrDefault();
            if (success != null) return true;

            return false;
        }
        public static int GetNumberOfKids2_17(int clientId)
        {
            // Assume Head of Household is not a Child
            var db = new BHelpContext();
            var client = db.Clients.Find(clientId);
            if (client != null)
            {
                var count2_17 = 0;
                var familyList = db.FamilyMembers.Where(c => c.ClientId == clientId).ToList();
                foreach (var member in familyList)
                {
                    var age = GetAge(member.DateOfBirth);
                    if (age >= 2 && age <= 17)
                    {
                        count2_17++;
                    }
                }
                return count2_17;
            }
            return 0;
        }
        public static int GetNumberOfChildren(int clientId)
        {
            // Assume Head of Household is not a Child
            var db = new BHelpContext();
            var kidCount = 0;
            var familyList = db.FamilyMembers.Where(c => c.ClientId == clientId).ToList();
            foreach (var member in familyList)
            {
                var age = GetAge(member.DateOfBirth);
                if (age <= 17)
                {
                    kidCount++;
                }
            }
            return kidCount;
        }
        public static int GetNumberOfAdults(int clientId)
        {
            var db = new BHelpContext();
            var client = db.Clients.Find(clientId);
            if (client != null)
            {
                var adultCount = 0;
                var clientAge = GetAge(client.DateOfBirth);
                if (clientAge >= 18 && clientAge <= 59)
                {
                    adultCount++;

                }
                var familyList = db.FamilyMembers.Where(c => c.ClientId == clientId).ToList();
                foreach (var member in familyList)
                {
                    var age = GetAge(member.DateOfBirth);
                    if (age >= 18 && age <= 59)
                    {
                        adultCount++;
                    }
                }
                return adultCount;
            }
            return 0;
        }
        public static int GetNumberOfSeniors(int clientId)
        {
            var db = new BHelpContext();
            var client = db.Clients.Find(clientId);
            if (client != null)
            {
                var seniorsCount = 0;
                var clientAge = GetAge(client.DateOfBirth);
                if (clientAge >= 60)
                {
                    seniorsCount++;

                }
                var familyList = db.FamilyMembers.Where(c => c.ClientId == clientId).ToList();
                foreach (var member in familyList)
                {
                    var age = GetAge(member.DateOfBirth);
                    if (age >= 60)
                    {
                        seniorsCount++;
                    }
                }
                return seniorsCount;
            }
            return 0;
        }
        public static FileStreamResult ExcelOpenDeliveries(OpenDeliveryViewModel view)
        {
            // view Parameter contains data only from Filtered Opens 
            if (view == null) view = GetOpenDeliveriesViewModel();
            
            var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add(view.ReportTitle);
            
            ws.Columns("1").Width = 8; 
            ws.Cell(1, 1).SetValue(view.ReportTitle).Style.Font.SetBold(true);
            ws.Cell(1, 1).Style.Alignment.WrapText = true;
            ws.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
            ws.Columns("2").Width = 12; // "Date" / "Driver"
            ws.Cell(1, 2).SetValue(DateTime.Today.ToShortDateString()).Style.Font.SetBold(true);
            ws.Cell(1, 2).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;

            var key = "K = Kids 0-17, A = Adults 18-59, S = Adults 60+, HH = Household, ";
                key +="F = Full Bags, H = Half Bags, KS = Kids Snacks for ages 2-17, GC = Gift Cards";
            ws.Cell(1, 8).SetValue(key);
            ws.Cell(1, 8).Style.Alignment.WrapText = true;
            ws.Cell(1, 8).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
            ws.Range(ws.Cell(1,8), ws.Cell(1,16)).Merge();

            var deliveryDateODs = "";
            foreach (var delDtOD in view.DistinctDeliveryDatesODList)
            { deliveryDateODs += " OD on " + delDtOD.Value + ": " + delDtOD.Text + ";"; }

            if (deliveryDateODs.Substring(deliveryDateODs.Length - 1, 1) == ";")
            { deliveryDateODs = deliveryDateODs.Remove(deliveryDateODs.Length - 1); }
            ws.Cell(2, 1).SetValue(deliveryDateODs);
            ws.Cell(2, 1).Style.Alignment.WrapText = true;
            ws.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Range(ws.Cell(2, 1), ws.Cell(2, 18)).Merge();

            ws.Columns("1").Width = 10;
            ws.Cell(3, 1).SetValue("Delivery Date").Style.Font.SetBold(true);
            ws.Cell(3, 1).Style.Alignment.WrapText = true;
            ws.Cell(3, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
            ws.Columns("2").Width = 12;
            ws.Cell(3, 2).SetValue("Driver").Style.Font.SetBold(true);
            ws.Cell(3, 2).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
            ws.Columns("3").Width = 6;
            ws.Cell(3, 3).SetValue("Zip Code").Style.Font.SetBold(true);
            ws.Cell(3, 3).Style.Alignment.WrapText = true;
            ws.Cell(3, 3).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
            ws.Columns("4").Width = 12;
            ws.Cell(3, 4).SetValue("Client").Style.Font.SetBold(true);
            ws.Cell(3, 4).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
            ws.Columns("5").Width = 16;
            ws.Cell(3, 5).SetValue("Address").Style.Font.SetBold(true);
            ws.Cell(3, 5).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
            ws.Columns("6").Width = 9;
            ws.Cell(3, 6).SetValue("City").Style.Font.SetBold(true);
            ws.Cell(3, 6).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
            ws.Columns("7").Width = 12;
            ws.Cell(3, 7).SetValue("Phone").Style.Font.SetBold(true);
            ws.Cell(3, 7).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;

            ws.Columns("8").Width = 3;
            ws.Cell(3, 8).SetValue("#K").Style.Font.SetBold(true);
            ws.Cell(3, 8).Style.Alignment.WrapText = true;
            ws.Cell(3, 8).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;

            ws.Columns("9").Width = 3;
            ws.Cell(3, 9).SetValue("#A").Style.Font.SetBold(true);
            ws.Cell(3, 9).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;

            ws.Columns("10").Width = 3;
            ws.Cell(3, 10).SetValue("#S").Style.Font.SetBold(true);
            ws.Cell(3, 10).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;

            ws.Columns("11").Width = 4;
            ws.Cell(3, 11).SetValue("# in HH").Style.Font.SetBold(true);
            ws.Cell(3, 11).Style.Alignment.WrapText = true;
            ws.Cell(3, 11).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;

            ws.Columns("12").Width = 15;
            ws.Cell(3, 12).SetValue("All Household Members/Ages").Style.Font.SetBold(true);
            ws.Cell(3, 12).Style.Alignment.WrapText = true;
            ws.Cell(3, 12).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;

            ws.Columns("13").Width = 3;
            ws.Cell(3, 13).SetValue("#F").Style.Font.SetBold(true);
            ws.Cell(3, 13).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;

            ws.Columns("14").Width = 3;
            ws.Cell(3, 14).SetValue("#H").Style.Font.SetBold(true);
            ws.Cell(3, 14).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;

            ws.Columns("15").Width = 4;
            ws.Cell(3, 15).SetValue("#KS").Style.Font.SetBold(true);
            ws.Cell(3, 15).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;

            ws.Columns("16").Width = 4;
            ws.Cell(3, 16).SetValue("#GC").Style.Font.SetBold(true);
            ws.Cell(3, 16).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
            ws.Columns("17").Width = 15;
            ws.Cell(3, 17).SetValue("Client Permanent Notes").Style.Font.SetBold(true);
            ws.Cell(3, 17).Style.Alignment.WrapText = true;
            ws.Cell(3, 17).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
            ws.Columns("18").Width = 15;
            ws.Cell(3, 18).SetValue("OD & Driver Delivery Notes").Style.Font.SetBold(true);
            ws.Cell(3, 18).Style.Alignment.WrapText = true;
            ws.Cell(3, 18).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;

           var activeRow = 3;
           for (var i = 0; i < view.OpenDeliveryCount; i++)
            {
                activeRow++;
                for (var col = 1; col < 19; col++)
                {
                    ws.Cell(activeRow, col).SetValue(view.OpenDeliveries[i, col]);
                    ws.Cell(activeRow, col).Style.Alignment.WrapText = true;
                    ws.Cell(activeRow, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                    ws.Cell(activeRow, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }
            }

            var ms = new MemoryStream();
            workbook.SaveAs(ms);
            ms.Position = 0;
            return new FileStreamResult(ms, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            { FileDownloadName = "OpenDeliveries" + DateTime.Today.ToString("MM-dd-yy") + ".xlsx" };
        }
        public static FileStreamResult ExcelOpenSelectedDeliveries(OpenDeliveryViewModel view) // From OpenFilters
        {
            var workbook = new XLWorkbook();
            IXLWorksheet ws = workbook.Worksheets.Add(view.ReportTitle);

            ws.Columns("1").Width = 8;
            ws.Cell(1, 1).SetValue("Selected Deliveries").Style.Font.SetBold(true);
            ws.Cell(1, 1).Style.Alignment.WrapText = true;
            ws.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
            ws.Columns("2").Width = 12; // "Date" / "Driver"
            ws.Cell(1, 2).SetValue(DateTime.Today.ToShortDateString()).Style.Font.SetBold(true);
            ws.Cell(1, 2).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;

            ws.Columns("1").Width = 10;
            ws.Cell(2, 1).SetValue("Delivery Date").Style.Font.SetBold(true);
            ws.Cell(2, 1).Style.Alignment.WrapText = true;
            ws.Cell(2, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
            ws.Columns("2").Width = 12;
            ws.Cell(2, 2).SetValue("Driver").Style.Font.SetBold(true);
            ws.Cell(2, 2).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
            ws.Columns("3").Width = 6;
            ws.Cell(2, 3).SetValue("Zip Code").Style.Font.SetBold(true);
            ws.Cell(2, 3).Style.Alignment.WrapText = true;
            ws.Cell(2, 3).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
            ws.Columns("4").Width = 40;
            ws.Cell(2, 4).SetValue("Client").Style.Font.SetBold(true);
            ws.Cell(2, 4).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                    
            ws.Columns("5").Width = 12;
            ws.Cell(2, 5).SetValue("Phone").Style.Font.SetBold(true);
            ws.Cell(2, 5).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
            
            ws.Columns("6").Width = 4;
            ws.Cell(2, 6).SetValue("#HH").Style.Font.SetBold(true);
            ws.Cell(2, 6).Style.Alignment.WrapText = true;
            ws.Cell(2, 6).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;

            ws.Columns("7").Width = 3;
            ws.Cell(2, 7).SetValue("#F").Style.Font.SetBold(true);
            ws.Cell(2, 7).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;

            ws.Columns("8").Width = 3;
            ws.Cell(2, 8).SetValue("#H").Style.Font.SetBold(true);
            ws.Cell(2, 8).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;

            ws.Columns("9").Width = 4;
            ws.Cell(2, 9).SetValue("#KS").Style.Font.SetBold(true);
            ws.Cell(2, 9).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;

            ws.Columns("10").Width = 4;
            ws.Cell(2, 10).SetValue("#GC").Style.Font.SetBold(true);
            ws.Cell(2, 10).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
            ws.Columns("11").Width = 15;
            ws.Cell(2, 11).SetValue("Client Notes").Style.Font.SetBold(true);
            ws.Cell(2, 11).Style.Alignment.WrapText = true;
            ws.Cell(2, 11).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
            ws.Columns("12").Width = 15;
            ws.Cell(2, 12).SetValue("OD & Driver Delivery Notes").Style.Font.SetBold(true);
            ws.Cell(2, 12).Style.Alignment.WrapText = true;
            ws.Cell(2, 12).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;

            int activeRow = 2;
            for (var i = 1; i < view.OpenDeliveryCount; i++)
            {
                activeRow++;
                for (var col = 1; col < 13; col++)
                {
                    ws.Cell(activeRow, col).SetValue(view.OpenDeliveries[i, col]);
                    ws.Cell(activeRow, col).Style.Alignment.WrapText = true;
                    ws.Cell(activeRow, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                    ws.Cell(activeRow, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }
            }
            //activeRow++;
            //ws.Cell(activeRow, 1).SetValue(view.OpenDeliveries[0,1]); //last Date
            //ws.Cell(activeRow, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
            //ws.Cell(activeRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            //ws.Cell(activeRow, 4).SetValue(view.OpenDeliveries[0, 4]); //last OD Name & Phone
            //ws.Cell(activeRow, 4).Style.Alignment.WrapText = true;
            //ws.Cell(activeRow, 4).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
            //ws.Cell(activeRow, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            var ms = new MemoryStream();
            workbook.SaveAs(ms);
            ms.Position = 0;
            return new FileStreamResult(ms, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            { FileDownloadName = "BHELPDeliveries" + DateTime.Today.ToString("MM-dd-yy") + ".xlsx" };
        }
        public static FileStreamResult CSVOpenSelectedDeliveries(OpenDeliveryViewModel view) // From Open Filters
        {
            //var view = GetOpenDeliveriesViewModel();
                var sb = new StringBuilder();

                sb.Append(view.ReportTitle + ',');
                sb.Append(DateTime.Today.ToShortDateString() + ',');
                sb.Append(",,,,,");
                var key = "K = Kids 0-17, A = Adults 18-59, S = Adults 60+, HH = Household, ";
                key += "F = Full Bags, H = Half Bags, KS = Kids Snacks for ages 2-17, GC = Gift Cards";
                sb.Append("\"" + key + "\"");
                sb.AppendLine();

                sb.Append("Delivery Date,Driver,ZipCode,Client,Address,City,Phone,#K,#A,#S,# in HH,");
                sb.Append("All Household Members/Ages,#F,#H,#KS,#GC,");
                sb.Append("Client Permanent Notes,OD & Driver Delivery Notes");
                sb.AppendLine();

                for (var i = 0; i < view.OpenDeliveryCount; i++)
                {
                    for (var col = 1; col < 18; col++)
                    {
                        if (view.OpenDeliveries[i, col] != null)
                        {
                            if (view.OpenDeliveries[i, col].Contains(","))
                            {
                                sb.Append("\"" + view.OpenDeliveries[i, col] + "\"" + ",");
                            }
                            else
                            {
                                sb.Append(view.OpenDeliveries[i, col] + ",");
                            }

                        }
                        else
                        {
                            sb.Append(view.OpenDeliveries[i, col] + ",");
                        }
                    }

                    sb.Append("\"" + view.OpenDeliveries[i, 18] + "\"");
                    sb.AppendLine();
                }

                var response = System.Web.HttpContext.Current.Response;
                response.BufferOutput = true;
                response.Clear();
                response.ClearHeaders();
                response.ContentEncoding = Encoding.Unicode;
                response.AddHeader("content-disposition", "attachment;filename=" + view.ReportTitle + ".csv");
                response.ContentType = "text/plain";
                response.Write(sb.ToString());
                response.End();
                return null;
            }
        private static OpenDeliveryViewModel GetOpenDeliveriesViewModel()
        {
            var odv = new OpenDeliveryViewModel
            //   OpenDeliveries[ Delivery, Column, Line ]
            { ReportTitle = "Bethesda Help Open Deliveries" };

            var db = new BHelpContext();
            var deliveryList = new List<Delivery>(db.Deliveries)
                .Where(d => d.Status == 0)
                .OrderBy(d => d.DateDelivered)
                //.ThenBy(d => d.DriverId)  // change 01/05/2022
                .ThenBy(z => z.Zip)
                .ThenBy(n => n.LastName).ToList();
            odv.OpenDeliveryCount = deliveryList.Count;

            odv.OpenDeliveries = new string[deliveryList.Count, 20];
            var i = 0;
            foreach (var del in deliveryList)
            {
                var client = db.Clients.Find(del.ClientId);
                if (del.DateDelivered != null) odv.OpenDeliveries[i, 1] = del.DateDelivered.Value.ToShortDateString();
                
                if (del.DeliveryDateODId != null)
                {
                    var usr = db.Users.Find(del.DeliveryDateODId);
                    del.DeliveryDateODName = usr.FullName + " " + usr.PhoneNumber;
                };

                var driver = db.Users.Find(del.DriverId);
                    if (driver != null)
                    { odv.OpenDeliveries[i, 2] = driver.FullName; }

                    odv.OpenDeliveries[i, 3] = del.Zip;
                    odv.OpenDeliveries[i, 4] = del.LastName + ", " + del.FirstName; // Client
                    odv.OpenDeliveries[i, 5] = del.StreetNumber + " " + del.StreetName;
                    odv.OpenDeliveries[i, 6] = del.City;
                    odv.OpenDeliveries[i, 7] = del.Phone;

                    if (client != null)
                    {
                        var familyMembers= db.FamilyMembers.Where(c => c.ClientId == client.Id).ToList();
                        //var kids2_17 = GetNumberOfKids2_17(client.Id);
                        var kidCount = AppRoutines.GetNumberOfChildren(client.Id);
                        odv.OpenDeliveries[i, 8] = kidCount.ToString();
                        odv.OpenDeliveries[i, 9] = AppRoutines.GetNumberOfAdults(client.Id).ToString();
                        odv.OpenDeliveries[i, 10] = AppRoutines.GetNumberOfSeniors(client.Id).ToString();
                        odv.OpenDeliveries[i, 11] = (familyMembers.Count + 1).ToString();
                        odv.OpenDeliveries[i, 12] = GetNamesAgesOfAllInHousehold(client.Id);

                        odv.OpenDeliveries[i, 17] = client.Notes;
                    }
                
                    odv.OpenDeliveries[i, 13] =  del.FullBags.ToString();
                    odv.OpenDeliveries[i, 14] =  del.HalfBags.ToString();
                    odv.OpenDeliveries[i, 15] =  del.KidSnacks.ToString();
                    odv.OpenDeliveries[i, 16] =  del.GiftCards.ToString();

                    odv.OpenDeliveries[i,18] = del.ODNotes + " " + del.DriverNotes;
                    i++;
            }
            odv.DistinctDeliveryDatesODList = GetDistinctDeliveryDatesOdList(deliveryList);

            return odv;
        }
        public static FileStreamResult OpenDeliveriesToCSV(OpenDeliveryViewModel view)
        {
            // view Parameter contains data only from Filtered Opens
            if (view == null) view = GetOpenDeliveriesViewModel();
            //var view = GetOpenDeliveriesViewModel();
            var sb = new StringBuilder();

            sb.Append(view.ReportTitle + ',');
            sb.Append(DateTime.Today.ToShortDateString() + ',');
            sb.Append(",,,,,");
            var key = "K = Kids 0-17, A = Adults 18-59, S = Adults 60+, HH = Household, ";
            key += "F = Full Bags, H = Half Bags, KS = Kids Snacks for ages 2-17, GC = Gift Cards";
            sb.Append("\"" + key + "\"");
            sb.AppendLine();

            sb.Append("Delivery Date,Driver,ZipCode,Client,Address,City,Phone,#K,#A,#S,# in HH,");
            sb.Append("All Household Members/Ages,#F,#H,#KS,#GC,");
            sb.Append("Client Permanent Notes,OD & Driver Delivery Notes");
            sb.AppendLine();

            for (var i = 0; i < view.OpenDeliveryCount; i++)
            {
                for (var col = 1; col < 18; col++)
                {
                    if (view.OpenDeliveries[i, col] != null)
                    {
                        if (view.OpenDeliveries[i, col].Contains(","))
                        {
                            sb.Append("\"" + view.OpenDeliveries[i, col] + "\"" + ",");
                        }
                        else
                        {
                            sb.Append(view.OpenDeliveries[i, col] + ",");
                        }

                    }
                    else
                    {
                        sb.Append(view.OpenDeliveries[i, col] + ",");
                    }
                }

                sb.Append("\"" + view.OpenDeliveries[i, 18] + "\"");
                sb.AppendLine();
            }

            var response = System.Web.HttpContext.Current.Response;
            response.BufferOutput = true;
            response.Clear();
            response.ClearHeaders();
            response.ContentEncoding = Encoding.Unicode;
            response.AddHeader("content-disposition", "attachment;filename=" + view.ReportTitle + ".csv");
            response.ContentType = "text/plain";
            response.Write(sb.ToString());
            response.End();
            return null;
        }
        public static FileStreamResult CallLogHistoryResultToCSV(DeliveryViewModel view, Boolean allData)
        {
            var db = new BHelpContext(); // for adding OD names if allData
            var sb = new StringBuilder();
            sb.Append(view.ReportTitle + ',');
            sb.AppendLine();

            sb.Append("Log Date,Name,Address,Driver,Delivery Date,ZipCode,Status,# in HH,#Children,");
            sb.Append("#Adults 18-59,# Seniors >=60,#Full Bags,#HalfBags,#Kid Snacks,");
            sb.Append("#Gift Cards,#Pounds of Food");
            if (allData)
            {
                sb.Append(",City,Phone,Household Names-Ages,Originating OD, Delivery Date OD,");
                sb.Append("OD Notes,Driver Notes,First Delivery");
            }
            sb.AppendLine();

            var totalHHCount = 0;
            var totalChildren = 0;
            var totalAdults = 0;
            var totalSeniors = 0;
            var totalFullBags = 0;
            var totalHalfBags = 0;
            var totalKidSnacks = 0;
            var totalGiftCards = 0;
            var totalPoundsOfFood = 0;
            foreach (var d in view.DeliveryList)
            {
                if (d == null) continue;
                sb.Append(d.LogDate.ToShortDateString() + ",");
                sb.Append("\"" + d.LastName + ", " + d.FirstName + "\"" + ",");
                sb.Append("\"" + d.StreetNumber + " " + d.StreetName + "\"" + ","); 
                sb.Append(d.DriverName + ",");
                var dtDel = "";
                if (d.DateDelivered != null) dtDel = d.DateDelivered.Value.ToString("MM/dd/yyyy");
                sb.Append(dtDel + ",");
                sb.Append(d.Zip + ",");
                var status = "";
                switch (d.Status)
                {
                    case 0: status = "Open"; break;
                    case 1: status = "Delivered"; break;
                    case 2: status = "Undelivered"; break;
                }
                sb.Append(status + ",");
                sb.Append(d.HouseoldCount + "," + d.Children + "," + d.Adults + "," + d.Seniors + ",");
                sb.Append(d.FullBags + "," + d.HalfBags + "," + d.KidSnacks + "," + d.GiftCards + ",");
                sb.Append(d.PoundsOfFood);
                totalHHCount += d.HouseoldCount;
                totalChildren += d.Children;
                totalAdults += d.Adults;
                totalSeniors += d.Seniors;
                totalFullBags += d.FullBags;
                totalHalfBags += d.HalfBags;
                totalKidSnacks += d.KidSnacks;
                totalGiftCards += d.GiftCards;
                totalPoundsOfFood += d.PoundsOfFood;
                
                if (allData)
                {
                    var _namesAges = "";
                    if (d.NamesAgesInHH != null)  _namesAges = d.NamesAgesInHH.Replace(",", " ");
                    sb.Append("," + d.City + "," +d.Phone + "," + _namesAges + ",");
                    if (d.ODId != null)
                    {
                        var _usr = db.Users.Find(d.ODId);
                        if (_usr != null) d.ODName = _usr.FullName;
                    }
                    if (d.DeliveryDateODId !=null){
                        var _usr = db.Users.Find(d.DeliveryDateODId);
                        if (_usr != null) d.DeliveryDateODName = _usr.FullName;
                    }
                    
                    sb.Append(d.ODName + "," + d.DeliveryDateODName + ",");
                    var _firstDelivery = "false";
                    if (d.FirstDelivery) _firstDelivery = "true";
                    var _ODNotes = "";
                    if (d.ODNotes !=null) _ODNotes = d.ODNotes.Replace(",", ";");
                    var _driverNotes = "";
                    if(d.DriverNotes != null) _driverNotes = d.DriverNotes.Replace(",", ";");
                    sb.Append(_ODNotes + "," + _driverNotes + "," + _firstDelivery);
                }
                sb.AppendLine();
            }

            sb.Append("Totals,,,,,,,");
            sb.Append(totalHHCount + "," + totalChildren + "," + totalAdults + "," + totalSeniors + ",");
            sb.Append(totalFullBags + "," + totalHalfBags + "," + totalKidSnacks + ",");
            sb.Append(totalGiftCards + "," + totalPoundsOfFood);
           
            var response = System.Web.HttpContext.Current.Response;
            response.BufferOutput = true;
            response.Clear();
            response.ClearHeaders();
            response.ContentEncoding = Encoding.Unicode;
            response.AddHeader("content-disposition", "attachment;filename=" + view.ReportTitle + ".csv");
            response.ContentType = "text/plain";
            response.Write(sb.ToString());
            response.End();
            return null;
        }

        public static List<SelectListItem> GetDistinctDeliveryDatesOdList(List<Delivery> deliveryList)
        {
            var distinctDeliveryDatesOdList = new List<SelectListItem>();
            var distinctDatesList = deliveryList.Select(d => d.DateDelivered).Distinct().ToList();
            var distinctDates = "";
            foreach (var dt in distinctDatesList)
            {
                if (dt != null)
                {
                    // Get delDate ODIDs for each distinct del date
                    for (var i = 0; i<deliveryList.Count; i++)
                    {
                        var del = deliveryList[i];
                        if (del.DateDelivered == dt && del.DeliveryDateODName != null)
                        {
                            if (!distinctDates.Contains(dt.Value.ToString("MM/dd/yyyy")))
                            {
                                distinctDeliveryDatesOdList.Add(new SelectListItem()
                                {
                                    Value = dt.Value.ToString("MM/dd/yyyy"),
                                    Text = del.DeliveryDateODName
                                });
                                distinctDates += dt.Value.ToString("MM/dd/yyyy") + ",";
                            }
                        }
                    }
                }
            }

            return distinctDeliveryDatesOdList;
        }
    }
}