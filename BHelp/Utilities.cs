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
                    FirstName = row[0].ToString(),
                    LastName = row[1].ToString(),
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
                i ++;
                //string _adults = row[10].ToString(); // Adults  
                string _adults = row[11].ToString();  // Kids
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

        public static Boolean ReverseNames()
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
    }
}