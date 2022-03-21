// Utilities used by developer //
using BHelp.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.IO;
using BHelp.Models;
using LumenWorks.Framework.IO.Csv;
using System.Data;
using System.Linq;
using BHelp.ViewModels;
using DataTable = System.Data.DataTable;

namespace BHelp
{
    public static class Utilities
    {
        public static Boolean UploadClients()
        {
            var db = new BHelpContext();

            var filePath = @"c:\TEMP\BH Food Client List-Table 1.csv";

            DataTable csvtable = new DataTable();
            using (CsvReader csvReader =
                new CsvReader(new StreamReader(filePath), true))
            { csvtable.Load(csvReader); }

            foreach (DataRow row in csvtable.Rows)
            {
                Client client = new Client()
                {
                    Active = true,
                    LastName = row[0].ToString(),
                    FirstName = row[1].ToString(),
                    StreetNumber = row[2].ToString(),
                    StreetName = row[3].ToString(),
                    City = row[4].ToString(),
                    Zip = row[5].ToString(),
                    Phone = row[6].ToString(),
                    Notes = row[13].ToString()
                };

                db.Clients.Add(client);
                db.SaveChanges();
                //System.Diagnostics.Debug.WriteLine(client.FirstName, client.LastName);
                string _adults = row[10].ToString();
                string[] adultsArray = _adults.Split(',');
                AddFamily(row, adultsArray, client.Id); // contains newly added client Id
                string _kids = row[11].ToString();
                string[] kidsArray = _kids.Split(',');
                AddFamily(row, kidsArray, client.Id);
            }
            return true;
        }
        public static Boolean UploadDeliveries()
        {
            var db = new BHelpContext();
            var count = 0;
            var newCount = 0;
            //var filePath = @"c:\TEMP\CallLog0830-0903.csv";
            var filePath = AppDomain.CurrentDomain.BaseDirectory + "/App_Data/CallLog0830-0903.csv";
            DataTable csvtable = new DataTable();
            using (CsvReader csvReader = new CsvReader(new StreamReader(filePath), true))
            {
                csvtable.Load(csvReader);
            }

            var rowCount = 0;
            foreach (DataRow row in csvtable.Rows)
            {
                rowCount++;
                if (rowCount > 1) // Start with 08/16/2021
                {
                    Delivery delivery = new Delivery
                    {
                        ClientId = GetClientId(row[2].ToString(), row[4].ToString(), row[5].ToString())
                    };
                        // Total Food lbs =  row[22]. If = 0, delivery was not made.
                    if (IsDate(row[14].ToString()) && delivery.ClientId == 0)
                    {
                        newCount++;
                            //   // Create new client (with only head of household)
                            //    Client newClient = new Client()
                            //    {
                            //        Active=true,
                            //        LastName = row[2].ToString(),
                            //        FirstName = row[3].ToString(),
                            //        StreetNumber = row[4].ToString(),
                            //        StreetName = row[5].ToString(),
                            //        City = row[6].ToString(),
                            //        Zip = row[7].ToString(),
                            //        Phone = row[8].ToString(),
                            //        Notes = "Auto-added"
                            //    };
                            //    db.Clients.Add(newClient);
                            //    //db.SaveChanges();
                    }

                    if (delivery.ClientId != 0)
                    {
                        Client client = db.Clients.Find(delivery.ClientId);
                        // Create new delivery
                        count++;
                        delivery.DateDelivered = Convert.ToDateTime(row[0].ToString());
                        //delivery.DeliveryDate = null;
                        delivery.ODId = GetUserId(row[1].ToString());
                        try
                        {
                            delivery.Children = Convert.ToInt32(row[9]);
                        }
                        catch
                        {
                            delivery.Children = 0;
                        }

                        try
                        {
                            delivery.Adults = Convert.ToInt32(row[10]);
                        }
                        catch
                        {
                            delivery.Adults = 0;
                        }

                        try
                        {
                            delivery.Seniors = Convert.ToInt32(row[11]);
                        }
                        catch
                        {
                            delivery.Seniors = 0;
                        }

                        delivery.ODNotes = row[13].ToString();
                        try
                        {
                            delivery.DateDelivered = Convert.ToDateTime(row[14]); // can be null or empty
                        }
                        catch
                        {
                            //delivery.DateDelivered = null;
                        }

                        delivery.DriverId = GetUserId(row[15].ToString());
                        try
                        {
                            delivery.FullBags = Convert.ToInt32(row[16]);
                        }
                        catch
                        {
                            delivery.FullBags = 0;
                        }

                        try
                        {
                            delivery.HalfBags = Convert.ToInt32(row[17]);
                        }
                        catch
                        {
                            delivery.HalfBags = 0;
                        }

                        try
                        {
                            delivery.KidSnacks = Convert.ToInt32(row[18]);
                        }
                        catch
                        {
                            delivery.KidSnacks = 0;
                        }

                        try
                        {
                            delivery.GiftCards = Convert.ToInt32(row[19]);
                        }
                        catch
                        {
                            delivery.GiftCards = 0;
                        }
                        
                        if (client != null)
                        {
                            delivery.LastName = client.LastName;
                            delivery.FirstName = client.FirstName;
                            delivery.NamesAgesInHH = AppRoutines.GetNamesAgesOfAllInHousehold(client.Id);
                            delivery.Notes = row[20].ToString();
                            delivery.StreetNumber = client.StreetNumber;
                            delivery.StreetName = client.StreetName;
                            delivery.City = client.City;
                            delivery.Phone = client.Phone;
                            delivery.Zip = client.Zip;
                        }
                    }

                    db.Deliveries.Add(delivery);
                    db.SaveChanges();
                }
            }
            var unused = count.ToString() + " " + newCount.ToString();
            return true;
        }
        public static Boolean CopyClientZipToDelivery()
        {
            var db = new BHelpContext();
            var deliveries = db.Deliveries.ToList();
            foreach (var delivery in deliveries)
            {
                var client = db.Clients.Find(delivery.ClientId);
                if (client != null) delivery.Zip = client.Zip;
            }
            //db.SaveChanges();
            return true;
        }
        public static Boolean CopySnapshotDataToDelivery()
        {
            var db = new BHelpContext();
            var deliveries = db.Deliveries.ToList();
            foreach (var delivery in deliveries)
            {
                var client = db.Clients.Find(delivery.ClientId);
                if (client != null)
                {
                    delivery.FirstName = client.FirstName;
                    delivery.LastName = client.LastName;
                    delivery.StreetNumber = client.StreetNumber;
                    delivery.StreetName = client.StreetName;
                    delivery.City = client.City;
                    delivery.Phone = client.Phone;
                    delivery.NamesAgesInHH = AppRoutines.GetNamesAgesOfAllInHousehold(client.Id);
                }
            }
            //db.SaveChanges();
            return true;
        }
        private static int GetClientId(string lastName, string streetNumber, string streetName)
        {
            var db = new BHelpContext();
            var client= db.Clients.FirstOrDefault(c => c.StreetNumber == streetNumber
                                                       && c.StreetName == streetName);
            if (client != null)
            {
                return client.Id;
            }
            else
            {
                client = db.Clients.FirstOrDefault(c => c.LastName == lastName && c.StreetNumber == streetNumber);
                if (client != null)
                {
                    return client.Id;
                }
                else
                {
                    return 0;
                }
            }
        }
        private static bool IsDate(string inputDate)
        {
            // ReSharper disable once NotAccessedVariable
            DateTime dat;
            return DateTime.TryParse(inputDate, out dat);
        }
        private static string GetUserId(string fullName)
        {
            if (fullName.Length < 5) { return ""; } //(Expect driver with only first name 'Jake')
            string[] names = fullName.Split( ' ');
            string firstName = names[0];
            string lastName = names[1];
                var db = new BHelpContext();
            var user = db.Users.FirstOrDefault(u => u.FirstName == firstName
                                                    && u.LastName == lastName);
            if (user != null)
            {
                return user.Id;
            }
            else
            {
                return "";
            }
        }
        private static void AddFamily(DataRow row, string[] familyArray, int clientId)
        {
            var db = new BHelpContext();
            foreach (var nameAge in familyArray)
            {
                if (clientId > 0)  // to reset if errors occured
                {
                    if (nameAge.Contains(row[0].ToString()) && nameAge.Contains(row[1].ToString())) // First and Last Name))
                    {
                        //Don't add to FamilyMembers - just capture the age & update the client DoB
                        int ageIndex = nameAge.IndexOf('/');
                        if (ageIndex > 0)
                        {
                            var years = nameAge.Substring(nameAge.IndexOf('/') + 1);
                            //System.Diagnostics.Debug.WriteLine(i.ToString() + ' ' + row[0].ToString() + ' ' + row[1].ToString() + ' ' + years);

                            if (int.TryParse(years, out int yy))
                            {
                                // Everybody born on August 13th
                                DateTime doB = new DateTime(2021 - yy, 8, 13);
                                var original = db.Clients.Find(clientId);
                                if (original != null)
                                {
                                    original.DateOfBirth = doB;
                                    db.SaveChanges();
                                }
                            }
                        }
                    }
                    else // not a client - add a family member =================
                    {
                        string nameAgeTrim = nameAge.Trim();
                        var years = nameAge.Substring(nameAge.IndexOf('/') + 1);
                        if (int.TryParse(years, out int yy))
                        {
                            DateTime doB = new DateTime(2021 - yy, 4, 1);
                            string fullName = nameAgeTrim.Substring(0, nameAgeTrim.IndexOf('/'));
                            string firstName;
                            string lastName;
                            if (fullName.IndexOf(' ') <= 0)
                            { firstName = fullName; lastName = ""; }
                            else
                            {
                                firstName = fullName.Substring(0, fullName.IndexOf(' '));
                                lastName = fullName.Substring(fullName.IndexOf(' ') + 1);
                            }

                            FamilyMember newMember = new FamilyMember()
                            {
                                Active = true,
                                ClientId = clientId,  // newly added client Id
                                FirstName = firstName,
                                LastName = lastName,
                                DateOfBirth = doB
                            };
                            db.FamilyMembers.Add(newMember);
                            db.SaveChanges();
                        }
                    }
                }
            }
        }
        public static void SetFirstDeliveries()
        {
            var db = new BHelpContext();
            var listPost06302021Clients = db.Clients.Where(c => c.Id > 1588).ToList();
            foreach (Client client in listPost06302021Clients)
            {
                var delCount = db.Deliveries.Count(d => d.ClientId == client.Id);
                if (delCount > 0)   // Get first delivery
                {
                    var del = db.Deliveries.First(d => d.ClientId == client.Id);
                    del.FirstDelivery = true;
                    //db.SaveChanges();
                }
            }
        }
        public static void SetStatusFlags()
        {
        }

