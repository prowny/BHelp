using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web.Mvc;
using BHelp.DataAccessLayer;
using BHelp.Models;
using Castle.Core.Internal;

namespace BHelp
{
    public static class AppRoutines
    {
        public static DateTime GetLastDeliveryDate(int clientId)
        {
            var dt = DateTime.MinValue;
            using (var db = new BHelpContext())
            {
                var delivery = db.Deliveries.Where(i => i.ClientId == clientId)
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
        public static List<SelectListItem> GetZipCodesSelectList()
        {
            List<SelectListItem> getZipCodesSelectList = new List<SelectListItem>();
            string[] lines =
                System.IO.File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory + "/App_Data/BHelpZipCodes.txt");
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
        public static int GetDeliveriesThisMonth(int clientId)
        {
            var endDate = DateTime.Today;
            var startDate = new DateTime(endDate.Year, endDate.Month,1);
            using (var db = new BHelpContext())
            {
                var dtm= db.Deliveries.Count(i => i.ClientId == clientId
                                 && i.DateDelivered >= startDate && i.DateDelivered <= endDate);
                return dtm;
            }
        }
        public static int GetGiftCardsThisMonth(int clientId)
        {
            var giftCardCount = 0;
            var delList = GetAllDeliveriesThisMonth(clientId);
            foreach (var del in delList)
            {
                var cards = Convert.ToInt32(del.GiftCards);
                if (del.GiftCards != null) giftCardCount += cards;
            }
            return giftCardCount;
        }
        public static List<Delivery> GetAllDeliveriesThisMonth(int clientId)
        {
            var endDate = DateTime.Today;
            var startDate = new DateTime(endDate.Year, endDate.Month, 1);
            using (var db = new BHelpContext())
            {
                return db.Deliveries.Where(i => i.ClientId == clientId
                && i.DateDelivered >= startDate && i.DateDelivered <= endDate).ToList();
            }
        }
        public static DateTime GetNextEligibleDeliveryDate(int clientId)
        {
            var deliveriesThisMonth = GetDeliveriesThisMonth(clientId);
            var lastDeliveryDate = GetLastDeliveryDate(clientId);
            var nextEligibleDate = lastDeliveryDate.AddDays(7);
          
            if (deliveriesThisMonth >= 3)    // if already 3 this month, no more
            {
                    nextEligibleDate = lastDeliveryDate.AddDays(7); // move it to next month
            }

            if (lastDeliveryDate < DateTime.Today.AddDays(-30))
            { nextEligibleDate = DateTime.Today; }
            return nextEligibleDate;
        }
        public static DateTime GetNextGiftCardEligibleDate(int clientId)
        {
            // GIFT CARDS ELIGIBLE:
            // 1 per week maximum
            // 1 per household of 3 or fewer
            // 2 per household of 4 or more
            // 3 max per calendar month;
            var giftCardsThisMonth = GetGiftCardsThisMonth(clientId);
            var lastGiftCardDate = GetDateLastGiftCard(clientId);
            var nextEligibleDate = lastGiftCardDate.AddDays(7);
            if (giftCardsThisMonth >= 3)    // if already 3 this month, no more
            {
                nextEligibleDate = nextEligibleDate.AddDays(7); // move it to next month
            }

            var nedd = GetNextEligibleDeliveryDate(clientId);
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
                System.IO.File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory + "/App_Data/BHelpZipCodes.txt");
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
    }
}