// Utilities used by developer //
using BHelp.DataAccessLayer;
using System;
using System.IO;
using BHelp.Models;
using LumenWorks.Framework.IO.Csv;
using System.Data;
using System.Linq;
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
                        delivery.LogDate = Convert.ToDateTime(row[0].ToString());
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
                            delivery.FullBags = null;
                        }

                        try
                        {
                            delivery.HalfBags = Convert.ToInt32(row[17]);
                        }
                        catch
                        {
                            delivery.HalfBags = null;
                        }

                        try
                        {
                            delivery.KidSnacks = Convert.ToInt32(row[18]);
                        }
                        catch
                        {
                            delivery.KidSnacks = null;
                        }

                        try
                        {
                            delivery.GiftCards = Convert.ToInt32(row[19]);
                        }
                        catch
                        {
                            delivery.GiftCards = null;
                        }

                        delivery.Completed = true;
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
                    delivery.Completed = true;
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
        private static void AddFamily(DataRow row,  string[] familyArray, int clientId)
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
                    //del.FirstDelivery = true;
                    //db.SaveChanges();
                }
            }
        }

        public static void SetStatusFlags()
        {
            var db = new BHelpContext();
            var listNullDelDates = db.Deliveries.Where(d => d.DateDelivered == null).ToList();
            foreach (var del in listNullDelDates)
            { // null & completed = undelivered;  null & not completed = open
                var delivery = db.Deliveries.Find(del.Id);
                if (delivery.Completed)
                {
                    delivery.Status = 2; // Undelivered
                    //db.SaveChanges();
                }
            }

            var listDelDates = db.Deliveries.Where(d => d.DateDelivered != null).ToList();
            foreach (var del in listDelDates)
            {
                var delivery = db.Deliveries.Find(del.Id);
                delivery.Status = 1;    // Delivered
                //db.SaveChanges();
            }
        }
    }
}