        public static void GetLatestDeliveries()
        {
            //var db = new BHelpContext();
            //var result = (from pi in db.Clients 
            //    join pu in db.Deliveries  on pi.Id equals pu.ClientId  into tpu
            //    from t in tpu.OrderByDescending( c => c.DateDelivered).Take(1)
            //    select new { pi.Id , pi.LastName, t.DateDelivered}).ToList();

            //var sql = "select c.Id, c.LastName,c.FirstName, COUNT(d.Id) as total "
            //          + "from Clients as c left join Deliveries as d on c.Id = d.ClientId "
            //          + "group by c.Id, c.LastName, c.FirstName order by total";
            //var results = db.Database.ExecuteSqlCommand(sql);

            //: 'The data reader has more than one field. Multiple fields are not valid for EDM primitive or enumeration types.'
            //var sql = "select c.LastName, c.FirstName, c.Id, latest_deliveries.DateDelivered "
            //          + "from(select ClientId, MAX(DateDelivered) as DateDelivered "
            //          + "from dbo.[Deliveries] d  where d.Status = 1 Group By ClientId) "
            //          + "as latest_deliveries "
            //          + "inner join dbo.[Clients] c on c.Id = latest_deliveries.ClientId "
            //          + "where c.Active = 1 "
            //          + "order by latest_deliveries.DateDelivered";
            //var success = db.Database.SqlQuery<string>(sql).ToList();

            //var results = (from c in db.Clients
            //    join d in db.Deliveries on c.Id equals d.ClientId into cd
            //    from t in cd.OrderBy(x => x.DateDelivered).FirstOrDefault()
            //    select new
            //    {
            //        c.Id, DateDelivered = t.DateDelivered,
            //        LastName = c.LastName, FirstName = c.FirstName
            //    }).ToList();

            //var delRecs = results
            //    .OrderBy(d => d.DateDelivered);

            //foreach (var item in results )
            //{
            //    var dt = item.DateDelivered ?? DateTime.Now ;
            //    DeliveryViewModel del = new DeliveryViewModel()
            //    {
            //        DateDeliveredString = dt .ToString("MM/dd/yyyy"),
            //        LastName = item.LastName ,
            //        FirstName =item.FirstName
            //    };
            //}

            //var clientRecIds = db.Clients.Where(a => a.Active)
            //    .Select(i => i.Id).ToList();

            //var delList = new List<DeliveryViewModel>();
            //foreach (var del in results)
            //{
            //    if (clientRecIds.Contains(del.Id))
            //    {
            //        var newDel = new DeliveryViewModel()
            //        {
            //            ClientId = del.Id,
            //            FirstName = del.FirstName 
            //        };
            //        delList.Add(newDel);
            //    }
            //}

            //var deliveryClientIds = db.Deliveries
            //    .Where(s => s.Status == 1)
            //    .GroupBy(d => d.ClientId)
            //    .Select(g => g.OrderBy(d => d.DateDelivered)
            //        .FirstOrDefault())
            //    .Select(i => i.ClientId).ToList();

            //var clientsWithDeliveries = new List<Client>();
            //var clientsWithNoDeliveries = new List<Client>();
            //foreach (var client in db.Clients)
            //{
            //    if (deliveryClientIds.Contains(client.Id))
            //    {
            //        clientsWithDeliveries.Add(client);
            //    }
            //    else
            //    {
            //        clientsWithNoDeliveries.Add(client);
            //    }
            //}

            //var dummy = "";
        }

