using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using BHelp.DataAccessLayer;
using BHelp.Models;
using BHelp.ViewModels;

namespace BHelp
{
    public static class ClientRoutines
    {
        public static ClientViewModel GetAllClientsListModel()
        {
            // Get Clients, Family, and Delieries in 3 single database queries
            // with no further hits to the database
            var view = new ClientViewModel { ReportTitle = "BH Client List" };
            const int columns = 24;

            using var db = new BHelpContext();
            var clientList = db.Clients.OrderBy(c => c.LastName)
                .ThenBy(c => c.FirstName).ToList();
            var familyList = db.FamilyMembers.ToList();
            var deliveryList = db.Deliveries.ToList();

            view.ClientCount = clientList.Count;
            view.ClientStrings = new string[clientList.Count, columns + 1];
            for (var i = 0; i < clientList.Count; i++)
            {
                var cli = clientList[i];
                //cvm.Id = cli.Id;
                view.ClientStrings[i, 1] = cli.Active.ToString();
                view.ClientStrings[i, 2] = cli.LastName;
                view.ClientStrings[i, 3] = cli.FirstName;
                var age = GetAge(cli.DateOfBirth);
                view.ClientStrings[i, 4] = age.ToString();
                view.ClientStrings[i, 5] = cli.StreetNumber;
                view.ClientStrings[i, 6] = cli.StreetName;
                view.ClientStrings[i, 7] = cli.City;
                view.ClientStrings[i, 8] = cli.Zip;
                view.ClientStrings[i, 9] = cli.Phone;
                view.ClientStrings[i, 10] = cli.Email;

                var family = familyList
                    .Where(f => f.ClientId == cli.Id).ToList();
                var hH = new FamilyMember() // add HeadOfHousehold:
                    { FirstName = cli.FirstName, LastName = cli.LastName, DateOfBirth = cli.DateOfBirth };
                family.Add(hH);
                view.ClientStrings[i, 11] = GetChildrenCount(family);
                view.ClientStrings[i, 12] = GetAdultCount(family);
                view.ClientStrings[i, 13] = GetSeniorCount(family);
                view.ClientStrings[i, 14] = GetChildrenNamesAges(family);
                view.ClientStrings[i, 15] = GetAdultNamesAges(family);
                view.ClientStrings[i, 16] = GetSeniorNamesAges(family); // !!! added row

                view.ClientStrings[i, 17] = family.Count.ToString();
                view.ClientStrings[i, 18] = cli.Notes;
                var deliverySubList = deliveryList
                    .Where(d => d.ClientId == cli.Id && d.Status == 1
                                                     && d.DateDelivered != null).ToList();
                var lastDD = GetLastDeliveryDate(deliverySubList);
                if (lastDD.Year < 2000)
                {
                    view.ClientStrings[i, 19] = " - - ";
                }
                else
                {
                    view.ClientStrings[i, 19] = lastDD.ToString("MM/dd/yyyy");
                }

                var lastGC = GetDateLastGiftCard(deliverySubList);
                if (lastGC.Year < 2000)
                {
                    view.ClientStrings[i, 20] = " - - ";
                }
                else
                {
                    view.ClientStrings[i, 20] = lastGC.ToString("MM/dd/yyyy");
                }

                var nextEDD = GetNextEligibleDeliveryDate(deliverySubList);
                if (nextEDD.Year < 2000)
                {
                    view.ClientStrings[i, 21] = " (now)";
                }
                else
                {
                    view.ClientStrings[i, 21] = nextEDD.ToString("MM/dd/yyyy");
                }

                var nextGCED = GetNextGiftCardEligibleDate(family, deliverySubList);
                if (nextGCED.Year < 2000)
                {
                    view.ClientStrings[i, 22] = " (now)";
                }
                else
                {
                    view.ClientStrings[i, 22] = nextGCED.ToString("MM/dd/yyyy");
                }

                view.ClientStrings[i, 23] = GetDeliveriesCountThisMonth(deliverySubList).ToString();
                view.ClientStrings[i, 24] = cli.Id.ToString();
            }

            return view;
        }

        private static int GetAge(DateTime dob)
        {
            TimeSpan span = DateTime.Now - dob;
            // Because we start at year 1 for the Gregorian
            // calendar, we must subtract a year here.
            int years = (DateTime.MinValue + span).Year - 1;
            return years;
        }

        private static string GetChildrenCount(List<FamilyMember> family)
        {
            var result = 0; // No Head of Household in Children count
            foreach (var mbr in family)
            {
                var age = GetAge(mbr.DateOfBirth);
                if (age <= 17)
                { result++; }
            }
            return result.ToString();
        }

        private static string GetAdultCount(List<FamilyMember> family)
        {
            var result = 0;
            foreach (var mbr in family)
            {
                var age = GetAge(mbr.DateOfBirth);
                if (age > 17 && age < 60)
                { result++; }
            }
            return result.ToString();
        }

        private static string GetSeniorCount(List<FamilyMember> family)
        {
            var result = 0;
            foreach (var mbr in family)
            {
                var age = GetAge(mbr.DateOfBirth);
                if (age >= 60)
                { result++; }
            }
            return result.ToString();
        }

        private static string GetAdultNamesAges(List<FamilyMember> family)
        {
            var strResult = "";
            foreach (var mbr in family)
            {

                var age = GetAge(mbr.DateOfBirth);
                if (age >= 18 && age < 60)
                {
                    if (strResult.Length != 0) strResult += "; ";
                    strResult += mbr.FirstName + " " + mbr.LastName + "/" + age;
                }
            }
            return strResult;
        }

