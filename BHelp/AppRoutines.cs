using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
            using (var db = new BHelpContext())
            {
                var delivery = db.Deliveries.Where(i => i.ClientId == clientId
                                                        && i.DateDelivered != null)
                    .OrderByDescending(d => d.DateDelivered).FirstOrDefault();
                if (delivery?.DateDelivered != null) return (DateTime)delivery.DateDelivered;
            }
            return dt;
        }
        public static DateTime GetPriorDeliveryDate(int clientId, DateTime callLogDate)
        {
            var dt = DateTime.MinValue;
            using (var db = new BHelpContext())
            {
                var delivery = db.Deliveries.Where(i => i.ClientId == clientId
                                                        && i.DateDelivered < callLogDate)
                    .OrderByDescending(d => d.DateDelivered).FirstOrDefault();
                if (delivery?.DateDelivered != null) return (DateTime)delivery.DateDelivered;
            }
            return dt;
        }
        public static DateTime GetDateLastGiftCard(int clientId)
        {
            var dt = DateTime.MinValue;
            using (var db = new BHelpContext())
            {
                var delivery = db.Deliveries.Where(i => i.ClientId == clientId && i.GiftCards > 0)
                    .OrderByDescending(d => d.DateDelivered).FirstOrDefault();
                if (delivery?.DateDelivered != null) return (DateTime)delivery.DateDelivered;
            }
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
            var totalThisMonth = GetGiftCardsThisMonth(clientId, firstOfMonth);
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
        private static int GetDeliveriesCountThisMonth(int clientId, DateTime dt)
        {
            var startDate = new DateTime(dt.Year, dt.Month, 1);
            var endDate = new DateTime(dt.Year, dt.Month, DateTime.DaysInMonth(dt.Year, dt.Month));
            using (var db = new BHelpContext())
            {
                var dtm= db.Deliveries.Count(i => i.ClientId == clientId
                                 && i.DateDelivered >= startDate && i.DateDelivered <= endDate);
                return dtm;
            }
        }
        public static int GetGiftCardsThisMonth(int clientId, DateTime dt)
        {
            var giftCardCount = 0;
            var delList = GetAllDeliveriesThisMonth(clientId, dt);
            foreach (var del in delList)
            {
                var cards = Convert.ToInt32(del.GiftCards);
                if (del.GiftCards != null) giftCardCount += cards;
            }
            return giftCardCount;
        }
        private static List<Delivery> GetAllDeliveriesThisMonth(int clientId, DateTime dt)
        {
            var startDate = new DateTime(dt.Year, dt.Month, 1);
            var endDate = new DateTime(dt.Year, dt.Month, DateTime.DaysInMonth(dt.Year, dt.Month));
            using (var db = new BHelpContext())
            {
                return db.Deliveries.Where(i => i.ClientId == clientId
                && i.DateDelivered >= startDate && i.DateDelivered <= endDate).ToList();
            }
        }
        public static DateTime GetNextEligibleDeliveryDate(int clientId, DateTime dt)
        {
            var deliveriesThisMonth = GetDeliveriesCountThisMonth(clientId, dt);
            var lastDeliveryDate = GetLastDeliveryDate(clientId);
            var nextEligibleDate = lastDeliveryDate.AddDays(7);
          
            if (deliveriesThisMonth >= 3)    // if already 3 this month, no more
            {
                nextEligibleDate = lastDeliveryDate.AddMonths(1);
                nextEligibleDate = new DateTime( nextEligibleDate.Year,
                        nextEligibleDate.Month,1); // move it to next month
            }

            if (lastDeliveryDate < DateTime.Today.AddDays(-30))
            { nextEligibleDate = DateTime.Today; }
            return nextEligibleDate;
        }
        public static DateTime GetNextGiftCardEligibleDate(int clientId, DateTime dt)
        {
            // GIFT CARDS ELIGIBLE:
            // 1 per week maximum
            // 1 per household of 3 or fewer
            // 2 per household of 4 or more
            // 3 max per calendar month;
            var giftCardsThisMonth = GetGiftCardsThisMonth(clientId, dt);
            var lastGiftCardDate = GetDateLastGiftCard(clientId);
            var nextEligibleDate = lastGiftCardDate.AddDays(7);
            if (giftCardsThisMonth >= 3)    // if already 3 this month, no more
            {
                nextEligibleDate = nextEligibleDate.AddMonths(1);
                nextEligibleDate =new DateTime( nextEligibleDate.Year,
                nextEligibleDate.Month, 1); // move it to next month
            }

            var nedd = GetNextEligibleDeliveryDate(clientId, dt);
            if (lastGiftCardDate < nedd)
            {
                nextEligibleDate = nedd;
            }
            return nextEligibleDate;
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
            if (today.ToString(CultureInfo.CurrentCulture).IsNullOrEmpty())
            { today = DateTime.Today; };
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
            using (var db = new BHelpContext())
            {
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
        }
        public static List<FamilyMember> GetFamilyMembers(int clientId)
        {
            var familyMembers = new List<FamilyMember>(); // For editiing

            using (var db = new BHelpContext())
            {
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

                var sqlString = "SELECT * FROM FamilyMembers ";
                sqlString += "WHERE Active > 0 AND ClientId =" + clientId;
                var familyList = db.Database.SqlQuery<FamilyMember>(sqlString).ToList();

                foreach (FamilyMember member in familyList)
                {
                    member.Age = GetAge(member.DateOfBirth, DateTime.Today);
                    member.NameAge = member.FirstName + " " + member.LastName + "/" + member.Age;
                    familyMembers.Add(member);
                }
            }
            return familyMembers;
        }
        public static List<SelectListItem> GetDriversSelectList()
        {
            List<SelectListItem> driverList = new List<SelectListItem>();
            using (var db = new BHelpContext())
            {
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
            }
            return (driverList);
        }
        public static List<SelectListItem> GetODSelectList()
        {
            List<SelectListItem> odList = new List<SelectListItem>();
            using (var db = new BHelpContext())
            {
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
            }
            return (odList);
        }
        public static Boolean UserIsInRole(string userId, string roleName)
        {
            var sqlString = "SELECT Id FROM AspNetRoles WHERE Name = '" + roleName + "'";
            string roleId;
            using (var context = new BHelpContext())
            {
                roleId = context.Database.SqlQuery<string>(sqlString).FirstOrDefault();
                if (roleId == null)
                {
                    return false;
                }
            }

            sqlString = "SELECT UserId FROM AspNetUserRoles WHERE ";
            sqlString += "UserId = '" + userId + "' AND RoleId ='" + roleId + "'";
            using (var context = new BHelpContext())
            {
                var success = context.Database.SqlQuery<string>(sqlString).FirstOrDefault();
                if (success != null)
                {
                    return true;
                }
                return false;
            }
        }
        public static int GetNumberOfKids2_17(int clientId)
        {
            // Assume Head of Household is not a Child
            using (var db = new BHelpContext())
            {
                var client = db.Clients.Find(clientId);
                if (client != null)
                {
                    var fromDate = DateTime.Today.AddYears(-17).ToShortDateString();
                    var thruDate = DateTime.Today.AddYears(-2).ToShortDateString();
                    var sqlString = "SELECT * FROM FamilyMembers ";
                    sqlString += "WHERE Active > 0 AND ClientId =" + clientId;
                    sqlString += " AND DateOfBirth >= '" + fromDate + "'";
                    sqlString += " AND DateOfBirth <= '" + thruDate + "'";
                    var familyList = db.Database.SqlQuery<FamilyMember>(sqlString).ToList();
                    return familyList.Count;
                }
            }
            return 0;
        }
        public static FileStreamResult OpenDeliveriesToExcel()
        {
            var view = GetOpenDeliveryViewModel();
            XLWorkbook workbook = new XLWorkbook();
            IXLWorksheet ws = workbook.Worksheets.Add(view.ReportTitle);
            int activeRow = 1;
            ws.Cell(activeRow, 1).SetValue(view.ReportTitle);
            ws.Cell(activeRow, 2).SetValue(DateTime.Today.ToShortDateString());
            activeRow++;
            ws.Cell(activeRow, 1).SetValue("Delivery Date");
            ws.Cell(activeRow, 2).SetValue("Driver");
            ws.Cell(activeRow, 3).SetValue("Zip Code");
            ws.Cell(activeRow, 4).SetValue("Client");
            ws.Cell(activeRow, 5).SetValue("Address");
            ws.Cell(activeRow, 6).SetValue("City");
            ws.Cell(activeRow, 7).SetValue("Phone");
            ws.Cell(activeRow, 8).SetValue("# in HH");
            ws.Cell(activeRow, 9).SetValue("Full Bags");
            ws.Cell(activeRow, 10).SetValue("Half Bags");
            ws.Cell(activeRow, 11).SetValue("Kid Snacks");
            ws.Cell(activeRow, 12).SetValue("Gift Cards");
            ws.Cell(activeRow, 13).SetValue("Client Notes");
            ws.Cell(activeRow, 14).SetValue("OD Notes");
            ws.Cell(activeRow, 15).SetValue("Driver Notes");

            for (var i = 0; i < view.OpenDeliveryCount; i++)
            {
                activeRow++;
                for (var j = 1; j < 16; j++)
                {
                    ws.Cell(activeRow, j).SetValue(view.OpenDeliveries[i, j]);
                }
            }


            ws.Columns().AdjustToContents();
            MemoryStream ms = new MemoryStream();
            workbook.SaveAs(ms);
            ms.Position = 0;
            return new FileStreamResult(ms, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                { FileDownloadName = view.ReportTitle + ".xlsx" };
        }
        private static OpenDeliveryViewModel GetOpenDeliveryViewModel()
        {
            var odv = new OpenDeliveryViewModel
                {ReportTitle = "Bethesda Help Open Deliveries"};

            using (var db = new BHelpContext())
            { 
                var deliveryList = new List<Delivery>(db.Deliveries)
                    .Where(d => d.Completed == false)
                    .OrderBy(d => d.DeliveryDate)
                    .ThenBy(d => d.DriverId)
                    .ThenBy(z => z.Zip)
                    .ThenBy(n => n.LastName).ToList();
                odv.OpenDeliveryCount = deliveryList.Count;
                odv.OpenDeliveries = new string[deliveryList.Count, 16];
                var i = 0;
                foreach (var del in deliveryList)
                {
                    var client = db.Clients.Find(del.ClientId);
                    odv.OpenDeliveries[i, 1] = del.DeliveryDate.ToShortDateString();

                    var driver = db.Users.Find(del.DriverId);
                    if (driver != null)
                    {
                        odv.OpenDeliveries[i, 2] = driver.FullName;
                    }

                    odv.OpenDeliveries[i, 3] = del.Zip;
                    odv.OpenDeliveries[i, 4] = del.LastName + ", " + del.FirstName;
                    odv.OpenDeliveries[i, 5] = del.StreetNumber + " " + del.StreetName;
                    odv.OpenDeliveries[i, 6] = del.City;
                    odv.OpenDeliveries[i, 7] = del.Phone;
                    if (client != null)
                    {
                        var familyMemberCount = db.FamilyMembers.Count(c => c.ClientId == client.Id);
                        odv.OpenDeliveries[i, 8] = (familyMemberCount + 1).ToString();
                        
                        odv.OpenDeliveries[i, 13] = client.Notes;
                    }

                    odv.OpenDeliveries[i, 9] = del.FullBags.ToString();
                    odv.OpenDeliveries[i, 10] = del.HalfBags.ToString();
                    odv.OpenDeliveries[i, 11] = del.KidSnacks.ToString();
                    odv.OpenDeliveries[i, 12] = del.GiftCards.ToString();

                    odv.OpenDeliveries[i, 14] = del.ODNotes;
                    odv.OpenDeliveries[i, 15] = del.DriverNotes;
                    i++;
                }

                return odv;
            }
        }
    }
}