        public static ClientViewModel GetAllClientsListModel()
        {
            var view = new ClientViewModel { ReportTitle = "BH Food Client List" };
            const int columns = 22;

            var db = new BHelpContext();
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
                var family = familyList
                    .Where(f => f.ClientId == cli.Id).ToList();
                var hH = new FamilyMember() // add HeadOfHousehold:
                    { FirstName = cli.FirstName, LastName = cli.LastName, DateOfBirth = cli.DateOfBirth  };
                family.Add(hH);
                view.ClientStrings[i, 10] = GetChildrenCount(family);
                view.ClientStrings[i, 11] = GetAdultCount(family);
                view.ClientStrings[i, 12] = GetSeniorCount(family);
                view.ClientStrings[i, 13] = GetAdultNamesAges(family);
                view.ClientStrings[i, 14] = GetSeniorNamesAges(family); // !!! added row
                view.ClientStrings[i, 14] = GetChildrenNamesAges(family);
                view.ClientStrings[i, 15] = family.Count.ToString();
                view.ClientStrings[i, 16] = cli.Notes;
                var deliverySubList = deliveryList
                    .Where(d => d.ClientId == cli.Id && d.Status == 1
                                                     && d.DateDelivered != null).ToList();
                var lastDD = GetLastDeliveryDate(deliverySubList);
                if (lastDD.Year < 2000)
                {
                    view.ClientStrings[i, 17] = " - - ";
                }
                else
                {
                    view.ClientStrings[i, 17] = lastDD.ToString("MM/dd/yyyy");
                }

                var lastGC = GetDateLastGiftCard(deliverySubList);
                if (lastGC.Year < 2000)
                {
                    view.ClientStrings[i, 18] = " - - ";
                }
                else
                {
                    view.ClientStrings[i, 18] = lastGC.ToString("MM/dd/yyyy");
                }

                var nextEDD = GetNextEligibleDeliveryDate(deliverySubList);
                if (nextEDD.Year < 2000)
                {
                    view.ClientStrings[i, 19] = " (now)";
                }
                else
                {
                    view.ClientStrings[i, 19] = nextEDD.ToString("MM/dd/yyyy");
                }

                var nextGCED = GetNextGiftCardEligibleDate(family, deliverySubList);
                if (nextGCED.Year < 2000)
                {
                    view.ClientStrings[i, 20] = " (now)";
                }
                else
                {
                    view.ClientStrings[i, 20] = nextGCED.ToString("MM/dd/yyyy");
                    view.ClientStrings[i, 21] = GetDeliveriesCountThisMonth(deliverySubList).ToString();
                    view.ClientStrings[i, 22] = cli.Id.ToString();
                }
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

        private static string GetChildrenCount( List<FamilyMember> family)
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

        private static string GetSeniorCount( List<FamilyMember> family)
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
                if (age >= 18 && age <60)
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
            { var age = GetAge(mbr.DateOfBirth);
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
                    if (strResult.Length != 0)  strResult += "; "; 
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
            var numberInHousehold =family.Count;
            var totalThisMonth = GetAllGiftCardsThisMonth(deliverySubList );
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
           
    }
}

