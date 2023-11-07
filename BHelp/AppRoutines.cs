using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Web;
using System.Web.Mvc;
using BHelp.DataAccessLayer;
using BHelp.Models;
using BHelp.ViewModels;
using Castle.Core.Internal;
using ClosedXML.Excel;
using Microsoft.AspNet.Identity;

namespace BHelp
{
    public static class AppRoutines
    {
        public static bool IsDebug()
        {
            #if DEBUG
                        return true;
            #else
                    return false;
            #endif
        }

        public static List<SelectListItem> GetPaymentHistorySelectList(int clientId, DateTime? startDate, DateTime? endDate)
        {
            if (startDate == null) startDate = DateTime.MinValue;
            if (endDate == null || endDate == DateTime.MinValue) endDate = DateTime.Today;
            var catList = GetAssistanceCategoriesList(); 
            using var db = new BHelpContext();
            var payments = db.AssistancePayments
                        .Where(i => i.ClientId == clientId 
                        && i.Date >= startDate && i.Date <= endDate) 
                           .OrderByDescending( d => d.Date).ToList();
            //var historyList = "";

            var paymentHistorySelectList = new List<SelectListItem>();
            foreach (var pymnt in payments)
            {
                var strDt = pymnt.Date.ToString("MM/dd/yyyy ");  // space after the date
                var cat = catList[pymnt.CategoryId - 1];
                cat = (cat + "            ").Substring(0, 14);
                cat = cat.Replace(" ", "_");
                var amt = pymnt.AmountDecimal.ToString("C");
                amt = GetPaddedDollarAmount(amt);
                var item = new SelectListItem()
                {
                    Value = "0",
                    Text = strDt + cat + amt
                };
                paymentHistorySelectList.Add(item);
            }

            return paymentHistorySelectList;
        }

        public static AssistanceDataViewModel GetAssistancePaymentData(int clientId, DateTime? startDate, DateTime? endDate)
        {
            if (startDate == null) startDate = DateTime.MinValue;
            if (endDate == null) endDate = DateTime.Today;
            var catList = GetAssistanceCategoriesList();
            using var db = new BHelpContext();
            var payments = db.AssistancePayments
                .Where(i => i.ClientId == clientId
                            && i.Date >= startDate && i.Date <= endDate)
                .OrderByDescending(d => d.Date).ToList();
            if (payments.Count > 0)
            {
                var historyList = "";
                var currentYear = payments[0].Date.Year;
                decimal currentYTDTotal = 0;
                var numberOfPayments = 0;

                decimal grandTotal = 0;
                foreach (var pymnt in payments)
                {
                    if (pymnt.Date.Year != currentYear)  // Yearly subtotal
                    {
                        var curTotal = "           Total for " + currentYear;
                        var strAmt = currentYTDTotal.ToString("C");
                        curTotal += GetPaddedDollarAmount(strAmt);

                        historyList += curTotal + Environment.NewLine + Environment.NewLine;

                        currentYear = pymnt.Date.Year;
                        currentYTDTotal = 0;
                    }
                    var strDt = pymnt.Date.ToString("MM/dd/yyyy "); // note space after date
                    var cat = catList[pymnt.CategoryId - 1];
                    cat = (cat + "          ").Substring(0, 14);
                    var amt = pymnt.AmountDecimal.ToString("C");
                    amt = GetPaddedDollarAmount(amt);
                    historyList += strDt + cat + amt + Environment.NewLine;
                    grandTotal += pymnt.AmountDecimal;
                    currentYTDTotal += pymnt.AmountDecimal;
                    numberOfPayments += 1;
                }
                historyList += "           Total for " + currentYear.ToString();
                var curYTD = currentYTDTotal.ToString("C");
                historyList += GetPaddedDollarAmount(curYTD) + Environment.NewLine;
                historyList += Environment.NewLine;
                historyList += "             Grand Total ";
                historyList += GetPaddedDollarAmount(grandTotal.ToString("C"));

                var paymentData = new AssistanceDataViewModel()
                {
                    StartDate = payments[payments.Count -1].Date,
                    //StartDate = (DateTime)startDate,
                    EarliestPaymentDate = payments[payments.Count - 1].Date,  
                    EndDate = (DateTime)endDate,
                    PaymentHistoryList = historyList, 
                    GrandTotalString  = $"${grandTotal / 100}.{grandTotal % 100:00}", 
                    NumberOfPayments = numberOfPayments,
                    CategoryList = catList,
                    TotalsByCategoryString = new List<string>()
                };
          
                return paymentData;
            }

            var noPaymentData = new AssistanceDataViewModel()
            {
                StartDate = (DateTime)startDate,
                EndDate = (DateTime)endDate,
                CategoryList = catList,
                NumberOfPayments = 0,
                GrandTotalString  = "$0"
            };
            return noPaymentData; 
        }

        private static string GetPaddedDollarAmount(string amt)
        {
            switch (amt.Length) // set width to 10 spaces
            {
                case 4:
                    amt = "      " + amt;
                    break;
                case 5:
                    amt = "     " + amt;
                    break;
                case 6:
                    amt = "    " + amt;
                    break;
                case 7:
                    amt = "   " + amt;
                    break;
                case 8:
                    amt = "  " + amt;
                    break;
                case 9:
                    amt = " " + amt;
                    break;
            }

            return amt;
        }

        public static List<Client> GetAllClientsList()
        {
            using var db = new BHelpContext();
            return db.Clients.ToList();
        }

        public static string GetUserFullName(string id)
        {
            using var db = new BHelpContext();
            if (id == null) return "(nobody yet)";
            var user = db.Users.Find(id);
            if (user == null) return "(nobody yet)";
            return user.FullName;
        }

