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
    public class Utilities
    {
        public static Boolean UploadClients()
        {
            var db = new BHelpContext();

            var filePath = @"c:\TEMP\BH Food Client List-Table 1.csv";

            DataTable csvtable = new DataTable();
            using (CsvReader csvReader =
                new CsvReader(new StreamReader(filePath), true))
            {
                csvtable.Load(csvReader);
            }

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
            }

            return true;
        }

        public static Boolean UploadAdults()
        {
            var db = new BHelpContext();

            var filePath = @"c:\TEMP\BH Food Client List-Table 1.csv";

            DataTable csvtable = new DataTable();
            using (CsvReader csvReader = new CsvReader(new StreamReader(filePath), true))
            { csvtable.Load(csvReader); }

            int i = 0;
            foreach (DataRow row in csvtable.Rows)
            { 
                i ++;   // Switch for  Kids / Adults:
                string _adults = row[10].ToString(); // Adults  
                //string _adults = row[11].ToString();  // Kids
                string[] adultsArray = _adults.Split(',');
                foreach (var nameAge in adultsArray)
                {
                    if (i > 0)  // to reset if errors occured
                    {
                        if (nameAge.Contains(row[0].ToString() ) && nameAge.Contains(row[1].ToString())) // First and Last Name))
                        {
                            //Don't add to FamilyMembers - just capture the age & update the client DoB
                            int ageIndex = nameAge.IndexOf('/');
                            if (ageIndex > 0)
                            {
                                var years = nameAge.Substring(nameAge.IndexOf('/') +1);
                                //System.Diagnostics.Debug.WriteLine(i.ToString() + ' ' + row[0].ToString() + ' ' + row[1].ToString() + ' ' + years);
                              
                              if (int.TryParse(years, out int yy))
                              {
                                  // Everybody born on April 1st
                                  DateTime doB = new DateTime(2021-yy, 4, 1);
                                  var original = db.Clients.Find(i);
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
                                {
                                    firstName = fullName;
                                    lastName = "";
                                }
                                else
                                {
                                    firstName = fullName.Substring(0, fullName.IndexOf(' '));
                                    lastName = fullName.Substring(fullName.IndexOf(' ') + 1);
                                }

                                FamilyMember newAdult = new FamilyMember()
                                {
                                    Active = true,
                                    ClientId = i,
                                    FirstName = firstName,
                                    LastName = lastName,
                                    DateOfBirth = doB
                                };
                                db.FamilyMembers.Add(newAdult);
                                db.SaveChanges();
                            }
                        }
                    }
                }
                //System.Diagnostics.Debug.WriteLine(client.FirstName, client.LastName);
            }
            return true;
        }

        public static Boolean ReverseNames() // Initial load had First & Last Names revesed
        {
            var db = new BHelpContext();
            foreach (Client client in db.Clients.ToList())
            {
                var oldLastName = client.LastName;
                client.LastName = client.FirstName;
                client.FirstName = oldLastName;
                db.SaveChanges();
            }

            return true;
        }

        public static Boolean UploadDeliveries()
        {
            var db = new BHelpContext();
            var count1 = 0;
            var count2 = 0;
            var filePath = @"c:\TEMP\BH Call Log - April 2021.csv";
            DataTable csvtable = new DataTable();
            using (CsvReader csvReader = new CsvReader(new StreamReader(filePath), true))
            { csvtable.Load(csvReader); }

            foreach (DataRow row in csvtable.Rows)
            {
                Delivery delivery = new Delivery();
                delivery.ClientId = GetClientId(row[2].ToString(), row[3].ToString());
                if (IsDate(row[15].ToString()) && delivery.ClientId == 0)
                {
                   // Create new client (with only head of household)
                    count2++;
                    Client client = new Client()
                    {
                        Active=true,
                        LastName = row[2].ToString(),
                        FirstName = row[3].ToString(),
                        StreetNumber = row[4].ToString(),
                        StreetName = row[5].ToString(),
                        City = row[6].ToString(),
                        Zip = row[7].ToString(),
                        Phone = row[8].ToString(),
                        Notes = "Auto-added"
                    };
                    //db.Clients.Add(client);
                    //db.SaveChanges();
                }

                if (IsDate(row[15].ToString()) && delivery.ClientId != 0)
                {
                    // Create new delivery
                    count1++;
                    delivery.DeliveryDate = Convert.ToDateTime(row[0].ToString());
                    delivery.ODId = GetUserId(row[1].ToString());
                    try { delivery.Children = Convert.ToInt32(row[9]); }
                    catch { delivery.Children = 0;}
                    try { delivery.Adults = Convert.ToInt32(row[10]); }
                    catch { delivery.Adults = 0;}
                    try { delivery.Seniors = Convert.ToInt32(row[11]); }
                    catch { delivery.Seniors = 0; }
                    delivery.DriverId = GetUserId(row[16].ToString());
                    delivery.ODNotes = row[14].ToString();
                    delivery.DateDelivered = Convert.ToDateTime(row[15]);
                    delivery.FullBags = Convert.ToInt32(row[17]);
                    delivery.HalfBags = Convert.ToInt32(row[18]);
                    delivery.KidSnacks = Convert.ToInt32(row[19]);
                    delivery.GiftCards = Convert.ToInt32(row[20]);
                    delivery.Notes = row[21].ToString();
                    //db.Deliveries.Add(delivery);
                    //db.SaveChanges();
                }
            }
            return true;
        }

        private static int GetClientId(string lastName, string firstName)
        {
            var db = new BHelpContext();
            var client= db.Clients.FirstOrDefault(c => c.LastName == lastName 
                                                       && c.FirstName == firstName);
            if (client != null)
            {
                return client.Id;
            }
            else
            {
                return 0;
            }
        }

        private static bool IsDate(string inputDate)
        {
            DateTime dat;
            return DateTime.TryParse(inputDate, out dat);
            //bool isDate = true;
            //try
            //{
            //    DateTime dt = DateTime.Parse(inputDate);
            //}
            //catch
            //{
            //    isDate = false;
            //}
            //return isDate;
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
    }
}