        private static string GetSeniorNamesAges(List<FamilyMember> family)
        {
            var strResult = "";
            foreach (var mbr in family)
            {
                var age = GetAge(mbr.DateOfBirth);
                if (age >= 60)
                {
                    if (strResult.Length != 0) strResult += "; ";
                    strResult += mbr.FirstName + " " + mbr.LastName + "/" + age;
                }
            }
            return strResult;
        }

        private static string GetChildrenNamesAges(List<FamilyMember> family)
        {
            var strResult = "";
            foreach (var mbr in family)
            {
                var age = GetAge(mbr.DateOfBirth);
                if (age <= 17)
                {
                    if (strResult.Length != 0) strResult += "; ";
                    strResult += mbr.FirstName + " " + mbr.LastName + "/" + age;
                }
            }
            return strResult;
        }

        private static DateTime GetLastDeliveryDate(List<Delivery> deliverySubList)
        {
            var dt = DateTime.MinValue;
            var delivery = deliverySubList.OrderByDescending(d => d.DateDelivered).FirstOrDefault();
            if (delivery?.DateDelivered != null) return (DateTime)delivery.DateDelivered;
            return dt;
        }

        private static DateTime GetNextEligibleDeliveryDate(List<Delivery> deliverySubList)
        {
            var deliveriesThisMonth = GetDeliveriesCountThisMonth(deliverySubList);
            var lastDeliveryDate = GetLastDeliveryDate(deliverySubList);
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
                nextEligibleDate = new DateTime(nextEligibleDate.Year,
                    nextEligibleDate.Month, 1); // move it to 1st of next month, unless less than 7 days
                if (nextEligibleDate < lastDeliveryDate.AddDays(7))
                {
                    nextEligibleDate = lastDeliveryDate.AddDays(7);
                }
            }

            return nextEligibleDate;
        }

        private static int GetDeliveriesCountThisMonth(List<Delivery> deliverySubList)
        {
            var dt = DateTime.Today;
            var startDate = new DateTime(dt.Year, dt.Month, 1);
            var endDate = new DateTime(dt.Year, dt.Month, DateTime.DaysInMonth(dt.Year, dt.Month));

            var dtm = deliverySubList.Count(i => i.DateDelivered >= startDate
                                                 && i.DateDelivered <= endDate);
            return dtm;
        }

        private static DateTime GetDateLastGiftCard(List<Delivery> deliverySubList)
        {
            var delList = deliverySubList.Where(d => d.DateDelivered < DateTime.Today
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

        private static DateTime GetNextGiftCardEligibleDate(List<FamilyMember> family, List<Delivery> deliverySubList)
        {
            // GIFT CARDS ELIGIBLE:
            // 1 per household of 3 or fewer; 1 per household per calendar month max
            // 2 per household of 4 or more; 2 per household per calendar month max;
            var monthlyEligible = 0;
            var numberInHousehold = family.Count;
            var totalThisMonth = GetAllGiftCardsThisMonth(deliverySubList);
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

            var lastGiftCardDate = GetDateLastGiftCard(deliverySubList);
            var nextEligibleGiftCardDate = lastGiftCardDate.AddDays(7);
            if (monthlyEligible == 0)   // move eligibility to 1st of next month
            {
                nextEligibleGiftCardDate = nextEligibleGiftCardDate.AddMonths(1);
                nextEligibleGiftCardDate = new DateTime(nextEligibleGiftCardDate.Year,
                    nextEligibleGiftCardDate.Month, 1); // move it to next month
            }

            if (lastGiftCardDate == DateTime.MinValue) { lastGiftCardDate = DateTime.Today.AddMonths(-1); }
            var succeedingMonthDate = lastGiftCardDate.AddMonths(1);
            var firstOfSucceedingMonth = new DateTime(succeedingMonthDate.Year, succeedingMonthDate.Month, 1);
            if (lastGiftCardDate < firstOfSucceedingMonth)
            {
                nextEligibleGiftCardDate = firstOfSucceedingMonth;
            }

            return nextEligibleGiftCardDate;
        }

        private static int GetAllGiftCardsThisMonth(List<Delivery> deliverySubList)
        {
            var giftCardCount = 0;
            var delList = GetAllDeliveriesThisMonth(deliverySubList);
            foreach (var del in delList)
            {
                var cards = Convert.ToInt32(del.GiftCards);
                giftCardCount += cards;
            }
            return giftCardCount;
        }

        private static List<Delivery> GetAllDeliveriesThisMonth(List<Delivery> deliverySubList)
        {
            var dt = DateTime.Today;
            var startDate = new DateTime(dt.Year, dt.Month, 1);
            var endDate = new DateTime(dt.Year, dt.Month, DateTime.DaysInMonth(dt.Year, dt.Month));
            return deliverySubList.Where(i => i.DateDelivered >= startDate
                                              && i.DateDelivered <= endDate).ToList();
        }
        
        public static List<SelectListItem> GetAddressCheckSelectList()
        {
            using var db = new BHelpContext();
            var AddressCheckList = (from a in db.AddressChecks
                select new { Text = a.Address }).ToList();
            if (AddressCheckList.Count == 0) return null;

            var checkList = new List<SelectListItem>();
            foreach (var item in AddressCheckList)
            {
                // remove "Text =  {" & trailing " }"
                var _address = item.ToString().Substring(9);
                _address = _address.Substring(0, _address.Length - 2);
                var addItem = new SelectListItem()
                {
                    Value = "0", Text = _address
                };

                checkList.Add(addItem);
            }

            return checkList;
        }

    }
}