        public static Delivery NewDeliveryRecord(int clientId)
        {
            using var context = new BHelpContext();
            var client = context.Clients.Find(clientId);
            if (client == null) return null;
            var delDate = DateTime.Today.AddDays(1);
            if (DateTime.Today.DayOfWeek == DayOfWeek.Saturday)
            {
                delDate = DateTime.Today.AddDays(2);
            }

            if (DateTime.Today.DayOfWeek == DayOfWeek.Friday)
            {
                delDate = DateTime.Today.AddDays(3);
            }

            var delivery = new Delivery
            {
                ODId = HttpContext.Current.User.Identity.GetUserId(),
                DeliveryDateODId = GetODIdForDate(delDate),
                //DriverId = GetDriverIdForDate(delDate),  // Leave null for drivers to choose delivery
                ClientId = clientId,
                LogDate = DateTime.Today,
                FirstName = client.FirstName,
                LastName = client.LastName,
                StreetNumber = client.StreetNumber,
                StreetName = client.StreetName,
                Phone = client.Phone,
                City = client.City,
                Zip = client.Zip,
                NamesAgesInHH = GetNamesAgesOfAllInHousehold(clientId),
                Children = 0,
                Adults = 0,
                Seniors = 0,
                DateDelivered = delDate
            };

            var familyList = GetFamilyMembers(clientId);
            if (familyList != null)
            {
                foreach (var mbr in familyList)
                {
                    switch (mbr.Age)
                    {
                        case < 18:
                            delivery.Children += 1;
                            break;
                        case >= 18 and < 60:
                            delivery.Adults += 1;
                            break;
                        case >= 60:
                            delivery.Seniors += 1;
                            break;
                    }
                }
            }

            delivery.GiftCardsEligible = GetGiftCardsEligible(delivery.ClientId, delivery.DateDelivered.Value);
            delivery.GiftCards = delivery.GiftCardsEligible;

            // Full Bags (A Bags):
            var numberInHousehold = delivery.Children + delivery.Adults + delivery.Seniors;
            if (numberInHousehold <= 2)
            {
                delivery.FullBags = 1;
            }

            if (numberInHousehold is >= 3 and <= 4)
            {
                delivery.FullBags = 2;
            }

            if (numberInHousehold is 5 or 6)
            {
                delivery.FullBags = 3;
            }

            if (numberInHousehold is 7 or 8)
            {
                delivery.FullBags = 4;
            }

            if (numberInHousehold >= 9)
            {
                delivery.FullBags = 5;
            }

            // Half Bags:
            if (numberInHousehold <= 4)
            {
                delivery.HalfBags = 1;
            }

            if (numberInHousehold is >= 5 and <= 8)
            {
                delivery.HalfBags = 2;
            }

            if (numberInHousehold >= 9)
            {
                delivery.HalfBags = 3;
            }

            // Kid Snacks:
            delivery.KidSnacks = GetNumberOfKids2_17(clientId);

            delivery.FirstDelivery = context.Deliveries.Count(d => d.ClientId == clientId) == 0;

            return delivery;

        }

        public static DateTime GetLastDeliveryDate(int clientId)
        {
            var dt = DateTime.MinValue;
            using var db = new BHelpContext();
            var delivery = db.Deliveries.Where(i => i.ClientId == clientId
                                                    && i.Status == 1 && i.DateDelivered != null)
                .OrderByDescending(d => d.DateDelivered).FirstOrDefault();
            if (delivery?.DateDelivered != null) return (DateTime)delivery.DateDelivered;

            return dt;
        }

        public static DateTime GetPriorDeliveryDate(int clientId, DateTime callLogDate)
        {
            var dt = DateTime.MinValue;
            using var db = new BHelpContext();
            var delivery = db.Deliveries.Where(i => i.ClientId == clientId
                                                    && i.Status == 1 && i.DateDelivered <= callLogDate)
                .OrderByDescending(d => d.DateDelivered).FirstOrDefault();
            if (delivery?.DateDelivered != null) return (DateTime)delivery.DateDelivered;
            return dt;
        }

        public static DateTime GetDateLastGiftCard(int clientId)
        {
            var dt = DateTime.MinValue;
            using var db = new BHelpContext();
            var delivery = db.Deliveries.Where(i => i.ClientId == clientId 
                && (i.GiftCards > 0 || i.HolidayGiftCards >0) && i.Status == 1)
                .OrderByDescending(d => d.DateDelivered).FirstOrDefault();
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
            if (numberInHousehold <= 3) // 1 per household of 3 or fewer
            {
                eligible = 1;
                if (eligible + totalThisMonth > 1) eligible = 0;
            }

            if (numberInHousehold >= 4) // 2 per household of 4 or more
            {
                eligible = 2;
                if (eligible + totalThisMonth > 2) eligible = 0;
            }

            return eligible;
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

        public static List<string> GetAssistanceCategoriesList()
        {
            var GetAssistanceCategoriesList = new List<string>
            {
                "Utilities",
                "Rent",
                "Other - Medical",
                "Prescriptions"
            };

            return GetAssistanceCategoriesList;
        }

        public static List<SelectListItem> GetAssistanceCategoriesSelectList()
        {
            var GetAssistanceCategoriesSelectList = new List<SelectListItem>();

            var selListItem = new SelectListItem() { Value = "1", Text = "Utilities" };
            GetAssistanceCategoriesSelectList.Add(selListItem);
            selListItem = new SelectListItem() { Value = "2", Text = "Rent" };
            GetAssistanceCategoriesSelectList.Add(selListItem);
            selListItem = new SelectListItem() { Value = "3", Text = "Other - Medical" };
            GetAssistanceCategoriesSelectList.Add(selListItem);
            selListItem = new SelectListItem() { Value = "4", Text = "Prescriptions" };
            GetAssistanceCategoriesSelectList.Add(selListItem);

            return GetAssistanceCategoriesSelectList;
        }

        public static List<AssistanceViewModel> SearchClients(string searchString)
        {
            if (searchString == null) { return null; }

            var assistanceView = new List<AssistanceViewModel>();
            using var db = new BHelpContext();
            List<Client> clientList;
            if (searchString.Any(char.IsDigit))
            {
                clientList = db.Clients.Where(c => c.Phone.Contains(searchString)
                                                   || c.StreetNumber.Contains(searchString)
                                                   || c.Notes.Contains(searchString)
                                                   || c.StreetName.Contains(searchString)
                                                   && c.Active).OrderBy(c => c.LastName).ToList();
            }
            else
            {
                clientList = db.Clients.Where(c => c.Active && c.LastName.Contains(searchString))
                    .OrderBy(c => c.LastName).ToList();
            }

            foreach (var client in clientList)
            {
                var household = new AssistanceViewModel()
                {
                    ClientId = client.Id,
                    FirstName = client.FirstName,
                    LastName = client.LastName,
                    StreetNumber = client.StreetNumber,
                    StreetName = client.StreetName,
                    StreetToolTip = client.StreetName.Replace(" ", "\u00a0"),
                    City = client.City,
                    CityToolTip = client.City.Replace(" ", "\u00a0"),
                    Zip = client.Zip,
                    Phone = client.Phone,
                    PhoneToolTip = client.Phone.Replace(" ", "\u00a0"),
                    Notes = client.Notes,
                    // (full length on mouseover)    \u00a0 is the Unicode character for NO-BREAK-SPACE.
                    NotesToolTip = client.Notes.Replace(" ", "\u00a0")
                };

                var s = household.StreetName; // For display, abbreviate to 20 characters:           
                s = s.Length <= 20 ? s : s.Substring(0, 20) + "...";
                household.StreetName = s;
                s = household.City; // For display, abbreviate to 11 characters:           
                s = s.Length <= 11 ? s : s.Substring(0, 11) + "...";
                household.City = s;
                s = household.Phone; // For display, abbreviate to 12 characters:           
                s = s.Length <= 12 ? s : s.Substring(0, 12) + "...";
                household.Phone = s;
                s = household.Notes; // For display, abbreviate to 30 characters:           
                s = s.Length <= 30 ? s : s.Substring(0, 30) + "...";
                household.Notes = s;
                assistanceView.Add(household);
            }

            return (assistanceView);
        }

        public static int GetDeliveriesCountThisMonth(int clientId, DateTime dt)
        {
            var startDate = new DateTime(dt.Year, dt.Month, 1);
            var endDate = new DateTime(dt.Year, dt.Month, DateTime.DaysInMonth(dt.Year, dt.Month));

            using var db = new BHelpContext();
            var dtm = db.Deliveries.Count(i => i.ClientId == clientId && i.Status == 1
                                                                      && i.DateDelivered >= startDate &&
                                                                      i.DateDelivered <= endDate);
            return dtm;
        }

        private static int GetAllGiftCardsThisMonth(int clientId, DateTime dt)
        {
            var giftCardCount = 0;
            var delList = GetAllDeliveriesThisMonth(clientId, dt);
            foreach (var del in delList)
            {
                var cards = Convert.ToInt32(del.GiftCards) + Convert.ToInt32(del.HolidayGiftCards);
                giftCardCount += cards;
            }

            return giftCardCount;
        }

        public static DateTime GetDateLastGiftCard(int clientId, DateTime toDate)
        {
            using var db = new BHelpContext();
            var delList = db.Deliveries.Where(d => d.ClientId == clientId
                && d.Status == 1 && d.DateDelivered < toDate 
                && (d.GiftCards > 0 || d.HolidayGiftCards > 0))
                .OrderByDescending(d => d.DateDelivered).ToList();
            if (delList.Count == 0) return DateTime.MinValue;
            var delivery = delList[0];
            if (delivery.DateDelivered.HasValue)
            {
                return (DateTime)delivery.DateDelivered;
            }

            return DateTime.MinValue;
        }

        public static int GetPriorGiftCardsThisMonth(int clientId, DateTime dt)
        {
            var delList = GetAllDeliveriesThisMonth(clientId, dt);

            return delList.Where(del => del.DateDelivered < dt)
                .Sum(del => del.GiftCards + del.HolidayGiftCards);
        }

        private static List<Delivery> GetAllDeliveriesThisMonth(int clientId, DateTime dt)
        {
            var startDate = new DateTime(dt.Year, dt.Month, 1);
            var endDate = new DateTime(dt.Year, dt.Month, DateTime.DaysInMonth(dt.Year, dt.Month));
            using var db = new BHelpContext();
            return db.Deliveries
                .Where(i => i.ClientId == clientId && i.Status == 1
                && i.DateDelivered >= startDate
                && i.DateDelivered <= endDate).ToList();
        }

        public static DateTime GetNextEligibleDeliveryDate(int clientId, DateTime dt)
        {
            var deliveriesThisMonth = GetDeliveriesCountThisMonth(clientId, dt);
            var lastDeliveryDate = GetLastDeliveryDate(clientId);
            DateTime nextEligibleDate;
            if (lastDeliveryDate != DateTime.MinValue)
            {
                nextEligibleDate = lastDeliveryDate.AddDays(7);
            }
            else
            {
                nextEligibleDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            }

            if (deliveriesThisMonth < 3) return nextEligibleDate; // if already 3 this month, no more
            nextEligibleDate = lastDeliveryDate.AddMonths(1);
            nextEligibleDate = new DateTime(nextEligibleDate.Year,
                nextEligibleDate.Month, 1); // move it to 1st of next month, unless less than 7 days
            if (nextEligibleDate < lastDeliveryDate.AddDays(7))
            {
                nextEligibleDate = lastDeliveryDate.AddDays(7);
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
            if (dt == DateTime.MinValue)
            {
                dt = DateTime.Today;
            }

            var firstOfMonth = new DateTime(dt.Year, dt.Month, 1);
            var totalThisMonth = GetAllGiftCardsThisMonth(clientId, firstOfMonth);
            if (numberInHousehold <= 3) // 1 per household of 3 or fewer
            {
                monthlyEligible = 1;
                if (monthlyEligible + totalThisMonth > 1) monthlyEligible = 0;
            }

            if (numberInHousehold >= 4) // 2 per household of 4 or more
            {
                monthlyEligible = 2;
                if (monthlyEligible + totalThisMonth > 2) monthlyEligible = 0;
            }

            var lastGiftCardDate = GetDateLastGiftCard(clientId);
            var nextEligibleGiftCardDate = lastGiftCardDate.AddDays(7);
            if (monthlyEligible == 0) // move eligibility to 1st of next month
            {
                nextEligibleGiftCardDate = nextEligibleGiftCardDate.AddMonths(1);
                nextEligibleGiftCardDate = new DateTime(nextEligibleGiftCardDate.Year,
                    nextEligibleGiftCardDate.Month, 1); // move it to next month
            }

            if (lastGiftCardDate == DateTime.MinValue)
            {
                lastGiftCardDate = DateTime.Today.AddMonths(-1);
            }

            var succeedingMonthDate = lastGiftCardDate.AddMonths(1);
            var firstOfSucceedingMonth = new DateTime(succeedingMonthDate.Year, succeedingMonthDate.Month, 1);
            if (lastGiftCardDate < firstOfSucceedingMonth)
            {
                nextEligibleGiftCardDate = firstOfSucceedingMonth;
            }

            return nextEligibleGiftCardDate;
        }

        public static string GetVoicemailPassword()
        {
            var _password = "";
            var lines =
                File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory + "/App_Data/BHelpVoicemailCredentials.txt");
            foreach (var line in lines)
            {
                if (line.Substring(0, 1) != "/")
                {
                    _password = line;
                }
            }

            return _password;
        }

        public static string[] GetVoicemailInfoLines()
        {
            var lines = File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory
                                          + "/App_Data/BHelpVoicemailCredentials.txt");
            lines = lines.Take(lines.Count() - 1).ToArray();
            return lines;
        }

        public static int GetAge(DateTime dob, [Optional] DateTime today)
        {
            if (today.ToString(CultureInfo.CurrentCulture).IsNullOrEmpty() || today == DateTime.MinValue)
            {
                today = DateTime.Now;
            }
            
            var span = today - dob;
            // Because we start at year 1 for the Gregorian
            // calendar, we must subtract a year here.
            var years = (DateTime.MinValue + span).Year - 1;
            return years;
        }

        private static string GetODIdForDate(DateTime delDate)
        {
            using var db = new BHelpContext();
            var rec = db.ODSchedules.FirstOrDefault(d => d.Date == delDate);
            return rec?.ODId;
        }

        public static string GetNamesAgesOfAllInHousehold(int clientId)
        {
            var NamesAges = "";
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
            var householdList = new List<SelectListItem>();
            using var db = new BHelpContext();
            var client = db.Clients.Find(clientId);
            if (client == null) return (householdList);
            client.FamilyMembers = GetFamilyMembers(clientId);
            foreach (var mbr in client.FamilyMembers)
            {
                var text = mbr.FirstName + " " + mbr.LastName + "/" +
                           GetAge(mbr.DateOfBirth, DateTime.Today);
                var selListItem = new SelectListItem() { Value = mbr.FirstName, Text = text };
                householdList.Add(selListItem);
            }

            return (householdList);
        }

        public static List<FamilyMember> GetFamilyMembers(int clientId)
        {
            var familyMembers = new List<FamilyMember>(); // For editiing
            using var db = new BHelpContext();
            var client = db.Clients.Find(clientId);
            if (client != null)
            {
                var headOfHousehold = new FamilyMember()
                {
                    FirstName = client.FirstName,
                    LastName = client.LastName,
                    DateOfBirth = client.DateOfBirth,
                    Age = GetAge(client.DateOfBirth, DateTime.Today)
                };
                familyMembers.Add(headOfHousehold);
            }

            var familyList = db.FamilyMembers.Where(d => d.Active && d.ClientId == clientId).ToList();
            foreach (var member in familyList)
            {
                member.Age = GetAge(member.DateOfBirth, DateTime.Today);
                member.NameAge = member.FirstName + " " + member.LastName + "/" + member.Age;
                familyMembers.Add(member);
            }

            return familyMembers;
        }

        public static List<SelectListItem> GetDriversSelectList()
        {
            var driverList = new List<SelectListItem>();
            using var db = new BHelpContext();
            var userList = db.Users.OrderBy(u => u.LastName).Where(a => a.Active).ToList();
            var selListItem = new SelectListItem() { Value =null, Text = @"(nobody yet)" };
            driverList.Add(selListItem);
            foreach (var user in userList)
            {
                if (!UserIsInRole(user.Id, "Driver")) continue;
                var newListItem = new SelectListItem() { Value = user.Id, Text = user.FullName };
                driverList.Add(newListItem);
            }

            return (driverList);
        }

        public static List<SelectListItem> GetODSelectList()
        {
            var odList = new List<SelectListItem>();
            using var db = new BHelpContext();
            var userList = db.Users.OrderBy(u => u.LastName).Where(a => a.Active).ToList();
            var selListItem = new SelectListItem() { Value = null, Text = @"(nobody yet)" };
            odList.Add(selListItem);
            foreach (var user in userList)
            {
                if (!UserIsInRole(user.Id, "OfficerOfTheDay")) continue;
                var newListItem = new SelectListItem() { Value = user.Id, Text = user.FullName };
                odList.Add(newListItem);
            }

            return (odList);
        }

        public static List<SelectListItem> GetStatesSelectList()
        {
            var allStates = "MD,VA,DC,AL,AK,AR,AZ,CA,CO,CT,DE,FL,GA,HI,ID,IL,IN,IA,KS,KY,LA,ME,"
                            + "MA,MI,MN,MS,MO,MT,NE,NV,NH,NJ,NM,NY,NC,ND,OH,OK,OR,PA,RI,SC,"
                            + "SD,TN,TX,UT,VT,WA,WV,WI,WY";
            const string allStateNames = "Maryland,Virginia,District of Columbia,Alabama,Alaska,Arkansas,Arizona,California,Colorado,Connecticut,"
                                         + "Delaware,Florida,Georgia,Hawaii,Idaho,Illinois,Indiana,"
                                         + "Iowa,Kansas,Kentucky,Louisiana,Maine,Massachusetts,Michigan,"
                                         + "Minnesota,Mississippi,Missouri,Montana,Nebraska,Nevada,New Hampshire,"
                                         + "New Jersey,New Mexico,New York,North Carolina,North Dakota,Ohio,Oklahoma,"
                                         + "Oregon,Pennsylvania,Rhode Island,South Carolina,South Dakota,Tennessee,"
                                         + "Texas,Utah,Vermont,Washington,WestVirginia,Wisconsin,Wyoming";
            var statesList = allStates.Split(',');
            var stateNamesList = allStateNames.Split(',');
            var stList = new List<SelectListItem>();
            for (var i = 0; i < statesList.Length; i++)
            {
                var newListItem = new SelectListItem()
                    { Value = statesList[i], Text = stateNamesList[i] };
                stList.Add(newListItem);
            }

            return stList;
        }

        public static bool UserIsInRole(string userId, string roleName)
        {
            var sqlString = "SELECT Id FROM AspNetRoles WHERE Name = '" + roleName + "'";
            using var context = new BHelpContext();
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
            using var db = new BHelpContext();
            var client = db.Clients.Find(clientId);
            if (client != null)
            {
                var count2_17 = 0;
                var familyList = db.FamilyMembers.Where(c => c.ClientId == clientId).ToList();
                foreach (var member in familyList)
                {
                    var age = GetAge(member.DateOfBirth);
                    if (age is >= 2 and <= 17)
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
            using var db = new BHelpContext();
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
            using var db = new BHelpContext();
            var client = db.Clients.Find(clientId);
            if (client == null) return 0;
            var adultCount = 0;
            var clientAge = GetAge(client.DateOfBirth);
            if (clientAge is >= 18 and <= 59)
            {
                adultCount++;
            }

            var familyList = db.FamilyMembers.Where(c => c.ClientId == clientId).ToList();
            foreach (var member in familyList)
            {
                var age = GetAge(member.DateOfBirth);
                if (age is >= 18 and <= 59)
                {
                    adultCount++;
                }
            }

            return adultCount;

        }

        public static int GetNumberOfSeniors(int clientId)
        {
            using var db = new BHelpContext();
            var client = db.Clients.Find(clientId);
            if (client == null) return 0;
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

            var key = "K = Kids 0-17, A = Adults 18-59, S = Adults 60+,";
            key += " HH = Household, Abags = A Bags, Bbags = B Bags,";
            key += " KS = Kids Snacks for ages 2-17, GC = Gift Cards,";
            key += " HGC = Holiday Gift Cards";
            ws.Cell(1, 8).SetValue(key);
            ws.Cell(1, 8).Style.Alignment.WrapText = true;
            ws.Cell(1, 8).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
            ws.Range(ws.Cell(1, 8), ws.Cell(1, 16)).Merge();

            var deliveryDateODs = "";
            if (view.DistinctDeliveryDatesODList.Count > 0)
            {
                foreach (var delDtOD in view.DistinctDeliveryDatesODList)
                {
                    deliveryDateODs += " OD on " + delDtOD.Value + ": " + delDtOD.Text + ";";
                }

                if (deliveryDateODs.Substring(deliveryDateODs.Length - 1, 1) == ";")
                {
                    deliveryDateODs = deliveryDateODs.Remove(deliveryDateODs.Length - 1);
                }

                ws.Cell(2, 1).SetValue(deliveryDateODs);
                ws.Cell(2, 1).Style.Alignment.WrapText = true;
                ws.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Range(ws.Cell(2, 1), ws.Cell(2, 18)).Merge();
            }

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

            ws.Columns("13").Width = 5;
            ws.Cell(3, 13).SetValue("#Abags").Style.Font.SetBold(true);
            ws.Cell(3, 13).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;

            ws.Columns("14").Width = 5;
            ws.Cell(3, 14).SetValue("#Bbags").Style.Font.SetBold(true);
            ws.Cell(3, 14).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;

            ws.Columns("15").Width = 4;
            ws.Cell(3, 15).SetValue("#KS").Style.Font.SetBold(true);
            ws.Cell(3, 15).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;

            ws.Columns("16").Width = 4;
            ws.Cell(3, 16).SetValue("#GC").Style.Font.SetBold(true);
            ws.Cell(3, 16).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;

            ws.Columns("17").Width = 4;
            ws.Cell(3, 17).SetValue("#HGC").Style.Font.SetBold(true);
            ws.Cell(3, 17).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;


            ws.Columns("18").Width = 15;
            ws.Cell(3, 18).SetValue("Client Permanent Notes").Style.Font.SetBold(true);
            ws.Cell(3, 18).Style.Alignment.WrapText = true;
            ws.Cell(3, 18).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
            ws.Columns("19").Width = 15;
            ws.Cell(3, 19).SetValue("OD & Driver Delivery Notes").Style.Font.SetBold(true);
            ws.Cell(3, 19).Style.Alignment.WrapText = true;
            ws.Cell(3, 19).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;

            var activeRow = 3;
            for (var i = 0; i < view.OpenDeliveryCount; i++)
            {
                activeRow++;
                for (var col = 1; col < 20; col++)
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
     
        private static OpenDeliveryViewModel GetOpenDeliveriesViewModel()
        {
            var odv = new OpenDeliveryViewModel
                //   OpenDeliveries[ Delivery, Column, Line ]
                { ReportTitle = "Bethesda Help Open Deliveries" };

            using var db = new BHelpContext();
            var deliveryList = new List<Delivery>(db.Deliveries)
                .Where(d => d.Status == 0)
                .OrderBy(d => d.DateDelivered)
                .ThenBy(z => z.Zip)
                .ThenBy(s => s.StreetNumber)
                .ThenBy(n => n.StreetName)
                .ThenBy(n => n.LastName).ToList();
            odv.OpenDeliveryCount = deliveryList.Count;

            odv.OpenDeliveries = new string[deliveryList.Count, 21];
            var i = 0;
            foreach (var del in deliveryList)
            {
                var client = db.Clients.Find(del.ClientId);
                if (del.DateDelivered != null)
                    odv.OpenDeliveries[i, 1] = del.DateDelivered.Value.ToShortDateString();

                if (del.DeliveryDateODId != null && del.DeliveryDateODId != "0")
                {
                    var usr = db.Users.Find(del.DeliveryDateODId);
                    del.DeliveryDateODName = usr.FullName + " " + usr.PhoneNumber;
                }

                var driver = db.Users.Find(del.DriverId);
                if (driver != null)
                {
                    odv.OpenDeliveries[i, 2] = driver.FullName;
                }

                odv.OpenDeliveries[i, 3] = del.Zip;
                odv.OpenDeliveries[i, 4] = del.LastName + ", " + del.FirstName; // Client
                odv.OpenDeliveries[i, 5] = del.StreetNumber + " " + del.StreetName;
                odv.OpenDeliveries[i, 6] = del.City;
                odv.OpenDeliveries[i, 7] = del.Phone;

                if (client != null)
                {
                    var familyMembers = db.FamilyMembers
                        .Where(c => c.ClientId == client.Id).ToList();
                    var kidCount = AppRoutines.GetNumberOfChildren(client.Id);
                    odv.OpenDeliveries[i, 8] = kidCount.ToString();
                    odv.OpenDeliveries[i, 9] = AppRoutines.GetNumberOfAdults(client.Id).ToString();
                    odv.OpenDeliveries[i, 10] = AppRoutines.GetNumberOfSeniors(client.Id).ToString();
                    odv.OpenDeliveries[i, 11] = (familyMembers.Count + 1).ToString();
                    odv.OpenDeliveries[i, 12] = GetNamesAgesOfAllInHousehold(client.Id);

                    odv.OpenDeliveries[i, 18] = client.Notes;
                }

                odv.OpenDeliveries[i, 13] = del.FullBags.ToString();
                odv.OpenDeliveries[i, 14] = del.HalfBags.ToString();
                odv.OpenDeliveries[i, 15] = del.KidSnacks.ToString();
                odv.OpenDeliveries[i, 16] = del.GiftCards.ToString();
                odv.OpenDeliveries[i, 17] = del.HolidayGiftCards.ToString();
                // Client Notes [i, 18] already set above
                odv.OpenDeliveries[i, 19] = del.ODNotes + " " + del.DriverNotes;
               
                i++;
            }

            odv.DistinctDeliveryDatesODList = GetDistinctDeliveryDatesOdList(deliveryList);

            return odv;
        }

        public static FileStreamResult OpenDeliveriesToCSV(OpenDeliveryViewModel view)
        {
            // view Parameter contains data only from Filtered Opens
            if (view == null) view = GetOpenDeliveriesViewModel();
            var sb = new StringBuilder();

            sb.Append(view.ReportTitle + ',');
            sb.Append(DateTime.Today.ToShortDateString() + ',');
            sb.Append(",,,,,");
            var key = "K = Kids 0-17, A = Adults 18-59, S = Adults 60+,";
            key += "HH = Household,Abags = A Bags, Bbags = B Bags,";
            key += "KS = Kids Snacks for ages 2-17, GC = Gift Cards,";
            key += "HGC = Holiday Gift Cards";
            sb.Append("\"" + key + "\"");
            sb.AppendLine();

            sb.Append("Delivery Date,Driver,ZipCode,Client,Address,City,Phone,");
            sb.Append("#K,#A,#S,# in HH,All Household Members/Ages,");
            sb.Append("#ABags,#Bbags,#KS,#GC,#HGC,");
            sb.Append("Client Permanent Notes,OD & Driver Delivery Notes");
            sb.AppendLine();

            for (var i = 0; i < view.OpenDeliveryCount; i++)
            {
                for (var col = 1; col < 19; col++)
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

                sb.AppendLine();
            }

            var response = HttpContext.Current.Response;
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

        public static FileStreamResult CallLogHistoryResultToCSV(DeliveryViewModel view, bool allData)
        {
            using var db = new BHelpContext();
            // for adding OD names if allData
            var sb = new StringBuilder();
            sb.Append(view.ReportTitle);
            sb.Append("\n");

            sb.Append("Log Date,Name,Address,Driver,Delivery Date,ZipCode,Status,# in HH,#Children,");
            sb.Append("#Adults 18-59,# Seniors >=60,#A Bags,#B Bags,#Kid Snacks,");
            sb.Append("#Gift Cards,#HolidayGift Cards,#Pounds of Food");
            if (allData)
            {
                sb.Append(",City,Phone,Household Names-Ages,Originating OD, Delivery Date OD,");
                sb.Append("OD Notes,Driver Notes,First Delivery");
            }
            
            sb.Append("\n");

            var totalHHCount = 0;
            var totalChildren = 0;
            var totalAdults = 0;
            var totalSeniors = 0;
            var totalFullBags = 0;
            var totalHalfBags = 0;
            var totalKidSnacks = 0;
            var totalGiftCards = 0;
            var totalHolidayGiftCards = 0;
            var totalPoundsOfFood = 0;
            foreach (var d in view.DeliveryList)
            {
                if (d == null) continue;
                sb.Append(d.LogDate.ToShortDateString() + ",");
                var clientName = d.FirstName + " " + d.LastName;
                sb.Append(clientName + ",");
                var address = (d.StreetNumber + " " + d.StreetName).Replace(",", " ");
                sb.Append(address + ",");
                sb.Append(d.DriverName + ",");
                var dtDel = "";
                if (d.DateDelivered != null) dtDel = d.DateDelivered.Value.ToString("MM/dd/yyyy");
                sb.Append(dtDel + ",");
                sb.Append(d.Zip + ",");
                var status = "";
                switch (d.Status)
                {
                    case 0:
                        status = "Open";
                        break;
                    case 1:
                        status = "Delivered";
                        break;
                    case 2:
                        status = "Undelivered";
                        break;
                }

                sb.Append(status + ",");
                sb.Append(d.HouseoldCount + "," + d.Children + "," + d.Adults + "," + d.Seniors + ",");
                sb.Append(d.FullBags + "," + d.HalfBags + "," + d.KidSnacks + "," + d.GiftCards + ",");
                sb.Append(d.HolidayGiftCards +"," + d.PoundsOfFood);
                totalHHCount += d.HouseoldCount;
                totalChildren += d.Children;
                totalAdults += d.Adults;
                totalSeniors += d.Seniors;
                totalFullBags += d.FullBags;
                totalHalfBags += d.HalfBags;
                totalKidSnacks += d.KidSnacks;
                totalGiftCards += d.GiftCards;
                totalHolidayGiftCards += d.HolidayGiftCards;
                totalPoundsOfFood += d.PoundsOfFood;

                if (allData)
                {
                    var _namesAges = "";
                    if (d.NamesAgesInHH != null) _namesAges = d.NamesAgesInHH.Replace("\n", "")
                        .Replace("\r", "").Replace(",", " ");
                    var _phone = "";;
                    if (d.Phone != null) _phone = d.Phone.Replace("\n", "")
                        .Replace("\r", "").Replace(",", " ");
                    var _city = "";
                    if (d.City != null) _city = d.City.Replace("\n", "")
                        .Replace("\r", "").Replace(",", " ");
                    sb.Append("," +_city + "," + _phone + "," + _namesAges + ",");
                    if (d.ODId != null)
                    {
                        var _usr = db.Users.Find(d.ODId);
                         if (_usr != null) d.ODName = _usr.FullName;
                    }

                    if (d.DeliveryDateODId != null)
                    {
                        var _usr = db.Users.Find(d.DeliveryDateODId);
                        if (_usr != null) d.DeliveryDateODName = _usr.FullName;
                    }

                    sb.Append(d.ODName + "," + d.DeliveryDateODName + ",");
                    var _firstDelivery = "false";
                    if (d.FirstDelivery) _firstDelivery = "true";
                    var _ODNotes = "";
                    if (d.ODNotes != null)
                    {
                        _ODNotes = d.ODNotes.Replace("\n", "")
                            .Replace("\r","").Replace(",", " ");
                    }
                    var _driverNotes = "";
                    if (d.DriverNotes != null)
                    {
                        _driverNotes = d.DriverNotes.Replace("\n", "")
                            .Replace("\r", "").Replace(",", " ");
                    }
                    sb.Append(_ODNotes + "," + _driverNotes + "," + _firstDelivery);
                }

                sb.Append("\n");
            }

            sb.Append("Totals,,,,,,,");
            sb.Append(totalHHCount + "," + totalChildren + "," + totalAdults + "," + totalSeniors + ",");
            sb.Append(totalFullBags + "," + totalHalfBags + "," + totalKidSnacks + ",");
            sb.Append(totalGiftCards + "," + totalHolidayGiftCards + "," + totalPoundsOfFood);

            var response = HttpContext.Current.Response;
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

        public static FileStreamResult 
            
            QORKReportToCSV(ReportsViewModel view)
        {
            var sb = new StringBuilder();
            sb.Append(view.ReportTitle + ',');
            sb.AppendLine();

            sb.Append("ZipCode,Children Served (<18),Adult Non-seniors Served (18-59),");
            sb.Append("Seniors (60+),Households Served,Pounds Distributed,");
            sb.Append("Prepared Meals Served,Individuals Served");
            sb.AppendLine();

            for (var i = 0; i < view.ZipCount; i++)
            {
                sb.Append(view.ZipCodes[i] + ",");
                for (var j = 1; j < 8; j++)
                {
                    if (j == 6)
                    {
                        sb.Append("N/A,");
                    } //prepared meals column
                    else
                    {
                        sb.Append(view.Counts[0, j, i] + ",");
                    }
                }

                sb.AppendLine();
            }

            sb.Append("Total Served:,");
            for (var j = 1; j < 8; j++)
            {
                if (j == 6)
                {
                    sb.Append("N/A,");
                } // prepared meals column
                else
                {
                    sb.Append(view.Counts[0, j, view.ZipCount] + ",");
                }
            }

            sb.AppendLine();
            if (view.ShowHoursTotals)
            {
                var _hours = view.HoursTotal[2, 2];
                sb.Append("Food Program Hours:," + _hours + ",");
                var _people = view.HoursTotal[2, 1];
                sb.Append("People Count:," + _people);
                sb.AppendLine();
            }

            var response = HttpContext.Current.Response;
            response.BufferOutput = true;
            response.Clear();
            response.ClearHeaders();
            response.ContentEncoding = Encoding.Unicode;
            response.AddHeader("content-disposition", "attachment;filename="
                                                      + "QORK Report " + view.EndDateString + ".csv");
            response.ContentType = "text/plain";
            response.Write(sb.ToString());
            response.End();
            return null;
        }

        public static FileStreamResult CountyReportToCSV(ReportsViewModel view)
        {
            var sb = new StringBuilder();
            sb.Append("\"" + "Bethesda Help, Inc." + "\"");
            sb.AppendLine();
            sb.Append(view.DateRangeTitle);
            sb.AppendLine();
            sb.AppendLine();

            var commas = new string(',', view.ZipCodes.Count + 1);
            for (var i = 0; i < 3; i++)
            {
                sb.Append(view.MonthYear[i]);
                sb.Append(commas + "TOTAL");
                sb.AppendLine();

                for (var t = 0; t < view.CountyTitles.Length; t++)
                {
                    sb.Append(view.CountyTitles[t] + ',');
                    if (t == 0)
                    {
                        foreach (var z in view.ZipCodes)
                        {
                            sb.Append(z + ",");
                        }

                        sb.Append("All Zip Codes");
                        sb.AppendLine();
                    }
                    else
                    {
                        for (var j = 0; j < view.ZipCodes.Count + 1; j++)
                        {
                            sb.Append(view.Counts[i + 1, j, t - 1].ToString() + ',');
                        }

                        sb.AppendLine();
                    }
                }

                sb.AppendLine();
            }

            var response = HttpContext.Current.Response;
            response.BufferOutput = true;
            response.Clear();
            response.ClearHeaders();
            response.ContentEncoding = Encoding.Unicode;
            response.AddHeader("content-disposition", "attachment;filename="
                                                      + view.ReportTitle + ".csv");
            response.ContentType = "text/plain";
            response.Write(sb.ToString());
            response.End();
            return null;
        }

        public static FileStreamResult HelperReportToCSV(ReportsViewModel view)
        {
            var sb = new StringBuilder();
            sb.Append("\"" + "Bethesda Help, Inc. " + "\"" + view.DateRangeTitle + ',');
            sb.AppendLine();

            sb.Append("Time Period,");
            foreach (var z in view.ZipCodes)
            {
                sb.Append(z + ",");
            }

            sb.Append("Total Zip Codes");
            sb.AppendLine();

            var startDate = new DateTime(view.Year, view.Month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            sb.Append(startDate.ToString("d"));
            sb.AppendLine();
            sb.Append(endDate.ToString("d"));
            sb.AppendLine();

            for (var i = 1; i < 11; i++) // First 10 data rows
            {
                sb.Append(view.HelperTitles[i] + ",");
                for (var j = 1; j < view.ZipCodes.Count + 1; j++)
                {
                    sb.Append(view.ZipCounts[i, j] + ",");
                }

                var k = view.ZipCodes.Count + 1;
                sb.Append(view.ZipCounts[i, k]); // add totals column
                sb.AppendLine();
            }

            sb.AppendLine(); // blank line

            sb.Append("Distinct Households and Residents Served (NOT reported in the Helper)");
            sb.AppendLine();
            for (var i = 11; i < 20; i++) // Last 9 data rows
            {
                sb.Append(view.HelperTitles[i] + ",");
                for (var j = 1; j < view.ZipCodes.Count + 1; j++)
                {
                    sb.Append(view.ZipCounts[i, j] + ",");
                }

                var k = view.ZipCodes.Count + 1;
                sb.Append(view.ZipCounts[i, k]); // add totals column
                sb.AppendLine();
            }

            var response = HttpContext.Current.Response;
            response.BufferOutput = true;
            response.Clear();
            response.ClearHeaders();
            response.ContentEncoding = Encoding.Unicode;
            response.AddHeader("content-disposition", "attachment;filename="
                                                      + view.ReportTitle + ".csv");
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
                    for (var i = 0; i < deliveryList.Count; i++)
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

        public static List<UserRoleViewModel> UsersInRolesLookup()
        {
            using var db = new BHelpContext();
            var report = new UsersInRolesReportViewModel { Report = new List<List<string[]>>() };
            var headerLines = new List<string[]>
            {
                new[]
                {
                    DateTime.Today.ToShortDateString(), "", "", "", "", "Volunteer Roles and Start / End Dates"
                }
            };
            report.Report.Add(headerLines);

            var rolesList = db.Roles.OrderBy(r => r.Name).ToList();
            var sql = "SELECT UserId FROM AspNetUserRoles";
            var userIds = db.Database.SqlQuery<string>(sql).ToList();
            sql = "SELECT RoleId FROM AspNetUserRoles";
            var roleIds = db.Database.SqlQuery<string>(sql).ToList();
            var roleLookup = new List<UserRoleViewModel>();
            for (var i = 0; i < userIds.Count; i++)
            {
                var rName = rolesList.Where(r => r.Id == roleIds[i])
                    .Select(r => new { r.Name }).Single().ToString();
                var uRVM = new UserRoleViewModel
                {
                    UserId = userIds[i],
                    RoleId = roleIds[i],
                    RoleName = rName
                };
                roleLookup.Add(uRVM);
            }

            return roleLookup;
        }

        public static string GetRoleId(string name)
        {
            var sqlString = "SELECT Id FROM AspNetRoles WHERE Name = '" + name + "'";
            using var context = new BHelpContext();
            var roleId = context.Database.SqlQuery<string>(sqlString).FirstOrDefault();
            return roleId;
        }

        public static DateTime GetFirstWeekdayDate(int month, int year)
        {
            var dt = new DateTime(year, month, 1);
            var dayOfWeek = (int)dt.DayOfWeek;
            if (dayOfWeek == 0) dt = dt.AddDays(1); // change from Sun to Mon 
            if (dayOfWeek == 6) dt = dt.AddDays(2); // change from Sat to Mon
            return dt;
        }

        public static string[] GetShortMonthArray()
        {
            var monthList = new string[13];
            monthList[1] = "Jan";
            monthList[2] = "Feb";
            monthList[3] = "Mar";
            monthList[4] = "Apr";
            monthList[5] = "May";
            monthList[6] = "Jun";
            monthList[7] = "Jul";
            monthList[8] = "Aug";
            monthList[9] = "Sep";
            monthList[10] = "Oct";
            monthList[11] = "Nov";
            monthList[12] = "Dec";
            return monthList;
        }

        public static string[] GetShortWeekdayArray()
        {
            var weekdayList = new string[7];
            weekdayList[0] = "Sun";
            weekdayList[1] = "Mon";
            weekdayList[2] = "Tue";
            weekdayList[3] = "Wed";
            weekdayList[4] = "Thu";
            weekdayList[5] = "Fri";
            weekdayList[6] = "Sat";
            return weekdayList;
        }

        public static List<ApplicationUser> GetActiveUserList()
        {
            using var _db = new BHelpContext();
            var activeVolunteersList = _db.Users
                .Where(u => u.Active).OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName).ToList();
            return activeVolunteersList;
        }

        public static List<ApplicationUser> GetAllUserList() // include inactive users
        {
            using var _db = new BHelpContext();
            var allVolunteersList = _db.Users
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName).ToList();
            return allVolunteersList;
        }

        public static List<string> GetUserIdsInRole(string roleId)
        {
            // Load user roles lookup table
            using var db = new BHelpContext();
            var sqlString = "SELECT UserId FROM AspNetUserRoles WHERE ";
            sqlString += "RoleId = '" + roleId + "'";
            return db.Database.SqlQuery<string>(sqlString).ToList();
        }

        public static string GetUserEmail(string id)
        {
            using var _db = new BHelpContext();
            var user = _db.Users.Find(id);
            if (user == null)
            {
                return null;
            }
            else
            {
                return user.Email;
            }
        }

        public static string GetStringAllRolesForUser(string userId)
        {
            using var context = new BHelpContext();
            var sqlString = "SELECT Name from AspNetUserRoles "
                            + "LEFT JOIN AspNetRoles ON AspNetRoles.Id = AspNetUserRoles.RoleId "
                            + "WHERE AspNetUserRoles.UserId =  '" + userId + "'";

            var isEmpty = context.Database.SqlQuery<string>(sqlString).IsNullOrEmpty();
            var roleNameString = "";
            if (!isEmpty)
            {
                var roleNameList = context.Database.SqlQuery<string>(sqlString).ToList();
                foreach (var roleName in roleNameList)
                {
                    roleNameString = roleNameString + roleName + " ";
                }
            }

            if (roleNameString.Length > 0)
            {
                return roleNameString.Substring(0, roleNameString.Length - 1);
            }

            return roleNameString;
        }

        public static ODSchedule GetODSchedule(DateTime date)
        {
            using var _db = new BHelpContext();
            return _db.ODSchedules.FirstOrDefault(d => d.Date == date);
        }

        public static DriverSchedule GetDriverSchedule(DateTime date)
        {
            using var _db = new BHelpContext();
            return _db.DriverSchedules.FirstOrDefault(d => d.Date == date);
        }

        public static Client GetClientRecord(int id)
        {
            using var _db = new BHelpContext();
            return _db.Clients.FirstOrDefault(i => i.Id == id);
        }

        public static Delivery GetDeliveryRecord(int id)
        {
            using var _db = new BHelpContext();
            return _db.Deliveries.FirstOrDefault(i => i.Id == id);
        }

        public static string GetDriverName(string id)
        {
            using var _db = new BHelpContext();
            var driver = _db.Users.Find(id);
            if (driver != null)
            {
                return driver.FirstName + " " + driver.LastName;
            }
            else
            {
                return "(unknown)";
            }
        }
        public static DateTime GetFirstWeekDay(int month, int year)
        {
            var dt = new DateTime(year, month, 1);
            var dayOfWeek = (int)dt.DayOfWeek;
            if (dayOfWeek == 0) dt = dt.AddDays(1); // change from Sun to Mon 
            if (dayOfWeek == 6) dt = dt.AddDays(2); // change from Sat to Mon
            return dt;
        }
    }
}
